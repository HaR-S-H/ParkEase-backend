using System.ComponentModel.DataAnnotations;

namespace NotificationService.Models.Dtos
{
    public class SendSmsRequest
    {
        [Required]
        [MaxLength(30)]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(1000)]
        public string Body { get; set; } = string.Empty;
    }
}
