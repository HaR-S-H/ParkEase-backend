using AuthService.Models.Dtos;
using Microsoft.AspNetCore.Http;

namespace AuthService.Services
{
    public interface IAuthService
    {
        Task<RegisterResponse> Register(RegisterRequest request);
        Task<LoginResponse> Login(LoginRequest request);
        Task<LoginResponse> GoogleLogin(GoogleLoginRequest request);
        Task Logout(int userId);
        Task<LoginResponse> RefreshToken(string refreshToken);
        Task VerifyEmail(VerifyEmailRequest request);
        Task ResendVerification(ResendVerificationRequest request);
        Task ForgotPassword(ForgotPasswordRequest request);
        Task<UserResponse?> GetUserByEmail(string email);
        Task<UserResponse?> GetUserById(int userId);
        Task<UserResponse> UpdateAccount(int userId, UpdateAccountRequest request);
        Task DeactivateAccount(int userId);
    }
}
