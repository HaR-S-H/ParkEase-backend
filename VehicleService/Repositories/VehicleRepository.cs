using Microsoft.EntityFrameworkCore;
using VehicleService.Data;
using VehicleService.Models;

namespace VehicleService.Repositories
{
    public class VehicleRepository : IVehicleRepository
    {
        private readonly VehicleDbContext _context;

        public VehicleRepository(VehicleDbContext context)
        {
            _context = context;
        }

        public Task<List<Vehicle>> FindByOwnerId(int ownerId)
        {
            return _context.Vehicles
                .AsNoTracking()
                .Where(vehicle => vehicle.OwnerId == ownerId)
                .OrderByDescending(vehicle => vehicle.RegisteredAt)
                .ToListAsync();
        }

        public Task<Vehicle?> FindByLicensePlate(string licensePlate)
        {
            return _context.Vehicles
                .AsNoTracking()
                .FirstOrDefaultAsync(vehicle => vehicle.LicensePlate == licensePlate);
        }

        public Task<Vehicle?> FindByVehicleId(int vehicleId)
        {
            return _context.Vehicles.FirstOrDefaultAsync(vehicle => vehicle.VehicleId == vehicleId);
        }

        public Task<List<Vehicle>> FindByVehicleType(string vehicleType)
        {
            return _context.Vehicles
                .AsNoTracking()
                .Where(vehicle => vehicle.VehicleType == vehicleType)
                .OrderByDescending(vehicle => vehicle.RegisteredAt)
                .ToListAsync();
        }

        public Task<List<Vehicle>> FindByIsEV(bool isEV)
        {
            return _context.Vehicles
                .AsNoTracking()
                .Where(vehicle => vehicle.IsEV == isEV)
                .OrderByDescending(vehicle => vehicle.RegisteredAt)
                .ToListAsync();
        }

        public Task<bool> ExistsByLicensePlate(string licensePlate)
        {
            return _context.Vehicles.AnyAsync(vehicle => vehicle.LicensePlate == licensePlate);
        }

        public Task<bool> ExistsByLicensePlate(int ownerId, string licensePlate)
        {
            return _context.Vehicles.AnyAsync(vehicle => vehicle.OwnerId == ownerId && vehicle.LicensePlate == licensePlate);
        }

        public async Task<Vehicle> Create(Vehicle vehicle)
        {
            _context.Vehicles.Add(vehicle);
            await _context.SaveChangesAsync();
            return vehicle;
        }

        public async Task Update(Vehicle vehicle)
        {
            _context.Vehicles.Update(vehicle);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteByVehicleId(int vehicleId)
        {
            var vehicle = await _context.Vehicles.FindAsync(vehicleId);
            if (vehicle != null)
            {
                _context.Vehicles.Remove(vehicle);
                await _context.SaveChangesAsync();
            }
        }

        public Task<List<Vehicle>> FindAll()
        {
            return _context.Vehicles
                .AsNoTracking()
                .OrderByDescending(vehicle => vehicle.RegisteredAt)
                .ToListAsync();
        }
    }
}
