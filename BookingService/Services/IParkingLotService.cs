namespace BookingService.Services
{
    public interface IParkingLotService
    {
        Task<LotDetails> GetLotById(int lotId);
        Task DecrementAvailable(int lotId, int quantity = 1);
        Task IncrementAvailable(int lotId, int quantity = 1);
    }
}
