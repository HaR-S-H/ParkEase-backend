using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Security.Claims;
using System.Text;
using AuthService.Data;
using AuthService.Helpers.Implementations;
using AuthService.Helpers.Interfaces;
using AuthService.Messaging;
using AuthService.Messaging.Messages;
using AuthService.Models;
using AuthService.Models.Dtos;
using AuthService.Repositories;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.Services
{
    public class AuthService : IAuthService
    {
        private static readonly string[] AllowedRoles = ["DRIVER", "MANAGER", "ADMIN"];
        private readonly IUserRepository _userRepository;
        private readonly IJwtService _jwtService;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        private readonly IRabbitMqPublisher _rabbitMqPublisher;
        private readonly IGoogleAuthService _googleAuthService;
        private readonly AuthDbContext _dbContext;

        public AuthService(
            IUserRepository userRepository,
            IJwtService jwtService,
            IPasswordHasher passwordHasher,
            IConfiguration configuration,
            IMapper mapper,
            IRabbitMqPublisher rabbitMqPublisher,
            IGoogleAuthService googleAuthService,
            AuthDbContext dbContext)
        {
            _userRepository = userRepository;
            _jwtService = jwtService;
            _passwordHasher = passwordHasher;
            _configuration = configuration;
            _mapper = mapper;
            _rabbitMqPublisher = rabbitMqPublisher;
            _googleAuthService = googleAuthService;
            _dbContext = dbContext;
        }

        public async Task<RegisterResponse> Register(RegisterRequest request)
        {
            var normalizedEmail = request.Email.Trim().ToLowerInvariant();
            var role = request.Role.Trim().ToUpperInvariant();

            if (!AllowedRoles.Contains(role))
            {
                throw new AppException("Role must be one of DRIVER, MANAGER, or ADMIN.", StatusCodes.Status400BadRequest);
            }

            if (await _userRepository.ExistsByEmail(normalizedEmail))
            {
                throw new AppException("Email already exists.", StatusCodes.Status409Conflict);
            }

            var phoneOwner = await _userRepository.FindByPhone(request.Phone.Trim());
            if (phoneOwner != null)
            {
                throw new AppException("Phone already exists.", StatusCodes.Status409Conflict);
            }

            var user = _mapper.Map<User>(request);
            user.Email = normalizedEmail;
            user.PasswordHash = _passwordHasher.HashPassword(request.Password);
            user.Phone = request.Phone.Trim();
            user.Role = role;
            user.VehiclePlate = request.VehiclePlate.Trim().ToUpperInvariant();
            user.EmailVerified = false;
            user.EmailVerificationToken = CreateVerificationToken();
            user.EmailVerificationTokenExpiresAt = DateTime.UtcNow.AddHours(24);
            user.IsActive = true;
            user.CreatedAt = DateTime.UtcNow;

            var created = await _userRepository.Create(user);

            // Handle profile picture upload if provided
            if (request.ProfilePic != null && request.ProfilePic.Length > 0)
            {
                await using var stream = request.ProfilePic.OpenReadStream();
                using var memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream);

                var uploadMessage = new ProfilePictureUploadRequestedMessage
                {
                    UserId = created.UserId,
                    FileName = request.ProfilePic.FileName,
                    ContentType = string.IsNullOrWhiteSpace(request.ProfilePic.ContentType) ? "application/octet-stream" : request.ProfilePic.ContentType,
                    Base64Content = Convert.ToBase64String(memoryStream.ToArray())
                };

                await _rabbitMqPublisher.Publish(GetPictureUploadQueue(), uploadMessage);
            }

            await PublishVerificationEmail(created.Email, created.FullName, created.EmailVerificationToken!);

            return new RegisterResponse
            {
                Message = "Registration successful. Please verify your email before logging in.",
                User = _mapper.Map<UserResponse>(created)
            };
        }

        public async Task<LoginResponse> Login(LoginRequest request)
        {
            var normalizedEmail = request.Email.Trim().ToLowerInvariant();
            var user = await _userRepository.FindByEmail(normalizedEmail);

            if (user == null || !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
            {
                throw new AppException("Invalid email or password.", StatusCodes.Status401Unauthorized);
            }

            if (!user.IsActive)
            {
                throw new AppException("Account is deactivated.", StatusCodes.Status403Forbidden);
            }

            if (!user.EmailVerified)
            {
                throw new AppException("Email is not verified.", StatusCodes.Status403Forbidden);
            }

            return await BuildAuthResponse(user);
        }

        public async Task<LoginResponse> GoogleLogin(GoogleLoginRequest request)
        {
            var payload = await _googleAuthService.ValidateIdToken(request.IdToken);
            var email = payload.Email?.Trim().ToLowerInvariant();

            if (string.IsNullOrWhiteSpace(email))
            {
                throw new AppException("Google token did not contain an email address.", StatusCodes.Status400BadRequest);
            }

            var user = await _userRepository.FindByEmail(email);

            if (user == null)
            {
                var role = NormalizeRole(request.Role);
                var phone = string.IsNullOrWhiteSpace(request.Phone)
                    ? $"GOOGLE-{Guid.NewGuid():N}"[..20]
                    : request.Phone.Trim();

                var phoneOwner = await _userRepository.FindByPhone(phone);
                if (phoneOwner != null)
                {
                    phone = $"GOOGLE-{Guid.NewGuid():N}"[..20];
                }

                user = new User
                {
                    FullName = payload.Name ?? email,
                    Email = email,
                    PasswordHash = _passwordHasher.HashPassword(Guid.NewGuid().ToString("N")),
                    Phone = phone,
                    Role = role,
                    VehiclePlate = string.IsNullOrWhiteSpace(request.VehiclePlate)
                        ? "UNASSIGNED"
                        : request.VehiclePlate.Trim().ToUpperInvariant(),
                    EmailVerified = true,
                    EmailVerificationToken = null,
                    EmailVerificationTokenExpiresAt = null,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    ProfilePicUrl = payload.Picture
                };

                user = await _userRepository.Create(user);
            }

            if (!user.IsActive)
            {
                throw new AppException("Account is deactivated.", StatusCodes.Status403Forbidden);
            }

            if (!user.EmailVerified)
            {
                user.EmailVerified = true;
                user.EmailVerificationToken = null;
                user.EmailVerificationTokenExpiresAt = null;
                await _userRepository.Update(user);
            }

            return await BuildAuthResponse(user);
        }

        public async Task Logout(int userId)
        {
            var activeTokens = await _dbContext.RefreshTokens
                .Where(t => t.UserId == userId && t.RevokedAt == null)
                .ToListAsync();

            foreach (var token in activeTokens)
            {
                token.RevokedAt = DateTime.UtcNow;
            }

            if (activeTokens.Count > 0)
            {
                await _dbContext.SaveChangesAsync();
            }
        }

        public bool ValidateToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(GetJwtValue("Jwt:Key"));

            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = GetJwtValue("Jwt:Issuer"),
                    ValidateAudience = true,
                    ValidAudience = GetJwtValue("Jwt:Audience"),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(1)
                }, out _);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<LoginResponse> RefreshToken(string refreshToken)
        {
            var tokenEntry = await _dbContext.RefreshTokens
                .FirstOrDefaultAsync(t => t.Token == refreshToken && t.RevokedAt == null);

            if (tokenEntry == null)
            {
                throw new AppException("Invalid refresh token.", StatusCodes.Status401Unauthorized);
            }

            if (tokenEntry.ExpiresAt <= DateTime.UtcNow)
            {
                tokenEntry.RevokedAt = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync();
                throw new AppException("Refresh token expired.", StatusCodes.Status401Unauthorized);
            }

            var user = await _userRepository.FindByUserId(tokenEntry.UserId);
            if (user == null || !user.IsActive)
            {
                tokenEntry.RevokedAt = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync();
                throw new AppException("User not found or inactive.", StatusCodes.Status401Unauthorized);
            }

            tokenEntry.RevokedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();

            return await BuildAuthResponse(user);
        }

        public async Task VerifyEmail(VerifyEmailRequest request)
        {
            var email = request.Email.Trim().ToLowerInvariant();
            var user = await _userRepository.FindByEmail(email)
                ?? throw new AppException("User not found.", StatusCodes.Status404NotFound);

            if (user.EmailVerified)
            {
                return;
            }

            if (user.EmailVerificationToken == null || user.EmailVerificationTokenExpiresAt == null)
            {
                throw new AppException("No verification token found. Request a new one.", StatusCodes.Status400BadRequest);
            }

            if (!string.Equals(user.EmailVerificationToken, request.Token, StringComparison.Ordinal))
            {
                throw new AppException("Invalid verification token.", StatusCodes.Status400BadRequest);
            }

            if (user.EmailVerificationTokenExpiresAt <= DateTime.UtcNow)
            {
                throw new AppException("Verification token expired.", StatusCodes.Status400BadRequest);
            }

            user.EmailVerified = true;
            user.EmailVerificationToken = null;
            user.EmailVerificationTokenExpiresAt = null;
            await _userRepository.Update(user);
        }

        public async Task ResendVerification(ResendVerificationRequest request)
        {
            var email = request.Email.Trim().ToLowerInvariant();
            var user = await _userRepository.FindByEmail(email)
                ?? throw new AppException("User not found.", StatusCodes.Status404NotFound);

            if (user.EmailVerified)
            {
                return;
            }

            user.EmailVerificationToken = CreateVerificationToken();
            user.EmailVerificationTokenExpiresAt = DateTime.UtcNow.AddHours(24);
            await _userRepository.Update(user);

            await PublishVerificationEmail(user.Email, user.FullName, user.EmailVerificationToken);
        }

        public async Task ForgotPassword(ForgotPasswordRequest request)
        {
            var email = request.Email.Trim().ToLowerInvariant();
            var user = await _userRepository.FindByEmail(email)
                ?? throw new AppException("User not found.", StatusCodes.Status404NotFound);

            if (!user.IsActive)
            {
                throw new AppException("Account is deactivated.", StatusCodes.Status403Forbidden);
            }

            var temporaryPassword = GenerateTemporaryPassword(12);
            user.PasswordHash = _passwordHasher.HashPassword(temporaryPassword);
            await _userRepository.Update(user);

            // Revoke active refresh tokens after forced password reset.
            var activeTokens = await _dbContext.RefreshTokens
                .Where(t => t.UserId == user.UserId && t.RevokedAt == null)
                .ToListAsync();

            foreach (var token in activeTokens)
            {
                token.RevokedAt = DateTime.UtcNow;
            }

            if (activeTokens.Count > 0)
            {
                await _dbContext.SaveChangesAsync();
            }

            await PublishForgotPasswordEmail(user.Email, user.FullName, temporaryPassword);
        }

        public async Task<UserResponse?> GetUserByEmail(string email)
        {
            var user = await _userRepository.FindByEmail(email.Trim().ToLowerInvariant());
            return user == null ? null : _mapper.Map<UserResponse>(user);
        }

        public async Task<UserResponse?> GetUserById(int userId)
        {
            var user = await _userRepository.FindByUserId(userId);
            return user == null ? null : _mapper.Map<UserResponse>(user);
        }

        public async Task<UserResponse> UpdateAccount(int userId, UpdateAccountRequest request)
        {
            var user = await _userRepository.FindByUserId(userId)
                ?? throw new AppException("User not found.", StatusCodes.Status404NotFound);

            if (!user.IsActive)
            {
                throw new AppException("Account is deactivated.", StatusCodes.Status403Forbidden);
            }

            if (!string.IsNullOrWhiteSpace(request.Phone))
            {
                var trimmedPhone = request.Phone.Trim();
                var existingByPhone = await _userRepository.FindByPhone(trimmedPhone);
                if (existingByPhone != null && existingByPhone.UserId != userId)
                {
                    throw new AppException("Phone already exists.", StatusCodes.Status409Conflict);
                }

                user.Phone = trimmedPhone;
            }

            if (!string.IsNullOrWhiteSpace(request.FullName))
            {
                user.FullName = request.FullName.Trim();
            }

            if (!string.IsNullOrWhiteSpace(request.VehiclePlate))
            {
                user.VehiclePlate = request.VehiclePlate.Trim().ToUpperInvariant();
            }

            if (!string.IsNullOrWhiteSpace(request.CurrentPassword)
                || !string.IsNullOrWhiteSpace(request.NewPassword))
            {
                if (string.IsNullOrWhiteSpace(request.CurrentPassword)
                    || string.IsNullOrWhiteSpace(request.NewPassword))
                {
                    throw new AppException("Both currentPassword and newPassword are required for password change.", StatusCodes.Status400BadRequest);
                }

                if (!_passwordHasher.VerifyPassword(request.CurrentPassword, user.PasswordHash))
                {
                    throw new AppException("Current password is incorrect.", StatusCodes.Status400BadRequest);
                }

                if (request.NewPassword.Length < 6)
                {
                    throw new AppException("New password must be at least 6 characters.", StatusCodes.Status400BadRequest);
                }

                user.PasswordHash = _passwordHasher.HashPassword(request.NewPassword);
            }

            if (request.ProfilePic != null)
            {
                if (request.ProfilePic.Length == 0)
                {
                    throw new AppException("Profile picture file is empty.", StatusCodes.Status400BadRequest);
                }

                await using var stream = request.ProfilePic.OpenReadStream();
                using var memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream);

                var uploadMessage = new ProfilePictureUploadRequestedMessage
                {
                    UserId = userId,
                    FileName = request.ProfilePic.FileName,
                    ContentType = string.IsNullOrWhiteSpace(request.ProfilePic.ContentType) ? "application/octet-stream" : request.ProfilePic.ContentType,
                    Base64Content = Convert.ToBase64String(memoryStream.ToArray())
                };

                await _rabbitMqPublisher.Publish(GetPictureUploadQueue(), uploadMessage);
            }

            await _userRepository.Update(user);
            return _mapper.Map<UserResponse>(user);
        }

        public async Task DeactivateAccount(int userId)
        {
            var user = await _userRepository.FindByUserId(userId)
                ?? throw new AppException("User not found.", StatusCodes.Status404NotFound);

            user.IsActive = false;
            await _userRepository.Update(user);
            await Logout(userId);
        }

        private async Task<LoginResponse> BuildAuthResponse(User user)
        {
            var token = _jwtService.GenerateToken(user.UserId, user.Email, user.Role);
            var refreshToken = Guid.NewGuid().ToString("N");
            var refreshExpiry = DateTime.UtcNow.AddDays(7);

            _dbContext.RefreshTokens.Add(new RefreshToken
            {
                UserId = user.UserId,
                Token = refreshToken,
                ExpiresAt = refreshExpiry,
                CreatedAt = DateTime.UtcNow,
                RevokedAt = null
            });
            await _dbContext.SaveChangesAsync();

            return new LoginResponse
            {
                Token = token,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddHours(3),
                User = _mapper.Map<UserResponse>(user)
            };
        }

        private string GetJwtValue(string key)
        {
            return _configuration[key] ?? throw new InvalidOperationException($"Missing configuration key: {key}");
        }

        private static string NormalizeRole(string? role)
        {
            if (string.IsNullOrWhiteSpace(role))
            {
                return "DRIVER";
            }

            var normalized = role.Trim().ToUpperInvariant();
            return AllowedRoles.Contains(normalized) ? normalized : "DRIVER";
        }

        private static string CreateVerificationToken()
        {
            return Convert.ToBase64String(Guid.NewGuid().ToByteArray())
                .Replace("+", string.Empty, StringComparison.Ordinal)
                .Replace("/", string.Empty, StringComparison.Ordinal)
                .Replace("=", string.Empty, StringComparison.Ordinal);
        }

        private static string GenerateTemporaryPassword(int length)
        {
            const string charset = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789!@#$%^&*";
            var bytes = RandomNumberGenerator.GetBytes(length);
            var chars = new char[length];

            for (var i = 0; i < length; i++)
            {
                chars[i] = charset[bytes[i] % charset.Length];
            }

            return new string(chars);
        }

        private Task PublishVerificationEmail(string email, string fullName, string token)
        {
            var message = new EmailVerificationRequestedMessage
            {
                Email = email,
                FullName = fullName,
                Token = token
            };

            return _rabbitMqPublisher.Publish(GetEmailQueue(), message);
        }

        private Task PublishForgotPasswordEmail(string email, string fullName, string temporaryPassword)
        {
            var message = new ForgotPasswordRequestedMessage
            {
                Email = email,
                FullName = fullName,
                TemporaryPassword = temporaryPassword
            };

            return _rabbitMqPublisher.Publish(GetForgotPasswordQueue(), message);
        }

        private string GetEmailQueue()
        {
            return _configuration["RabbitMQ:Queues:EmailVerification"]
                ?? throw new InvalidOperationException("Missing RabbitMQ:Queues:EmailVerification.");
        }

        private string GetPictureUploadQueue()
        {
            return _configuration["RabbitMQ:Queues:ProfilePictureUpload"]
                ?? throw new InvalidOperationException("Missing RabbitMQ:Queues:ProfilePictureUpload.");
        }

        private string GetForgotPasswordQueue()
        {
            return _configuration["RabbitMQ:Queues:ForgotPassword"]
                ?? throw new InvalidOperationException("Missing RabbitMQ:Queues:ForgotPassword.");
        }
    }
}
