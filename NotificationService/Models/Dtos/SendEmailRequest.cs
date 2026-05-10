using System.ComponentModel.DataAnnotations;

namespace NotificationService.Models.Dtos
{
    public class SendEmailRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string Subject { get; set; } = string.Empty;

        [Required]
        [MaxLength(5000)]
        public string Body { get; set; } = string.Empty;
    }
}
