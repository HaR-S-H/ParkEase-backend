using System.ComponentModel.DataAnnotations;

namespace VehicleService.Models.Dtos
{
    public class UpdateVehicleRequest
    {
        [Required]
        [MaxLength(30)]
        public string LicensePlate { get; set; } = string.Empty;

        [Required]
        [MaxLength(80)]
        public string Make { get; set; } = string.Empty;

        [Required]
        [MaxLength(80)]
        public string Model { get; set; } = string.Empty;

        [Required]
        [MaxLength(40)]
        public string Color { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string VehicleType { get; set; } = string.Empty;

        public bool IsEV { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
