namespace PaymentService.Models.Dtos
{
    public class PaymentResponse
    {
        public int PaymentId { get; set; }
        public int BookingId { get; set; }
        public int UserId { get; set; }
        public int? LotId { get; set; }
        public double Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Mode { get; set; } = string.Empty;
        public string TransactionId { get; set; } = string.Empty;
        public string Currency { get; set; } = string.Empty;
        public string? CheckoutKey { get; set; }
        public string? CheckoutData { get; set; }
        public DateTime PaidAt { get; set; }
        public DateTime? RefundedAt { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}
