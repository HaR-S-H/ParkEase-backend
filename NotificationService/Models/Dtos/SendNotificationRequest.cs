using System.ComponentModel.DataAnnotations;

namespace NotificationService.Models.Dtos
{
    public class SendNotificationRequest
    {
        [Range(1, int.MaxValue)]
        public int RecipientId { get; set; }

        [Required]
        [MaxLength(30)]
        public string Type { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(1000)]
        public string Message { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string Channel { get; set; } = string.Empty;

        public int? RelatedId { get; set; }

        [MaxLength(50)]
        public string? RelatedType { get; set; }
    }
}
