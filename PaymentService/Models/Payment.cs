namespace PaymentService.Models
{
    public class Payment
    {
        public int PaymentId { get; set; }
        public int BookingId { get; set; }
        public int UserId { get; set; }
        public int? LotId { get; set; }
        public double Amount { get; set; }
        public PaymentStatus Status { get; set; } = PaymentStatus.PENDING;
        public PaymentMode Mode { get; set; } = PaymentMode.CARD;
        public string TransactionId { get; set; } = string.Empty;
        public string Currency { get; set; } = "INR";
        public DateTime PaidAt { get; set; } = DateTime.UtcNow;
        public DateTime? RefundedAt { get; set; }
        public string Description { get; set; } = string.Empty;

        public int GetPaymentId() => PaymentId;
        public string GetStatus() => Status.ToString();
        public void SetStatus(string status)
        {
            if (!Enum.TryParse<PaymentStatus>(status, true, out var parsed))
            {
                throw new ArgumentException("Invalid payment status.", nameof(status));
            }

            Status = parsed;
        }

        public double GetAmount() => Amount;
    }
}
