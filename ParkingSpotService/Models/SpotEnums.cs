namespace ParkingSpotService.Models
{
    public enum SpotType
    {
        COMPACT,
        STANDARD,
        LARGE,
        MOTORBIKE,
        EV
    }

    public enum VehicleType
    {
        TWOW,
        FOURW,
        HEAVY
    }

    public enum SpotStatus
    {
        AVAILABLE,
        RESERVED,
        OCCUPIED
    }
}