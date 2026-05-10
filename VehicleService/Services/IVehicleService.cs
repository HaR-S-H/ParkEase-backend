using VehicleService.Models;

namespace VehicleService.Services
{
    public interface IVehicleService
    {
        Task<Vehicle> RegisterVehicle(Vehicle vehicle);
        Task<Vehicle?> GetVehicleById(int vehicleId);
        Task<List<Vehicle>> GetVehiclesByOwner(int ownerId);
        Task<Vehicle?> GetByLicensePlate(string licensePlate);
        Task<Vehicle> UpdateVehicle(int vehicleId, Vehicle updatedVehicle);
        Task DeleteVehicle(int vehicleId);
        Task<string> GetVehicleType(int vehicleId);
        Task<bool> IsEVVehicle(int vehicleId);
        Task<List<Vehicle>> GetAllVehicles();
    }
}
