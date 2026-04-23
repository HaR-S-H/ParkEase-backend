using VehicleService.Models;
using VehicleService.Repositories;

namespace VehicleService.Services
{
    public class VehicleService : IVehicleService
    {
        private static readonly HashSet<string> AllowedVehicleTypes =
        ["2-WHEELER", "4-WHEELER", "HEAVY"];

        private readonly IVehicleRepository _repo;

        public VehicleService(IVehicleRepository repo)
        {
            _repo = repo;
        }

        public async Task<Vehicle> RegisterVehicle(Vehicle vehicle)
        {
            NormalizeVehicle(vehicle);

            if (vehicle.OwnerId <= 0)
            {
                throw new AppException("OwnerId must be greater than zero.", StatusCodes.Status400BadRequest);
            }

            var exists = await _repo.ExistsByLicensePlate(vehicle.OwnerId, vehicle.LicensePlate);
            if (exists)
            {
                throw new AppException("Vehicle with this license plate already exists for the owner.", StatusCodes.Status409Conflict);
            }

            vehicle.RegisteredAt = DateOnly.FromDateTime(DateTime.UtcNow);
            vehicle.IsActive = true;

            return await _repo.Create(vehicle);
        }

        public Task<Vehicle?> GetVehicleById(int vehicleId)
        {
            return _repo.FindByVehicleId(vehicleId);
        }

        public Task<List<Vehicle>> GetVehiclesByOwner(int ownerId)
        {
            return _repo.FindByOwnerId(ownerId);
        }

        public Task<Vehicle?> GetByLicensePlate(string licensePlate)
        {
            var normalizedPlate = NormalizePlate(licensePlate);
            return _repo.FindByLicensePlate(normalizedPlate);
        }

        public async Task<Vehicle> UpdateVehicle(int vehicleId, Vehicle updatedVehicle)
        {
            var existing = await _repo.FindByVehicleId(vehicleId)
                ?? throw new AppException("Vehicle not found.", StatusCodes.Status404NotFound);

            NormalizeVehicle(updatedVehicle);

            if (!string.Equals(existing.LicensePlate, updatedVehicle.LicensePlate, StringComparison.Ordinal)
                && await _repo.ExistsByLicensePlate(existing.OwnerId, updatedVehicle.LicensePlate))
            {
                throw new AppException("Vehicle with this license plate already exists for the owner.", StatusCodes.Status409Conflict);
            }

            existing.LicensePlate = updatedVehicle.LicensePlate;
            existing.Make = updatedVehicle.Make;
            existing.Model = updatedVehicle.Model;
            existing.Color = updatedVehicle.Color;
            existing.VehicleType = updatedVehicle.VehicleType;
            existing.IsEV = updatedVehicle.IsEV;
            existing.IsActive = updatedVehicle.IsActive;

            await _repo.Update(existing);
            return existing;
        }

        public async Task DeleteVehicle(int vehicleId)
        {
            var existing = await _repo.FindByVehicleId(vehicleId)
                ?? throw new AppException("Vehicle not found.", StatusCodes.Status404NotFound);

            await _repo.DeleteByVehicleId(existing.VehicleId);
        }

        public async Task<string> GetVehicleType(int vehicleId)
        {
            var vehicle = await _repo.FindByVehicleId(vehicleId)
                ?? throw new AppException("Vehicle not found.", StatusCodes.Status404NotFound);

            return vehicle.VehicleType;
        }

        public async Task<bool> IsEVVehicle(int vehicleId)
        {
            var vehicle = await _repo.FindByVehicleId(vehicleId)
                ?? throw new AppException("Vehicle not found.", StatusCodes.Status404NotFound);

            return vehicle.IsEV;
        }

        public Task<List<Vehicle>> GetAllVehicles()
        {
            return _repo.FindAll();
        }

        private static void NormalizeVehicle(Vehicle vehicle)
        {
            vehicle.LicensePlate = NormalizePlate(vehicle.LicensePlate);
            vehicle.Make = vehicle.Make.Trim();
            vehicle.Model = vehicle.Model.Trim();
            vehicle.Color = vehicle.Color.Trim();
            vehicle.VehicleType = NormalizeVehicleType(vehicle.VehicleType);

            if (string.IsNullOrWhiteSpace(vehicle.Make)
                || string.IsNullOrWhiteSpace(vehicle.Model)
                || string.IsNullOrWhiteSpace(vehicle.Color))
            {
                throw new AppException("Make, Model, and Color are required.", StatusCodes.Status400BadRequest);
            }
        }

        private static string NormalizePlate(string licensePlate)
        {
            if (string.IsNullOrWhiteSpace(licensePlate))
            {
                throw new AppException("License plate is required.", StatusCodes.Status400BadRequest);
            }

            return licensePlate.Trim().ToUpperInvariant();
        }

        private static string NormalizeVehicleType(string vehicleType)
        {
            if (string.IsNullOrWhiteSpace(vehicleType))
            {
                throw new AppException("Vehicle type is required.", StatusCodes.Status400BadRequest);
            }

            var normalized = vehicleType.Trim().ToUpperInvariant().Replace(" ", "-");

            normalized = normalized switch
            {
                "2W" => "2-WHEELER",
                "2WHEELER" => "2-WHEELER",
                "TWO-WHEELER" => "2-WHEELER",
                "4W" => "4-WHEELER",
                "4WHEELER" => "4-WHEELER",
                "FOUR-WHEELER" => "4-WHEELER",
                _ => normalized
            };

            if (!AllowedVehicleTypes.Contains(normalized))
            {
                throw new AppException("VehicleType must be one of: 2-WHEELER, 4-WHEELER, HEAVY.", StatusCodes.Status400BadRequest);
            }

            return normalized;
        }
    }
}
