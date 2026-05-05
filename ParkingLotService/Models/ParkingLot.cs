using System.ComponentModel.DataAnnotations;

namespace ParkingLotService.Models
{
    public class ParkingLot
    {
        [Key]
        public int LotId { get; set; }

        [Required]
        public required string Name { get; set; }

        [Required]
        public required string Address { get; set; }

        [Required]
        public required string City { get; set; }

        [Required]
        public double Latitude { get; set; }

        [Required]
        public double Longitude { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int TotalSpots { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        [ConcurrencyCheck]
        public int AvailableSpots { get; set; }

        [Required]
        public int ManagerId { get; set; }

        public bool IsOpen { get; set; } = true;

        public bool IsApproved { get; set; } = false;

        public TimeOnly OpenTime { get; set; }

        public TimeOnly CloseTime { get; set; }

        public string? ImageUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}