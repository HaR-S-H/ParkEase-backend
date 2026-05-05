using PaymentService.Models.Dtos;

namespace PaymentService.Services
{
    public interface IPaymentService
    {
        Task<PaymentResponse> ProcessPayment(ProcessPaymentRequest request);
        Task<PaymentResponse?> GetByBooking(int bookingId);
        Task<List<PaymentResponse>> GetByUser(int userId);
        Task<PaymentResponse> RefundPayment(int bookingId, string? reason = null);
        Task<string> GetPaymentStatus(int bookingId);
        Task UpdateStatus(int bookingId, string status, string? transactionId = null);
        Task<PaymentReceipt> GenerateReceipt(int bookingId);
        Task<double> GetTotalRevenue(int lotId);
        Task<List<PaymentResponse>> GetTransactionHistory(int userId);
        Task<List<PaymentResponse>> GetByStatus(string status);
    }
}
