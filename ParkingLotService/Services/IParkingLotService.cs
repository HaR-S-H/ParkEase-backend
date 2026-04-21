using ParkingLotService.Models.Dtos;
using Microsoft.AspNetCore.Http;

namespace ParkingLotService.Services
{
    public interface IParkingLotService
    {
        Task<ParkingLotResponse> CreateLot(CreateParkingLotRequest request);
        Task QueueImageUpload(int lotId, IFormFile imageFile, CancellationToken cancellationToken = default);
        Task<ParkingLotResponse?> GetLotById(int lotId);
        Task<List<ParkingLotResponse>> GetLotsByCity(string city);
        Task<List<ParkingLotResponse>> GetNearbyLots(double latitude, double longitude, double radiusKm);
        Task<List<ParkingLotResponse>> GetLotsByManager(int managerId);
        Task<ParkingLotResponse> UpdateLot(int lotId, UpdateParkingLotRequest request);
        Task<ParkingLotResponse> ToggleOpen(int lotId);
        Task<ParkingLotResponse> ApproveLot(int lotId);
        Task DeleteLot(int lotId);
        Task<ParkingLotResponse> DecrementAvailable(int lotId, int quantity = 1);
        Task<ParkingLotResponse> IncrementAvailable(int lotId, int quantity = 1);
        Task<List<ParkingLotResponse>> SearchLots(string query);
    }
}