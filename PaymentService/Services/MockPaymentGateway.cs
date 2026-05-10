using PaymentService.Models;

namespace PaymentService.Services
{
    public class MockPaymentGateway : IPaymentGateway
    {
        public Task<GatewayResponse> Charge(Payment payment)
        {
            var prefix = payment.Mode switch
            {
                PaymentMode.STRIPE => "STRIPE",
                PaymentMode.RAZORPAY => "RAZORPAY",
                PaymentMode.UPI => "RAZORPAY",
                PaymentMode.WALLET => "RAZORPAY",
                PaymentMode.CASH => "CASH",
                _ => "PAY"
            };

            var transactionId = $"{prefix}-MOCK-{Guid.NewGuid():N}";
            
            return Task.FromResult(new GatewayResponse
            {
                TransactionId = transactionId,
                CheckoutKey = "mock_key",
                CheckoutData = "{\"mock\": true}"
            });
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
