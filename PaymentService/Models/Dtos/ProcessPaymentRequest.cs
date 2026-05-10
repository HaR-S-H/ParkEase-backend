using System.ComponentModel.DataAnnotations;

namespace PaymentService.Models.Dtos
{
    public class ProcessPaymentRequest
    {
        [Required]
        public int BookingId { get; set; }

        [Required]
        public int UserId { get; set; }

        public int? LotId { get; set; }

        [Range(0.01, double.MaxValue)]
        public double Amount { get; set; }

        [Required]
        public PaymentMode Mode { get; set; } = PaymentMode.CARD;

        [Required]
        [MaxLength(8)]
        public string Currency { get; set; } = "INR";

        [MaxLength(300)]
        public string Description { get; set; } = string.Empty;
    }
}
