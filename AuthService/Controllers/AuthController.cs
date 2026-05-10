using System.Security.Claims;
using AuthService.Models.Dtos;
using AuthService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Controllers
{
    [ApiController]
    [Route("api/v1/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Register([FromForm] RegisterRequest request)
        {
            var response = await _authService.Register(request);
            return Ok(response);
        }

        [HttpPost("google-login")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request)
        {
            var response = await _authService.GoogleLogin(request);
            SetAuthCookies(response);
            return Ok(response);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var response = await _authService.Login(request);
            SetAuthCookies(response);
            return Ok(response);
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var userId = GetCurrentUserId();
            await _authService.Logout(userId);
            ClearAuthCookies();
            return Ok(new { message = "Logged out successfully." });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest? request)
        {
            var refreshToken = request?.RefreshToken;
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                Request.Cookies.TryGetValue("refreshToken", out refreshToken);
            }

            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return Unauthorized(new { message = "Missing refresh token." });
            }

            var response = await _authService.RefreshToken(refreshToken);
            SetAuthCookies(response);
            return Ok(response);
        }

        [HttpGet("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromQuery] string email, [FromQuery] string token)
        {
            await _authService.VerifyEmail(new VerifyEmailRequest
            {
                Email = email,
                Token = token
            });

            return Ok(new { message = "Email verified successfully." });
        }

        [HttpPost("resend-verification")]
        public async Task<IActionResult> ResendVerification([FromBody] ResendVerificationRequest request)
        {
            await _authService.ResendVerification(request);
            return Ok(new { message = "Verification email sent." });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            await _authService.ForgotPassword(request);
            return Ok(new { message = "A temporary password has been sent to your email." });
        }

        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = GetCurrentUserId();
            var user = await _authService.GetUserById(userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            return Ok(user);
        }

        [Authorize]
        [HttpPut("profile")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateProfile([FromForm] UpdateAccountRequest request)
        {
            var userId = GetCurrentUserId();
            var user = await _authService.UpdateAccount(userId, request);
            return Ok(user);
        }

        [Authorize]
        [HttpDelete("deactivate")]
        public async Task<IActionResult> DeactivateAccount()
        {
            var userId = GetCurrentUserId();
            await _authService.DeactivateAccount(userId);
            return Ok(new { message = "Account deactivated successfully." });
        }

        private int GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(claim) || !int.TryParse(claim, out var userId))
            {
                throw new InvalidOperationException("Invalid token payload.");
            }

            return userId;
        }

        private void SetAuthCookies(LoginResponse response)
        {
            var isSecure = Request.IsHttps;

            Response.Cookies.Append("accessToken", response.Token, new CookieOptions
            {
                HttpOnly = true,
                Secure = isSecure,
                SameSite = SameSiteMode.Lax,
                Expires = response.ExpiresAt,
                Path = "/"
            });

            Response.Cookies.Append("refreshToken", response.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = isSecure,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddDays(7),
                Path = "/"
            });
        }

        private void ClearAuthCookies()
        {
            Response.Cookies.Delete("accessToken", new CookieOptions { Path = "/" });
            Response.Cookies.Delete("refreshToken", new CookieOptions { Path = "/" });
        }
    }
}
