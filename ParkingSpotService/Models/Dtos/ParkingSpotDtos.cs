using System.ComponentModel.DataAnnotations;
using ParkingSpotService.Models;

namespace ParkingSpotService.Models.Dtos
{
    public class AddSpotRequest
    {
        [Required]
        public int LotId { get; set; }

        [Required]
        public required string SpotNumber { get; set; }

        public int Floor { get; set; }

        public SpotType SpotType { get; set; }

        public VehicleType VehicleType { get; set; }

        public SpotStatus Status { get; set; } = SpotStatus.AVAILABLE;

        public bool IsHandicapped { get; set; }

        public bool IsEVCharging { get; set; }

        public double PricePerHour { get; set; }
    }

    public class AddBulkSpotsRequest
    {
        [Required]
        public int LotId { get; set; }

        [Required]
        public string? SpotNumberPrefix { get; set; } = "S";

        [Range(1, int.MaxValue)]
        public int StartNumber { get; set; } = 1;

        [Range(1, int.MaxValue)]
        public int Count { get; set; }

        public int Floor { get; set; }

        public SpotType SpotType { get; set; }

        public VehicleType VehicleType { get; set; }

        public SpotStatus Status { get; set; } = SpotStatus.AVAILABLE;

        public bool IsHandicapped { get; set; }

        public bool IsEVCharging { get; set; }

        public double PricePerHour { get; set; }
    }

    public class UpdateSpotRequest
    {
        public string? SpotNumber { get; set; }
        public int? Floor { get; set; }
        public SpotType? SpotType { get; set; }
        public VehicleType? VehicleType { get; set; }
        public SpotStatus? Status { get; set; }
        public bool? IsHandicapped { get; set; }
        public bool? IsEVCharging { get; set; }
        public double? PricePerHour { get; set; }
    }

    public class ParkingSpotResponse
    {
        public int SpotId { get; set; }
        public int LotId { get; set; }
        public required string SpotNumber { get; set; }
        public int Floor { get; set; }
        public SpotType SpotType { get; set; }
        public VehicleType VehicleType { get; set; }
        public SpotStatus Status { get; set; }
        public bool IsHandicapped { get; set; }
        public bool IsEVCharging { get; set; }
        public double PricePerHour { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CountAvailableResponse
    {
        public int LotId { get; set; }
        public int AvailableCount { get; set; }
    }
}