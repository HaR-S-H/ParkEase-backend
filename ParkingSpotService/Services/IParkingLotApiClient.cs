namespace ParkingSpotService.Services
{
    public interface IParkingLotApiClient
    {
        Task IncrementTotalSpots(int lotId, int quantity = 1);
    }
}