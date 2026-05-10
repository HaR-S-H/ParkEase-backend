using AnalyticsService.Models;

namespace AnalyticsService.Repositories
{
    public interface IAnalyticsRepository
    {
        Task<List<OccupancyLog>> FindByLotId(int lotId);
        Task<List<OccupancyLog>> FindByLotIdAndTimestampBetween(int lotId, DateTime fromUtc, DateTime toUtc);
        Task<double> AvgOccupancyByLotId(int lotId);
        Task<List<int>> FindPeakHoursByLotId(int lotId);
        Task<List<OccupancyLog>> FindByVehicleType(string vehicleType);
        Task<int> CountByLotIdToday(int lotId);
        Task<OccupancyLog> Create(OccupancyLog log);
    }
}
