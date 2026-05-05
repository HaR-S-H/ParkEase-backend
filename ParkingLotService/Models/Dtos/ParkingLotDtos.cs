using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace ParkingLotService.Models.Dtos
{
    public class CreateParkingLotRequest
    {
        [Required]
        public required string Name { get; set; }

        [Required]
        public required string Address { get; set; }

        [Required]
        public required string City { get; set; }

        [Range(-90, 90)]
        public double Latitude { get; set; }

        [Range(-180, 180)]
        public double Longitude { get; set; }

        [Range(0, int.MaxValue)]
        public int TotalSpots { get; set; } = 0;

        [Range(0, int.MaxValue)]
        public int AvailableSpots { get; set; } = 0;

        [Required]
        public int ManagerId { get; set; }

        public bool IsOpen { get; set; } = true;

        public bool IsApproved { get; set; } = false;

        public TimeOnly OpenTime { get; set; }

        public TimeOnly CloseTime { get; set; }

        public IFormFile? ImageFile { get; set; }
    }

    public class UpdateParkingLotRequest
    {
        public string? Name { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public int? TotalSpots { get; set; }
        public int? AvailableSpots { get; set; }
        public int? ManagerId { get; set; }
        public bool? IsOpen { get; set; }
        public bool? IsApproved { get; set; }
        public TimeOnly? OpenTime { get; set; }
        public TimeOnly? CloseTime { get; set; }
        public IFormFile? ImageFile { get; set; }
    }

    public class ParkingLotResponse
    {
        public int LotId { get; set; }
        public required string Name { get; set; }
        public required string Address { get; set; }
        public required string City { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int TotalSpots { get; set; }
        public int AvailableSpots { get; set; }
        public int ManagerId { get; set; }
        public bool IsOpen { get; set; }
        public bool IsApproved { get; set; }
        public TimeOnly OpenTime { get; set; }
        public TimeOnly CloseTime { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}