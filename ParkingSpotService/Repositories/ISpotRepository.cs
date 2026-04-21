using ParkingSpotService.Models;

namespace ParkingSpotService.Repositories
{
    public interface ISpotRepository
    {
        Task<ParkingSpot> AddSpot(ParkingSpot spot);
        Task AddRange(IEnumerable<ParkingSpot> spots);
        Task<List<ParkingSpot>> FindByLotId(int lotId);
        Task<List<ParkingSpot>> FindByLotIdAndStatus(int lotId, SpotStatus status);
        Task<List<ParkingSpot>> FindByLotIdAndSpotType(int lotId, SpotType spotType);
        Task<List<ParkingSpot>> FindByLotIdAndVehicleType(int lotId, VehicleType vehicleType);
        Task<ParkingSpot?> FindBySpotId(int spotId);
        Task<int> CountByLotIdAndStatus(int lotId, SpotStatus status);
        Task<List<ParkingSpot>> FindByIsEVCharging(bool isEVCharging);
        Task UpdateSpot(ParkingSpot spot);
        Task DeleteBySpotId(int spotId);
    }
}