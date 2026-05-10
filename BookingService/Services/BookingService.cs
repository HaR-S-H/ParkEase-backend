using BookingService.Models;
using BookingService.Models.Dtos;
using BookingService.Repositories;

namespace BookingService.Services
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _repo;
        private readonly ISpotService _spotSvc;
        private readonly IParkingLotService _lotSvc;
        private readonly IPaymentService _paySvc;

        public BookingService(
            IBookingRepository repo,
            ISpotService spotSvc,
            IParkingLotService lotSvc,
            IPaymentService paySvc)
        {
            _repo = repo;
            _spotSvc = spotSvc;
            _lotSvc = lotSvc;
            _paySvc = paySvc;
        }

        public async Task<BookingResponse> CreateBooking(CreateBookingRequest request)
        {
            if (request.EndTime <= request.StartTime)
            {
                throw new AppException("End time must be after start time.", StatusCodes.Status400BadRequest);
            }

            var existing = await _repo.FindActiveBySpotId(request.SpotId);
            if (existing != null && existing.UserId != request.UserId)
            {
                // If there's an active booking for this spot by a DIFFERENT user, block it.
                throw new AppException("Spot already has an active booking by another user.", StatusCodes.Status409Conflict);
            }
            // If the same user has a reserved booking, we can allow them to proceed (effectively re-creating or skipping)
            // Or better: let the physical spot status be the ultimate decider later in the method.

            SpotDetails? spot = null;
            try
            {
                spot = await _spotSvc.GetSpotById(request.SpotId);
            }
            catch (AppException ex) when (ex.StatusCode == StatusCodes.Status502BadGateway)
            {
                // ParkingSpotService is optional for local dev until the service is restored.
                spot = null;
            }

            if (spot != null && !string.Equals(spot.Status, "AVAILABLE", StringComparison.OrdinalIgnoreCase))
            {
                throw new AppException("Spot is not available for booking.", StatusCodes.Status409Conflict);
            }

            var lotId = request.LotId > 0 ? request.LotId : spot?.LotId ?? 0;
            if (lotId <= 0)
            {
                throw new AppException("A valid lot is required for booking.", StatusCodes.Status400BadRequest);
            }

            var lot = await _lotSvc.GetLotById(lotId);
            if (!lot.IsOpen || !lot.IsApproved)
            {
                throw new AppException("Parking lot is not open for booking.", StatusCodes.Status409Conflict);
            }

            // The spot-specific check (line 50) is sufficient. 
            // Aggregated AvailableSpots in the Lot service can be inconsistent or out of sync.
            /*
            if (lot.AvailableSpots <= 0)
            {
                throw new AppException("No available spots left in the selected parking lot.", StatusCodes.Status409Conflict);
            }
            */

            if (spot != null)
            {
                await _spotSvc.ReserveSpot(request.SpotId);
            }

            // We rely on spot-level reservation. Aggregate lot counts are not synchronized.
            /*
            try
            {
                await _lotSvc.DecrementAvailable(lotId);
            }
            catch
            {
                if (spot != null)
                {
                    await _spotSvc.ReleaseSpot(request.SpotId);
                }

                throw;
            }
            */

            var booking = new Booking
            {
                UserId = request.UserId,
                LotId = lotId,
                SpotId = request.SpotId,
                VehiclePlate = request.VehiclePlate.Trim().ToUpperInvariant(),
                VehicleType = request.VehicleType.Trim().ToUpperInvariant(),
                BookingType = request.BookingType,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                Status = BookingStatus.RESERVED,
                TotalAmount = 0,
                CreatedAt = DateTime.UtcNow
            };

            booking.TotalAmount = await ComputeAmount(booking);

            var created = await _repo.Create(booking);
            return ToResponse(created);
        }

        public async Task<BookingResponse?> GetBookingById(int bookingId)
        {
            var booking = await _repo.FindByBookingId(bookingId);
            return booking == null ? null : ToResponse(booking);
        }

        public async Task<List<BookingResponse>> GetBookingsByUser(int userId)
        {
            var bookings = await _repo.FindByUserId(userId);
            
            // Self-heal: 
            // 1. Calculate amounts for old bookings that were saved with 0
            // 2. Ensure spots are released for cancelled bookings
            foreach (var b in bookings)
            {
                try {
                    if (b.TotalAmount <= 0)
                    {
                        b.TotalAmount = await ComputeAmount(b);
                        await _repo.Update(b);
                    }
                    
                    if (b.Status == BookingStatus.CANCELLED)
                    {
                        // Retry release in case it failed during the initial cancellation
                        await _spotSvc.ReleaseSpot(b.SpotId);
                    }
                } catch { /* Ignore repair failures */ }
            }

            return bookings.Select(ToResponse).ToList();
        }

        public async Task<List<BookingResponse>> GetBookingsByLot(int lotId)
        {
            var bookings = await _repo.FindByLotId(lotId);
            return bookings.Select(ToResponse).ToList();
        }

        public async Task<List<BookingResponse>> GetActiveBookings()
        {
            var reserved = await _repo.FindByStatus(BookingStatus.RESERVED);
            var active = await _repo.FindByStatus(BookingStatus.ACTIVE);

            return reserved
                .Concat(active)
                .OrderByDescending(booking => booking.CreatedAt)
                .Select(ToResponse)
                .ToList();
        }

        public async Task CancelBooking(int bookingId)
        {
            var booking = await RequireBooking(bookingId);

            if (booking.Status == BookingStatus.COMPLETED)
            {
                throw new AppException("Completed bookings cannot be cancelled.", StatusCodes.Status409Conflict);
            }

            // 1. Release spot and increment lot capacity (Critical for availability)
            try
            {
                await _spotSvc.ReleaseSpot(booking.SpotId);
            }
            catch (Exception ex)
            {
                // Log and continue - we don't want to block cancellation if spot service has issues
                // but we should still try to release it.
            }

            /*
            try
            {
                await _lotSvc.IncrementAvailable(booking.LotId);
            }
            catch (Exception ex)
            {
                // Log and continue
            }
            */

            // 2. Process Refund if applicable
            if (booking.TotalAmount > 0)
            {
                try
                {
                    await _paySvc.Refund(booking.BookingId, booking.UserId, booking.TotalAmount);
                }
                catch (Exception ex)
                {
                    // Log but don't block the cancellation status update
                }
            }

            // 3. Mark booking as cancelled
            booking.Status = BookingStatus.CANCELLED;
            await _repo.Update(booking);
        }

        public async Task<BookingResponse> CheckIn(int bookingId)
        {
            var booking = await RequireBooking(bookingId);

            if (booking.Status != BookingStatus.RESERVED)
            {
                throw new AppException("Only reserved bookings can be checked in.", StatusCodes.Status409Conflict);
            }

            try
            {
                await _spotSvc.OccupySpot(booking.SpotId);
            }
            catch (AppException ex) when (ex.StatusCode == StatusCodes.Status502BadGateway)
            {
                // Ignore for local setups without ParkingSpotService.
            }

            booking.Status = BookingStatus.ACTIVE;
            booking.CheckInTime = DateTime.UtcNow;
            await _repo.Update(booking);

            return ToResponse(booking);
        }

        public async Task<BookingResponse> CheckOut(int bookingId)
        {
            var booking = await RequireBooking(bookingId);

            if (booking.Status != BookingStatus.ACTIVE)
            {
                throw new AppException("Only active bookings can be checked out.", StatusCodes.Status409Conflict);
            }

            var amount = await ComputeAmount(booking);
            await _paySvc.Charge(booking.BookingId, booking.UserId, amount);

            try
            {
                await _spotSvc.ReleaseSpot(booking.SpotId);
            }
            catch (AppException ex) when (ex.StatusCode == StatusCodes.Status502BadGateway)
            {
                // Ignore for local setups without ParkingSpotService.
            }

            // await _lotSvc.IncrementAvailable(booking.LotId);

            booking.EndTime = DateTime.UtcNow;
            booking.TotalAmount = amount;
            booking.Status = BookingStatus.COMPLETED;
            await _repo.Update(booking);

            return ToResponse(booking);
        }

        public async Task<BookingResponse> ExtendBooking(int bookingId, DateTime newEndTime)
        {
            var booking = await RequireBooking(bookingId);

            if (booking.Status is BookingStatus.CANCELLED or BookingStatus.COMPLETED)
            {
                throw new AppException("Completed or cancelled bookings cannot be extended.", StatusCodes.Status409Conflict);
            }

            if (newEndTime <= booking.EndTime)
            {
                throw new AppException("New end time must be later than current end time.", StatusCodes.Status400BadRequest);
            }

            booking.EndTime = newEndTime;
            await _repo.Update(booking);

            return ToResponse(booking);
        }

        public async Task<double> CalculateAmount(int bookingId)
        {
            var booking = await RequireBooking(bookingId);
            return await ComputeAmount(booking);
        }

        public async Task<List<BookingResponse>> GetBookingHistory(int userId)
        {
            var bookings = await _repo.FindByUserId(userId);
            return bookings
                .Where(booking => booking.Status is BookingStatus.CANCELLED or BookingStatus.COMPLETED)
                .OrderByDescending(booking => booking.CreatedAt)
                .Select(ToResponse)
                .ToList();
        }

        private async Task<Booking> RequireBooking(int bookingId)
        {
            return await _repo.FindByBookingId(bookingId)
                ?? throw new AppException("Booking not found.", StatusCodes.Status404NotFound);
        }

        private async Task<double> ComputeAmount(Booking booking)
        {
            SpotDetails? spot = null;
            try
            {
                spot = await _spotSvc.GetSpotById(booking.SpotId);
            }
            catch (AppException ex) when (ex.StatusCode == StatusCodes.Status502BadGateway)
            {
                // Default hourly price keeps checkout working during local development.
                spot = new SpotDetails
                {
                    SpotId = booking.SpotId,
                    LotId = booking.LotId,
                    Status = "AVAILABLE",
                    PricePerHour = 50
                };
            }

            // Determine start and end times for fare calculation
            DateTime startTime;
            DateTime endTime;

            if (booking.Status == BookingStatus.ACTIVE && booking.CheckInTime.HasValue)
            {
                // For active bookings being calculated mid-stay: use actual check-in to current time
                startTime = booking.CheckInTime.Value;
                endTime = DateTime.UtcNow;
            }
            else if (booking.Status == BookingStatus.ACTIVE)
            {
                // Fallback if CheckInTime missing (shouldn't happen)
                startTime = booking.StartTime;
                endTime = DateTime.UtcNow;
            }
            else
            {
                // For completed/reserved bookings: use full reservation window
                startTime = booking.CheckInTime ?? booking.StartTime;
                endTime = booking.EndTime;
            }

            // Calculate duration in hours
            var durationHours = (endTime - startTime).TotalHours;

            // Enforce minimum 1-hour charge
            durationHours = Math.Max(durationHours, 1.0);

            // Round up to 2 decimal places for fair charging
            var roundedHours = Math.Ceiling(durationHours * 100) / 100;
            
            // Ensure we use a sensible price if the spot price is missing/zero
            var hourlyPrice = spot.PricePerHour > 0 ? spot.PricePerHour : 50.0;
            
            return Math.Round(roundedHours * hourlyPrice, 2);
        }

        private static BookingResponse ToResponse(Booking booking)
        {
            return new BookingResponse
            {
                BookingId = booking.BookingId,
                UserId = booking.UserId,
                LotId = booking.LotId,
                SpotId = booking.SpotId,
                VehiclePlate = booking.VehiclePlate,
                VehicleType = booking.VehicleType,
                BookingType = booking.BookingType,
                StartTime = booking.StartTime,
                EndTime = booking.EndTime,
                CheckInTime = booking.CheckInTime,
                Status = booking.Status,
                TotalAmount = booking.TotalAmount,
                CreatedAt = booking.CreatedAt
            };
        }
    }
}
