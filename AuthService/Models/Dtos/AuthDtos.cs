using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace AuthService.Models.Dtos
{
    public class RegisterResponse
    {
        public required string Message { get; set; }
        public required UserResponse User { get; set; }
    }

    public class RegisterRequest
    {
        [Required]
        public required string FullName { get; set; }

        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        [MinLength(6)]
        public required string Password { get; set; }

        [Required]
        public required string Phone { get; set; }

        [Required]
        public required string Role { get; set; }

        [Required]
        public required string VehiclePlate { get; set; }

        public IFormFile? ProfilePic { get; set; }
    }

    public class LoginRequest
    {
        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        public required string Password { get; set; }
    }

    public class GoogleLoginRequest
    {
        [Required]
        public required string IdToken { get; set; }

        public string? Role { get; set; }
        public string? Phone { get; set; }
        public string? VehiclePlate { get; set; }
    }

    public class VerifyEmailRequest
    {
        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        public required string Token { get; set; }
    }

    public class ResendVerificationRequest
    {
        [Required]
        [EmailAddress]
        public required string Email { get; set; }
    }

    public class ForgotPasswordRequest
    {
        [Required]
        [EmailAddress]
        public required string Email { get; set; }
    }

    public class RefreshTokenRequest
    {
        public string? RefreshToken { get; set; }
    }

    public class UpdateAccountRequest
    {
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public string? VehiclePlate { get; set; }
        public IFormFile? ProfilePic { get; set; }
        public string? CurrentPassword { get; set; }
        public string? NewPassword { get; set; }
    }

    public class UserResponse
    {
        public int UserId { get; set; }
        public required string FullName { get; set; }
        public required string Email { get; set; }
        public required string Phone { get; set; }
        public required string Role { get; set; }
        public required string VehiclePlate { get; set; }
        public bool EmailVerified { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? ProfilePicUrl { get; set; }
    }

    public class LoginResponse
    {
        public required string Token { get; set; }
        public required string RefreshToken { get; set; }
        public DateTime ExpiresAt { get; set; }
        public required UserResponse User { get; set; }
    }
}
