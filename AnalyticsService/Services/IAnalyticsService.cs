using AnalyticsService.Models.Dtos;

namespace AnalyticsService.Services
{
    public interface IAnalyticsService
    {
        Task LogOccupancy(CancellationToken cancellationToken = default);
        Task<double> GetOccupancyRate(int lotId);
        Task<Dictionary<int, double>> GetOccupancyByHour(int lotId, DateOnly? date = null);
        Task<List<int>> GetPeakHours(int lotId);
        Task<double> GetRevenueByLot(int lotId);
        Task<Dictionary<DateOnly, double>> GetRevenueByDay(int lotId, DateOnly? from = null, DateOnly? to = null);
        Task<Dictionary<string, long>> GetMostUsedSpotTypes(int lotId);
        Task<double> GetAvgDuration(int lotId);
        Task<PlatformSummary> GetPlatformSummary();
        Task<LotReport> GenerateDailyReport(int lotId, DateOnly date);
    }
}
