namespace AnalyticsService.Models.Dtos
{
    public class PlatformSummary
    {
        public int TotalActiveLots { get; set; }
        public double AverageOccupancyRate { get; set; }
        public double TotalRevenue { get; set; }
        public Dictionary<string, long> SpotTypeUsage { get; set; } = [];
        public DateTime GeneratedAt { get; set; }
    }
}
