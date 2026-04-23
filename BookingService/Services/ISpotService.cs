namespace BookingService.Services
{
    public interface ISpotService
    {
        Task<SpotDetails> GetSpotById(int spotId);
        Task ReserveSpot(int spotId);
        Task OccupySpot(int spotId);
        Task ReleaseSpot(int spotId);
    }
}
