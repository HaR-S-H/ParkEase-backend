namespace AnalyticsService.Models.Dtos
{
    public class LotReport
    {
        public int LotId { get; set; }
        public DateOnly Date { get; set; }
        public double OccupancyRate { get; set; }
        public Dictionary<int, double> OccupancyByHour { get; set; } = [];
        public List<int> PeakHours { get; set; } = [];
        public double Revenue { get; set; }
        public Dictionary<string, long> MostUsedSpotTypes { get; set; } = [];
        public double AvgDurationHours { get; set; }
        public int LogsCaptured { get; set; }
    }
}
