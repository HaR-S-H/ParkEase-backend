using ParkingLotService.Models;

namespace ParkingLotService.Repositories
{
    public interface IParkingLotRepository
    {
        Task<ParkingLot?> FindByLotId(int lotId);
        Task<List<ParkingLot>> FindByCity(string city);
        Task<List<ParkingLot>> FindByManagerId(int managerId);
        Task<List<ParkingLot>> FindByIsOpen(bool isOpen);
        Task<List<ParkingLot>> FindNearby(double latitude, double longitude, double radiusKm);
        Task<List<ParkingLot>> FindByAvailableSpotsGreaterThan(int availableSpots);
        Task<int> CountByCity(string city);
        Task<List<ParkingLot>> SearchLots(string query);
        Task<List<ParkingLot>> GetAll();
        Task<ParkingLot> Create(ParkingLot parkingLot);
        Task Update(ParkingLot parkingLot);
        Task<bool> UpdateImageUrl(int lotId, string imageUrl);
        Task DeleteByLotId(int lotId);
        Task<int> SaveChanges();
    }
}