using System.Net.Http.Json;
using AnalyticsService.Models;
using AnalyticsService.Models.Dtos;
using AnalyticsService.Repositories;

namespace AnalyticsService.Services
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly IAnalyticsRepository _repo;
        private readonly HttpClient _bookingClient;
        private readonly HttpClient _paymentClient;
        private readonly HttpClient _lotClient;

        public AnalyticsService(IAnalyticsRepository repo, IHttpClientFactory httpClientFactory)
        {
            _repo = repo;
            _bookingClient = httpClientFactory.CreateClient("BookingServiceClient");
            _paymentClient = httpClientFactory.CreateClient("PaymentServiceClient");
            _lotClient = httpClientFactory.CreateClient("ParkingLotServiceClient");
        }

        public async Task LogOccupancy(CancellationToken cancellationToken = default)
        {
            var activeBookings = await GetActiveBookings(cancellationToken);
            if (activeBookings.Count == 0)
            {
                return;
            }

            foreach (var lotGroup in activeBookings.GroupBy(booking => booking.LotId))
            {
                var lot = await GetLotById(lotGroup.Key, cancellationToken);
                if (lot == null || lot.TotalSpots <= 0)
                {
                    continue;
                }

                var occupied = Math.Max(0, lot.TotalSpots - lot.AvailableSpots);
                var occupancyRate = lot.TotalSpots == 0 ? 0 : (double)occupied / lot.TotalSpots * 100;

                var mostCommonVehicleType = lotGroup
                    .Select(booking => booking.VehicleType?.Trim().ToUpperInvariant())
                    .Where(vehicleType => !string.IsNullOrWhiteSpace(vehicleType))
                    .GroupBy(vehicleType => vehicleType!)
                    .OrderByDescending(group => group.Count())
                    .Select(group => group.Key)
                    .FirstOrDefault() ?? "UNKNOWN";

                var spotId = lotGroup.Select(booking => booking.SpotId).DefaultIfEmpty(0).First();

                await _repo.Create(new OccupancyLog
                {
                    LotId = lot.LotId,
                    SpotId = spotId,
                    Timestamp = DateTime.UtcNow,
                    OccupancyRate = occupancyRate,
                    AvailableSpots = lot.AvailableSpots,
                    TotalSpots = lot.TotalSpots,
                    VehicleType = mostCommonVehicleType
                });
            }
        }

        public Task<double> GetOccupancyRate(int lotId)
        {
            return _repo.AvgOccupancyByLotId(lotId);
        }

        public async Task<Dictionary<int, double>> GetOccupancyByHour(int lotId, DateOnly? date = null)
        {
            var target = date ?? DateOnly.FromDateTime(DateTime.UtcNow);
            var from = target.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            var to = from.AddDays(1);

            var logs = await _repo.FindByLotIdAndTimestampBetween(lotId, from, to);
            return logs
                .GroupBy(log => log.Timestamp.Hour)
                .OrderBy(group => group.Key)
                .ToDictionary(
                    group => group.Key,
                    group => Math.Round(group.Average(log => log.OccupancyRate), 2));
        }

        public Task<List<int>> GetPeakHours(int lotId)
        {
            return _repo.FindPeakHoursByLotId(lotId);
        }

        public async Task<double> GetRevenueByLot(int lotId)
        {
            var response = await _paymentClient.GetAsync($"/api/v1/payments/revenue/{lotId}");
            if (!response.IsSuccessStatusCode)
            {
                throw new AppException("Failed to fetch lot revenue from PaymentService.", StatusCodes.Status502BadGateway);
            }

            var payload = await response.Content.ReadFromJsonAsync<RevenueByLotResponse>();
            return payload?.TotalRevenue ?? 0;
        }

        public async Task<Dictionary<DateOnly, double>> GetRevenueByDay(int lotId, DateOnly? from = null, DateOnly? to = null)
        {
            var paid = await GetPaidPayments();

            var fromDate = from ?? DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-7));
            var toDate = to ?? DateOnly.FromDateTime(DateTime.UtcNow);

            return paid
                .Where(payment => payment.LotId == lotId)
                .Where(payment => DateOnly.FromDateTime(payment.PaidAt) >= fromDate && DateOnly.FromDateTime(payment.PaidAt) <= toDate)
                .GroupBy(payment => DateOnly.FromDateTime(payment.PaidAt))
                .OrderBy(group => group.Key)
                .ToDictionary(
                    group => group.Key,
                    group => Math.Round(group.Sum(payment => payment.Amount), 2));
        }

        public async Task<Dictionary<string, long>> GetMostUsedSpotTypes(int lotId)
        {
            var bookings = await GetBookingsByLot(lotId);

            return bookings
                .Where(booking => !string.IsNullOrWhiteSpace(booking.VehicleType))
                .GroupBy(booking => booking.VehicleType.Trim().ToUpperInvariant())
                .OrderByDescending(group => group.LongCount())
                .ToDictionary(group => group.Key, group => group.LongCount());
        }

        public async Task<double> GetAvgDuration(int lotId)
        {
            var bookings = await GetBookingsByLot(lotId);

            var durations = bookings
                .Where(booking => booking.EndTime > booking.StartTime)
                .Select(booking => (booking.EndTime - booking.StartTime).TotalHours)
                .Where(hours => hours > 0)
                .ToList();

            if (durations.Count == 0)
            {
                return 0;
            }

            return Math.Round(durations.Average(), 2);
        }

        public async Task<PlatformSummary> GetPlatformSummary()
        {
            var activeBookings = await GetActiveBookings(CancellationToken.None);
            var lotIds = activeBookings.Select(booking => booking.LotId).Distinct().ToList();

            var occupancyRates = new List<double>();
            var totalRevenue = 0d;
            var allVehicleTypes = new List<string>();

            foreach (var lotId in lotIds)
            {
                occupancyRates.Add(await GetOccupancyRate(lotId));
                totalRevenue += await GetRevenueByLot(lotId);

                var spotTypes = await GetMostUsedSpotTypes(lotId);
                foreach (var pair in spotTypes)
                {
                    for (var i = 0; i < pair.Value; i++)
                    {
                        allVehicleTypes.Add(pair.Key);
                    }
                }
            }

            var usage = allVehicleTypes
                .GroupBy(vehicleType => vehicleType)
                .ToDictionary(group => group.Key, group => group.LongCount());

            return new PlatformSummary
            {
                TotalActiveLots = lotIds.Count,
                AverageOccupancyRate = occupancyRates.Count == 0 ? 0 : Math.Round(occupancyRates.Average(), 2),
                TotalRevenue = Math.Round(totalRevenue, 2),
                SpotTypeUsage = usage,
                GeneratedAt = DateTime.UtcNow
            };
        }

        public async Task<LotReport> GenerateDailyReport(int lotId, DateOnly date)
        {
            var from = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            var to = from.AddDays(1).AddTicks(-1);

            var logs = await _repo.FindByLotIdAndTimestampBetween(lotId, from, to);
            var byHour = await GetOccupancyByHour(lotId, date);
            var revenueByDay = await GetRevenueByDay(lotId, date, date);
            var spotTypes = await GetMostUsedSpotTypes(lotId);

            return new LotReport
            {
                LotId = lotId,
                Date = date,
                OccupancyRate = logs.Count == 0 ? 0 : Math.Round(logs.Average(log => log.OccupancyRate), 2),
                OccupancyByHour = byHour,
                PeakHours = byHour.OrderByDescending(pair => pair.Value).Take(3).Select(pair => pair.Key).ToList(),
                Revenue = revenueByDay.TryGetValue(date, out var revenue) ? revenue : 0,
                MostUsedSpotTypes = spotTypes,
                AvgDurationHours = await GetAvgDuration(lotId),
                LogsCaptured = logs.Count
            };
        }

        private async Task<List<BookingLite>> GetActiveBookings(CancellationToken cancellationToken)
        {
            var response = await _bookingClient.GetAsync("/api/v1/bookings/active", cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                throw new AppException("Failed to fetch active bookings from BookingService.", StatusCodes.Status502BadGateway);
            }

            return await response.Content.ReadFromJsonAsync<List<BookingLite>>(cancellationToken: cancellationToken) ?? [];
        }

        private async Task<List<BookingLite>> GetBookingsByLot(int lotId)
        {
            var response = await _bookingClient.GetAsync($"/api/v1/bookings/lot/{lotId}");
            if (!response.IsSuccessStatusCode)
            {
                throw new AppException("Failed to fetch lot bookings from BookingService.", StatusCodes.Status502BadGateway);
            }

            return await response.Content.ReadFromJsonAsync<List<BookingLite>>() ?? [];
        }

        private async Task<LotLite?> GetLotById(int lotId, CancellationToken cancellationToken)
        {
            var response = await _lotClient.GetAsync($"/api/v1/lots/{lotId}", cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<LotLite>(cancellationToken: cancellationToken);
        }

        private async Task<List<PaymentLite>> GetPaidPayments()
        {
            var response = await _paymentClient.GetAsync("/api/v1/payments/by-status/PAID");
            if (!response.IsSuccessStatusCode)
            {
                throw new AppException("Failed to fetch paid transactions from PaymentService.", StatusCodes.Status502BadGateway);
            }

            return await response.Content.ReadFromJsonAsync<List<PaymentLite>>() ?? [];
        }

        private sealed class BookingLite
        {
            public int LotId { get; set; }
            public int SpotId { get; set; }
            public string VehicleType { get; set; } = string.Empty;
            public DateTime StartTime { get; set; }
            public DateTime EndTime { get; set; }
        }

        private sealed class LotLite
        {
            public int LotId { get; set; }
            public int TotalSpots { get; set; }
            public int AvailableSpots { get; set; }
        }

        private sealed class PaymentLite
        {
            public int? LotId { get; set; }
            public double Amount { get; set; }
            public DateTime PaidAt { get; set; }
        }

        private sealed class RevenueByLotResponse
        {
            public int LotId { get; set; }
            public double TotalRevenue { get; set; }
        }
    }
}
