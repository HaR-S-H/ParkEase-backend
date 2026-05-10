using System.ComponentModel.DataAnnotations;

namespace PaymentService.Models.Dtos
{
    public class RefundPaymentRequest
    {
        [Required]
        public int BookingId { get; set; }

        [MaxLength(300)]
        public string Reason { get; set; } = string.Empty;
    }
}
