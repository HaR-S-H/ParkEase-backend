using BookingService.Models;
using BookingService.Repositories;
using Microsoft.Extensions.Options;

namespace BookingService.Services
{
    /// <summary>
    /// Background service that periodically checks for expired pre-bookings 
    /// (no check-in within grace period) and auto-cancels them.
    /// </summary>
    public class ExpiredBookingCancellationService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ExpiredBookingCancellationService> _logger;
        private readonly int _gracePeriodMinutes;
        private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(5);

        public ExpiredBookingCancellationService(
            IServiceScopeFactory scopeFactory,
            ILogger<ExpiredBookingCancellationService> logger,
            IConfiguration config)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _gracePeriodMinutes = config.GetValue<int>("PreBookingGracePeriodMinutes", 30);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Expired Booking Cancellation Service started with {GracePeriod} minute grace period.", _gracePeriodMinutes);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CancelExpiredPreBookings(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (ObjectDisposedException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in expired booking cancellation service.");
                }

                try
                {
                    await Task.Delay(_checkInterval, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
            }

            _logger.LogInformation("Expired Booking Cancellation Service stopped.");
        }

        private async Task CancelExpiredPreBookings(CancellationToken stoppingToken)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var repo = scope.ServiceProvider.GetRequiredService<IBookingRepository>();
                var spotSvc = scope.ServiceProvider.GetRequiredService<ISpotService>();
                var lotSvc = scope.ServiceProvider.GetRequiredService<IParkingLotService>();

                // Find all RESERVED pre-bookings that haven't been checked in
                var allReserved = await repo.FindByStatus(BookingStatus.RESERVED);

                var now = DateTime.UtcNow;
                var gracePeriod = TimeSpan.FromMinutes(_gracePeriodMinutes);

                foreach (var booking in allReserved)
                {
                    if (stoppingToken.IsCancellationRequested)
                    {
                        break;
                    }

                    // Only auto-cancel PRE booking type that have exceeded grace period
                    if (booking.BookingType == BookingType.PRE &&
                        (now - booking.CreatedAt) > gracePeriod)
                    {
                        try
                        {
                            await CancelExpiredBooking(booking, repo, spotSvc, lotSvc);
                            _logger.LogInformation(
                                "Auto-cancelled expired pre-booking {BookingId} (User: {UserId}, Spot: {SpotId})",
                                booking.BookingId, booking.UserId, booking.SpotId);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex,
                                "Failed to auto-cancel expired pre-booking {BookingId}",
                                booking.BookingId);
                        }
                    }
                }
            }
        }

        private async Task CancelExpiredBooking(
            Booking booking,
            IBookingRepository repo,
            ISpotService spotSvc,
            IParkingLotService lotSvc)
        {
            // Release spot back to available
            await spotSvc.ReleaseSpot(booking.SpotId);

            // Increment lot availability
            await lotSvc.IncrementAvailable(booking.LotId);

            // Mark booking as cancelled
            booking.Status = BookingStatus.CANCELLED;
            await repo.Update(booking);
        }
    }
}
