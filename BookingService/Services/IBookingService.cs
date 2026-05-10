using BookingService.Models.Dtos;

namespace BookingService.Services
{
    public interface IBookingService
    {
        Task<BookingResponse> CreateBooking(CreateBookingRequest request);
        Task<BookingResponse?> GetBookingById(int bookingId);
        Task<List<BookingResponse>> GetBookingsByUser(int userId);
        Task<List<BookingResponse>> GetBookingsByLot(int lotId);
        Task<List<BookingResponse>> GetActiveBookings();
        Task CancelBooking(int bookingId);
        Task<BookingResponse> CheckIn(int bookingId);
        Task<BookingResponse> CheckOut(int bookingId);
        Task<BookingResponse> ExtendBooking(int bookingId, DateTime newEndTime);
        Task<double> CalculateAmount(int bookingId);
        Task<List<BookingResponse>> GetBookingHistory(int userId);
    }
}
