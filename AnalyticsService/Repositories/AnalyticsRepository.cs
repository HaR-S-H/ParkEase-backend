using AnalyticsService.Data;
using AnalyticsService.Models;
using Microsoft.EntityFrameworkCore;

namespace AnalyticsService.Repositories
{
    public class AnalyticsRepository : IAnalyticsRepository
    {
        private readonly AnalyticsDbContext _context;

        public AnalyticsRepository(AnalyticsDbContext context)
        {
            _context = context;
        }

        public Task<List<OccupancyLog>> FindByLotId(int lotId)
        {
            return _context.OccupancyLogs
                .AsNoTracking()
                .Where(log => log.LotId == lotId)
                .OrderByDescending(log => log.Timestamp)
                .ToListAsync();
        }

        public Task<List<OccupancyLog>> FindByLotIdAndTimestampBetween(int lotId, DateTime fromUtc, DateTime toUtc)
        {
            return _context.OccupancyLogs
                .AsNoTracking()
                .Where(log => log.LotId == lotId && log.Timestamp >= fromUtc && log.Timestamp <= toUtc)
                .OrderBy(log => log.Timestamp)
                .ToListAsync();
        }

        public async Task<double> AvgOccupancyByLotId(int lotId)
        {
            var avg = await _context.OccupancyLogs
                .AsNoTracking()
                .Where(log => log.LotId == lotId)
                .Select(log => (double?)log.OccupancyRate)
                .AverageAsync();

            return avg ?? 0;
        }

        public async Task<List<int>> FindPeakHoursByLotId(int lotId)
        {
            return await _context.OccupancyLogs
                .AsNoTracking()
                .Where(log => log.LotId == lotId)
                .GroupBy(log => log.Timestamp.Hour)
                .Select(group => new
                {
                    Hour = group.Key,
                    AvgOcc = group.Average(log => log.OccupancyRate)
                })
                .OrderByDescending(item => item.AvgOcc)
                .Take(3)
                .Select(item => item.Hour)
                .ToListAsync();
        }

        public Task<List<OccupancyLog>> FindByVehicleType(string vehicleType)
        {
            return _context.OccupancyLogs
                .AsNoTracking()
                .Where(log => log.VehicleType == vehicleType)
                .OrderByDescending(log => log.Timestamp)
                .ToListAsync();
        }

        public Task<int> CountByLotIdToday(int lotId)
        {
            var from = DateTime.UtcNow.Date;
            var to = from.AddDays(1);

            return _context.OccupancyLogs.CountAsync(log => log.LotId == lotId && log.Timestamp >= from && log.Timestamp < to);
        }

        public async Task<OccupancyLog> Create(OccupancyLog log)
        {
            _context.OccupancyLogs.Add(log);
            await _context.SaveChangesAsync();
            return log;
        }
    }
}
