using BookingService.Data;
using BookingService.Models;
using Microsoft.EntityFrameworkCore;

namespace BookingService.Repositories
{
    public class BookingRepository : IBookingRepository
    {
        private readonly BookingDbContext _context;

        public BookingRepository(BookingDbContext context)
        {
            _context = context;
        }

        public Task<List<Booking>> FindByUserId(int userId)
        {
            return _context.Bookings
                .AsNoTracking()
                .Where(booking => booking.UserId == userId)
                .OrderByDescending(booking => booking.CreatedAt)
                .ToListAsync();
        }

        public Task<List<Booking>> FindByLotId(int lotId)
        {
            return _context.Bookings
                .AsNoTracking()
                .Where(booking => booking.LotId == lotId)
                .OrderByDescending(booking => booking.CreatedAt)
                .ToListAsync();
        }

        public Task<List<Booking>> FindBySpotId(int spotId)
        {
            return _context.Bookings
                .AsNoTracking()
                .Where(booking => booking.SpotId == spotId)
                .OrderByDescending(booking => booking.CreatedAt)
                .ToListAsync();
        }

        public Task<List<Booking>> FindByStatus(BookingStatus status)
        {
            return _context.Bookings
                .AsNoTracking()
                .Where(booking => booking.Status == status)
                .OrderByDescending(booking => booking.CreatedAt)
                .ToListAsync();
        }

        public Task<Booking?> FindByBookingId(int bookingId)
        {
            return _context.Bookings.FirstOrDefaultAsync(booking => booking.BookingId == bookingId);
        }

        public Task<Booking?> FindActiveBySpotId(int spotId)
        {
            return _context.Bookings.FirstOrDefaultAsync(booking =>
                booking.SpotId == spotId
                && (booking.Status == BookingStatus.RESERVED || booking.Status == BookingStatus.ACTIVE));
        }

        public Task<List<Booking>> FindByVehiclePlate(string vehiclePlate)
        {
            return _context.Bookings
                .AsNoTracking()
                .Where(booking => booking.VehiclePlate == vehiclePlate)
                .OrderByDescending(booking => booking.CreatedAt)
                .ToListAsync();
        }

        public Task<int> CountByLotIdAndStatus(int lotId, BookingStatus status)
        {
            return _context.Bookings.CountAsync(booking => booking.LotId == lotId && booking.Status == status);
        }

        public async Task<Booking> Create(Booking booking)
        {
            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();
            return booking;
        }

        public async Task Update(Booking booking)
        {
            _context.Bookings.Update(booking);
            await _context.SaveChangesAsync();
        }
    }
}
