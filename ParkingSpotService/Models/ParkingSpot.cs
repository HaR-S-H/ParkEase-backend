namespace ParkingSpotService.Models
{
    public class ParkingSpot
    {
        public int SpotId { get; set; }
        public int LotId { get; set; }
        public required string SpotNumber { get; set; }
        public int Floor { get; set; }
        public SpotType SpotType { get; set; }
        public VehicleType VehicleType { get; set; }
        public SpotStatus Status { get; set; } = SpotStatus.AVAILABLE;
        public bool IsHandicapped { get; set; }
        public bool IsEVCharging { get; set; }
        public double PricePerHour { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}