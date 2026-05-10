namespace BookingService.Services
{
    public class LotDetails
    {
        public int LotId { get; set; }
        public int AvailableSpots { get; set; }
        public bool IsOpen { get; set; }
        public bool IsApproved { get; set; }
    }

    public class SpotDetails
    {
        public int SpotId { get; set; }
        public int LotId { get; set; }
        public required string Status { get; set; }
        public double PricePerHour { get; set; }
    }
}
