using System.ComponentModel.DataAnnotations;

namespace NotificationService.Models.Dtos
{
    public class SendForgotPasswordEmailRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(120)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string TemporaryPassword { get; set; } = string.Empty;
    }
}