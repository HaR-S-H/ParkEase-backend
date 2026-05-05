using ParkingSpotService.Models.Dtos;
using ParkingSpotService.Models;

namespace ParkingSpotService.Services
{
    public interface ISpotService
    {
        Task<ParkingSpotResponse> AddSpot(AddSpotRequest request);
        Task<List<ParkingSpotResponse>> AddBulkSpots(AddBulkSpotsRequest request);
        Task<ParkingSpotResponse?> GetSpotById(int spotId);
        Task<List<ParkingSpotResponse>> GetSpotsByLot(int lotId);
        Task<List<ParkingSpotResponse>> GetAvailableSpots(int lotId);
        Task<List<ParkingSpotResponse>> GetByTypeAndLot(int lotId, SpotType spotType);
        Task<ParkingSpotResponse> OccupySpot(int spotId);
        Task<ParkingSpotResponse> ReleaseSpot(int spotId);
        Task<ParkingSpotResponse> UpdateSpot(int spotId, UpdateSpotRequest request);
        Task DeleteSpot(int spotId);
        Task<int> CountAvailable(int lotId);
    }
}