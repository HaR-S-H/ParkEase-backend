using System.ComponentModel.DataAnnotations;
using BookingService.Models;

namespace BookingService.Models.Dtos
{
    public class CreateBookingRequest
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public int LotId { get; set; }

        [Required]
        public int SpotId { get; set; }

        [Required]
        public required string VehiclePlate { get; set; }

        [Required]
        public required string VehicleType { get; set; }

        [Required]
        public BookingType BookingType { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public DateTime EndTime { get; set; }
    }

    public class ExtendBookingRequest
    {
        [Required]
        public DateTime NewEndTime { get; set; }
    }

    public class BookingResponse
    {
        public int BookingId { get; set; }
        public int UserId { get; set; }
        public int LotId { get; set; }
        public int SpotId { get; set; }
        public required string VehiclePlate { get; set; }
        public required string VehicleType { get; set; }
        public BookingType BookingType { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public DateTime? CheckInTime { get; set; }
        public BookingStatus Status { get; set; }
        public double TotalAmount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CalculateAmountResponse
    {
        public int BookingId { get; set; }
        public double Amount { get; set; }
    }
}
