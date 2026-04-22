using PaymentService.Models;

namespace PaymentService.Services
{
    public interface IPaymentGateway
    {
        Task<string> Charge(Payment payment);
        Task Refund(Payment payment);
    }
}
