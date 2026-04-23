using BookingService.Models;

namespace BookingService.Repositories
{
    public interface IBookingRepository
    {
        Task<List<Booking>> FindByUserId(int userId);
        Task<List<Booking>> FindByLotId(int lotId);
        Task<List<Booking>> FindBySpotId(int spotId);
        Task<List<Booking>> FindByStatus(BookingStatus status);
        Task<Booking?> FindByBookingId(int bookingId);
        Task<Booking?> FindActiveBySpotId(int spotId);
        Task<List<Booking>> FindByVehiclePlate(string vehiclePlate);
        Task<int> CountByLotIdAndStatus(int lotId, BookingStatus status);
        Task<Booking> Create(Booking booking);
        Task Update(Booking booking);
    }
}
