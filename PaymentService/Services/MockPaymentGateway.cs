using PaymentService.Models;

namespace PaymentService.Services
{
    public class MockPaymentGateway : IPaymentGateway
    {
        public Task<string> Charge(Payment payment)
        {
            var prefix = payment.Mode switch
            {
                PaymentMode.CARD => "STRIPE",
                PaymentMode.UPI => "RAZORPAY",
                PaymentMode.WALLET => "RAZORPAY",
                PaymentMode.CASH => "CASH",
                _ => "PAY"
            };

            var transactionId = $"{prefix}-{Guid.NewGuid():N}";
            return Task.FromResult(transactionId);
        }

        public Task Refund(Payment payment)
        {
            if (string.IsNullOrWhiteSpace(payment.TransactionId))
            {
                throw new AppException("Missing transaction ID for refund.", StatusCodes.Status400BadRequest);
            }

            return Task.CompletedTask;
        }
    }
}
