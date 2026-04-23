namespace VehicleService.Models.Dtos
{
    public class VehicleResponse
    {
        public int VehicleId { get; set; }
        public int OwnerId { get; set; }
        public string LicensePlate { get; set; } = string.Empty;
        public string Make { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string VehicleType { get; set; } = string.Empty;
        public bool IsEV { get; set; }
        public DateOnly RegisteredAt { get; set; }
        public bool IsActive { get; set; }
    }
}
