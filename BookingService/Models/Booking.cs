using System.ComponentModel.DataAnnotations;

namespace BookingService.Models
{
    public class Booking
    {
        [Key]
        public int BookingId { get; set; }

        public int UserId { get; set; }
        public int LotId { get; set; }
        public int SpotId { get; set; }

        [MaxLength(30)]
        public required string VehiclePlate { get; set; }

        [MaxLength(20)]
        public required string VehicleType { get; set; }

        public BookingType BookingType { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public BookingStatus Status { get; set; }

        public double TotalAmount { get; set; }

        public DateTime? CheckInTime { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
