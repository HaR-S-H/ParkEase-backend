using System.ComponentModel.DataAnnotations;

namespace NotificationService.Models.Dtos
{
    public class SendBulkRequest
    {
        [Required]
        public List<int> RecipientIds { get; set; } = [];

        [Required]
        [MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(1000)]
        public string Message { get; set; } = string.Empty;
    }
}
