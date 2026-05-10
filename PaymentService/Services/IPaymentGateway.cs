namespace PaymentService.Services
{
    public class GatewayResponse
    {
        public string TransactionId { get; set; } = string.Empty;
        public string? CheckoutKey { get; set; }
        public string? CheckoutData { get; set; }
    }

    public interface IPaymentGateway
    {
        Task<GatewayResponse> Charge(Models.Payment payment);
        Task Refund(Models.Payment payment);
    }
}
