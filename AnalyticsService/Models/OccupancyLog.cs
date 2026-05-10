namespace AnalyticsService.Models
{
    public class OccupancyLog
    {
        public int LogId { get; set; }
        public int LotId { get; set; }
        public int SpotId { get; set; }
        public DateTime Timestamp { get; set; }
        public double OccupancyRate { get; set; }
        public int AvailableSpots { get; set; }
        public int TotalSpots { get; set; }
        public string VehicleType { get; set; } = string.Empty;

        public int GetLogId() => LogId;
        public DateTime GetTimestamp() => Timestamp;
        public double GetOccupancyRate() => OccupancyRate;
    }
}
