using ParkingLotService.Data;
using ParkingLotService.Models;
using Microsoft.EntityFrameworkCore;

namespace ParkingLotService.Repositories
{
    public class ParkingLotRepository : IParkingLotRepository
    {
        private readonly ParkingLotDbContext _context;

        public ParkingLotRepository(ParkingLotDbContext context)
        {
            _context = context;
        }

        public Task<ParkingLot?> FindByLotId(int lotId)
        {
            return _context.ParkingLots
                .FirstOrDefaultAsync(lot => lot.LotId == lotId);
        }

        public Task<List<ParkingLot>> FindByCity(string city)
        {
            var normalizedCity = city.Trim().ToLowerInvariant();

            return _context.ParkingLots
                .AsNoTracking()
                .Where(lot => lot.City.ToLower() == normalizedCity)
                .ToListAsync();
        }

        public Task<List<ParkingLot>> FindByManagerId(int managerId)
        {
            return _context.ParkingLots
                .AsNoTracking()
                .Where(lot => lot.ManagerId == managerId)
                .ToListAsync();
        }

        public Task<List<ParkingLot>> FindByIsOpen(bool isOpen)
        {
            return _context.ParkingLots
                .AsNoTracking()
                .Where(lot => lot.IsOpen == isOpen)
                .ToListAsync();
        }

        public async Task<List<ParkingLot>> FindNearby(double latitude, double longitude, double radiusKm)
        {
            var lots = await _context.ParkingLots
                .AsNoTracking()
                .ToListAsync();

            return lots
                .Where(lot => DistanceKm(latitude, longitude, lot.Latitude, lot.Longitude) <= radiusKm)
                .ToList();
        }

        public Task<List<ParkingLot>> FindByAvailableSpotsGreaterThan(int availableSpots)
        {
            return _context.ParkingLots
                .AsNoTracking()
                .Where(lot => lot.AvailableSpots > availableSpots)
                .ToListAsync();
        }

        public Task<int> CountByCity(string city)
        {
            var normalizedCity = city.Trim().ToLowerInvariant();

            return _context.ParkingLots
                .CountAsync(lot => lot.City.ToLower() == normalizedCity);
        }

        public Task<List<ParkingLot>> SearchLots(string query)
        {
            var normalizedQuery = query.Trim().ToLowerInvariant();

            return _context.ParkingLots
                .AsNoTracking()
                .Where(lot => lot.Name.ToLower().Contains(normalizedQuery)
                    || lot.Address.ToLower().Contains(normalizedQuery)
                    || lot.City.ToLower().Contains(normalizedQuery))
                .ToListAsync();
        }

        public Task<List<ParkingLot>> GetAll()
        {
            return _context.ParkingLots
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<ParkingLot> Create(ParkingLot parkingLot)
        {
            _context.ParkingLots.Add(parkingLot);
            await _context.SaveChangesAsync();
            return parkingLot;
        }

        public async Task Update(ParkingLot parkingLot)
        {
            var entry = _context.Entry(parkingLot);
            if (entry.State == EntityState.Detached)
            {
                _context.ParkingLots.Update(parkingLot);
            }
            await _context.SaveChangesAsync();
        }

        public async Task<bool> UpdateImageUrl(int lotId, string imageUrl)
        {
            var parkingLot = await _context.ParkingLots.FirstOrDefaultAsync(lot => lot.LotId == lotId);
            if (parkingLot == null)
            {
                return false;
            }

            parkingLot.ImageUrl = imageUrl;
            parkingLot.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task DeleteByLotId(int lotId)
        {
            var parkingLot = await _context.ParkingLots.FindAsync(lotId);
            if (parkingLot != null)
            {
                _context.ParkingLots.Remove(parkingLot);
                await _context.SaveChangesAsync();
            }
        }

        public Task<int> SaveChanges()
        {
            return _context.SaveChangesAsync();
        }

        private static double DistanceKm(double latitude1, double longitude1, double latitude2, double longitude2)
        {
            const double earthRadiusKm = 6371.0;

            var dLat = DegreesToRadians(latitude2 - latitude1);
            var dLon = DegreesToRadians(longitude2 - longitude1);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
                + Math.Cos(DegreesToRadians(latitude1)) * Math.Cos(DegreesToRadians(latitude2))
                * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return earthRadiusKm * c;
        }

        private static double DegreesToRadians(double degrees)
        {
            return degrees * (Math.PI / 180.0);
        }
    }
}