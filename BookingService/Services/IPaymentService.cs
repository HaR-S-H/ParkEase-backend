namespace BookingService.Services
{
    public interface IPaymentService
    {
        Task Charge(int bookingId, int userId, double amount);
        Task Refund(int bookingId, int userId, double amount);
    }
}
