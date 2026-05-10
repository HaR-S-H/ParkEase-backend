using PaymentService.Models;

namespace PaymentService.Repositories
{
    public interface IPaymentRepository
    {
        Task<Payment?> FindByBookingId(int bookingId);
        Task<List<Payment>> FindByUserId(int userId);
        Task<List<Payment>> FindByStatus(PaymentStatus status);
        Task<Payment?> FindByTransactionId(string transactionId);
        Task<List<Payment>> FindByPaidAtBetween(DateTime fromUtc, DateTime toUtc);
        Task<double> SumAmountByLotId(int lotId);
        Task<int> CountByUserId(int userId);
        Task<Payment> Create(Payment payment);
        Task Update(Payment payment);
    }
}
