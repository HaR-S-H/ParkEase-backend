using VehicleService.Models;

namespace VehicleService.Repositories
{
    public interface IVehicleRepository
    {
        Task<List<Vehicle>> FindByOwnerId(int ownerId);
        Task<Vehicle?> FindByLicensePlate(string licensePlate);
        Task<Vehicle?> FindByVehicleId(int vehicleId);
        Task<List<Vehicle>> FindByVehicleType(string vehicleType);
        Task<List<Vehicle>> FindByIsEV(bool isEV);
        Task<bool> ExistsByLicensePlate(string licensePlate);
        Task<bool> ExistsByLicensePlate(int ownerId, string licensePlate);
        Task<Vehicle> Create(Vehicle vehicle);
        Task Update(Vehicle vehicle);
        Task DeleteByVehicleId(int vehicleId);
        Task<List<Vehicle>> FindAll();
    }
}
