using Microsoft.EntityFrameworkCore;
using ParkingSpotService.Data;
using ParkingSpotService.Models;

namespace ParkingSpotService.Repositories
{
    public class SpotRepository : ISpotRepository
    {
        private readonly ParkingSpotDbContext _context;

        public SpotRepository(ParkingSpotDbContext context)
        {
            _context = context;
        }

        public async Task<ParkingSpot> AddSpot(ParkingSpot spot)
        {
            _context.ParkingSpots.Add(spot);
            await _context.SaveChangesAsync();
            return spot;
        }

        public async Task AddRange(IEnumerable<ParkingSpot> spots)
        {
            _context.ParkingSpots.AddRange(spots);
            await _context.SaveChangesAsync();
        }

        public Task<List<ParkingSpot>> FindByLotId(int lotId)
        {
            return _context.ParkingSpots
                .AsNoTracking()
                .Where(spot => spot.LotId == lotId)
                .OrderBy(spot => spot.SpotNumber)
                .ToListAsync();
        }

        public Task<List<ParkingSpot>> FindByLotIdAndStatus(int lotId, SpotStatus status)
        {
            return _context.ParkingSpots
                .AsNoTracking()
                .Where(spot => spot.LotId == lotId && spot.Status == status)
                .OrderBy(spot => spot.SpotNumber)
                .ToListAsync();
        }

        public Task<List<ParkingSpot>> FindByLotIdAndSpotType(int lotId, SpotType spotType)
        {
            return _context.ParkingSpots
                .AsNoTracking()
                .Where(spot => spot.LotId == lotId && spot.SpotType == spotType)
                .OrderBy(spot => spot.SpotNumber)
                .ToListAsync();
        }

        public Task<List<ParkingSpot>> FindByLotIdAndVehicleType(int lotId, VehicleType vehicleType)
        {
            return _context.ParkingSpots
                .AsNoTracking()
                .Where(spot => spot.LotId == lotId && spot.VehicleType == vehicleType)
                .OrderBy(spot => spot.SpotNumber)
                .ToListAsync();
        }

        public Task<ParkingSpot?> FindBySpotId(int spotId)
        {
            return _context.ParkingSpots.FirstOrDefaultAsync(spot => spot.SpotId == spotId);
        }

        public Task<int> CountByLotIdAndStatus(int lotId, SpotStatus status)
        {
            return _context.ParkingSpots.CountAsync(spot => spot.LotId == lotId && spot.Status == status);
        }

        public Task<List<ParkingSpot>> FindByIsEVCharging(bool isEVCharging)
        {
            return _context.ParkingSpots
                .AsNoTracking()
                .Where(spot => spot.IsEVCharging == isEVCharging)
                .OrderBy(spot => spot.LotId)
                .ThenBy(spot => spot.SpotNumber)
                .ToListAsync();
        }

        public async Task UpdateSpot(ParkingSpot spot)
        {
            _context.ParkingSpots.Update(spot);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteBySpotId(int spotId)
        {
            var spot = await _context.ParkingSpots.FirstOrDefaultAsync(item => item.SpotId == spotId);

            if (spot != null)
            {
                _context.ParkingSpots.Remove(spot);
                await _context.SaveChangesAsync();
            }
        }
    }
}