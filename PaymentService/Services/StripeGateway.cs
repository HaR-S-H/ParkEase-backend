using Stripe;
using PaymentService.Models;

namespace PaymentService.Services
{
    public class StripeGateway : IPaymentGateway
    {
        public StripeGateway(IConfiguration config)
        {
            StripeConfiguration.ApiKey = config["Gateway:StripeApiKey"] ?? "";
        }

        public async Task<GatewayResponse> Charge(Models.Payment payment)
        {
            if (string.IsNullOrEmpty(StripeConfiguration.ApiKey))
                throw new AppException("Stripe configuration missing.", StatusCodes.Status500InternalServerError);

            var options = new Stripe.Checkout.SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card", "upi" },
                LineItems = new List<Stripe.Checkout.SessionLineItemOptions>
                {
                    new Stripe.Checkout.SessionLineItemOptions
                    {
                        PriceData = new Stripe.Checkout.SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(payment.Amount * 100),
                            Currency = payment.Currency.ToLower(),
                            ProductData = new Stripe.Checkout.SessionLineItemPriceDataProductDataOptions
                            {
                                Name = payment.Description,
                            },
                        },
                        Quantity = 1,
                    },
                },
                Mode = "payment",
                SuccessUrl = "http://localhost:4200/driver/dashboard?payment_success=true&booking_id=" + payment.BookingId,
                CancelUrl = "http://localhost:4200/driver/dashboard?payment_cancelled=true",
                ClientReferenceId = payment.BookingId.ToString()
            };

            var service = new Stripe.Checkout.SessionService();
            var session = await service.CreateAsync(options);

            return new GatewayResponse
            {
                TransactionId = session.Id,
                CheckoutKey = session.Id,
                CheckoutData = System.Text.Json.JsonSerializer.Serialize(new
                {
                    session_id = session.Id,
                    checkout_url = session.Url
                })
            };
        }

        public Task Refund(Models.Payment payment)
        {
            return Task.CompletedTask;
        }
    }
}
