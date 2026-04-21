using ParkingSpotService.Models;
using ParkingSpotService.Models.Dtos;
using ParkingSpotService.Repositories;

namespace ParkingSpotService.Services
{
    public class SpotService : ISpotService
    {
        private readonly ISpotRepository _repository;

        public SpotService(ISpotRepository repository)
        {
            _repository = repository;
        }

        public async Task<ParkingSpotResponse> AddSpot(AddSpotRequest request)
        {
            await EnsureSpotNumberIsUnique(request.LotId, request.SpotNumber);

            var now = DateTime.UtcNow;
            var spot = new ParkingSpot
            {
                LotId = request.LotId,
                SpotNumber = request.SpotNumber.Trim(),
                Floor = request.Floor,
                SpotType = request.SpotType,
                VehicleType = request.VehicleType,
                Status = request.Status,
                IsHandicapped = request.IsHandicapped,
                IsEVCharging = request.IsEVCharging,
                PricePerHour = request.PricePerHour,
                CreatedAt = now,
                UpdatedAt = now
            };

            var created = await _repository.AddSpot(spot);
            return ToResponse(created);
        }

        public async Task<List<ParkingSpotResponse>> AddBulkSpots(AddBulkSpotsRequest request)
        {
            if (request.Count <= 0)
            {
                throw new AppException("Count must be greater than zero.", StatusCodes.Status400BadRequest);
            }

            var prefix = string.IsNullOrWhiteSpace(request.SpotNumberPrefix) ? "S" : request.SpotNumberPrefix.Trim();
            var existingSpots = await _repository.FindByLotId(request.LotId);
            var existingNumbers = new HashSet<string>(existingSpots.Select(spot => spot.SpotNumber), StringComparer.OrdinalIgnoreCase);

            var now = DateTime.UtcNow;
            var spots = new List<ParkingSpot>(request.Count);

            for (var index = 0; index < request.Count; index++)
            {
                var spotNumber = $"{prefix}{request.StartNumber + index}";

                if (!existingNumbers.Add(spotNumber))
                {
                    throw new AppException($"Spot number '{spotNumber}' already exists in the lot.", StatusCodes.Status409Conflict);
                }

                spots.Add(new ParkingSpot
                {
                    LotId = request.LotId,
                    SpotNumber = spotNumber,
                    Floor = request.Floor,
                    SpotType = request.SpotType,
                    VehicleType = request.VehicleType,
                    Status = request.Status,
                    IsHandicapped = request.IsHandicapped,
                    IsEVCharging = request.IsEVCharging,
                    PricePerHour = request.PricePerHour,
                    CreatedAt = now,
                    UpdatedAt = now
                });
            }

            await _repository.AddRange(spots);
            return spots.Select(ToResponse).ToList();
        }

        public async Task<ParkingSpotResponse?> GetSpotById(int spotId)
        {
            var spot = await _repository.FindBySpotId(spotId);
            return spot == null ? null : ToResponse(spot);
        }

        public async Task<List<ParkingSpotResponse>> GetSpotsByLot(int lotId)
        {
            var spots = await _repository.FindByLotId(lotId);
            return spots.Select(ToResponse).ToList();
        }

        public async Task<List<ParkingSpotResponse>> GetAvailableSpots(int lotId)
        {
            var spots = await _repository.FindByLotIdAndStatus(lotId, SpotStatus.AVAILABLE);
            return spots.Select(ToResponse).ToList();
        }

        public async Task<List<ParkingSpotResponse>> GetByTypeAndLot(int lotId, SpotType spotType)
        {
            var spots = await _repository.FindByLotIdAndSpotType(lotId, spotType);
            return spots.Select(ToResponse).ToList();
        }

        public async Task<ParkingSpotResponse> OccupySpot(int spotId)
        {
            var spot = await GetExistingSpot(spotId);

            if (spot.Status == SpotStatus.OCCUPIED)
            {
                throw new AppException("Spot is already occupied.", StatusCodes.Status409Conflict);
            }

            spot.Status = SpotStatus.OCCUPIED;
            spot.UpdatedAt = DateTime.UtcNow;
            await _repository.UpdateSpot(spot);

            return ToResponse(spot);
        }

        public async Task<ParkingSpotResponse> ReleaseSpot(int spotId)
        {
            var spot = await GetExistingSpot(spotId);

            spot.Status = SpotStatus.AVAILABLE;
            spot.UpdatedAt = DateTime.UtcNow;
            await _repository.UpdateSpot(spot);

            return ToResponse(spot);
        }

        public async Task<ParkingSpotResponse> UpdateSpot(int spotId, UpdateSpotRequest request)
        {
            var spot = await GetExistingSpot(spotId);

            if (!string.IsNullOrWhiteSpace(request.SpotNumber))
            {
                var trimmedSpotNumber = request.SpotNumber.Trim();
                await EnsureSpotNumberIsUnique(spot.LotId, trimmedSpotNumber, spot.SpotId);
                spot.SpotNumber = trimmedSpotNumber;
            }

            if (request.Floor.HasValue)
            {
                spot.Floor = request.Floor.Value;
            }

            if (request.SpotType.HasValue)
            {
                spot.SpotType = request.SpotType.Value;
            }

            if (request.VehicleType.HasValue)
            {
                spot.VehicleType = request.VehicleType.Value;
            }

            if (request.Status.HasValue)
            {
                spot.Status = request.Status.Value;
            }

            if (request.IsHandicapped.HasValue)
            {
                spot.IsHandicapped = request.IsHandicapped.Value;
            }

            if (request.IsEVCharging.HasValue)
            {
                spot.IsEVCharging = request.IsEVCharging.Value;
            }

            if (request.PricePerHour.HasValue)
            {
                spot.PricePerHour = request.PricePerHour.Value;
            }

            spot.UpdatedAt = DateTime.UtcNow;
            await _repository.UpdateSpot(spot);

            return ToResponse(spot);
        }

        public async Task DeleteSpot(int spotId)
        {
            var spot = await _repository.FindBySpotId(spotId);

            if (spot == null)
            {
                throw new AppException("Parking spot not found.", StatusCodes.Status404NotFound);
            }

            await _repository.DeleteBySpotId(spotId);
        }

        public Task<int> CountAvailable(int lotId)
        {
            return _repository.CountByLotIdAndStatus(lotId, SpotStatus.AVAILABLE);
        }

        private async Task<ParkingSpot> GetExistingSpot(int spotId)
        {
            return await _repository.FindBySpotId(spotId)
                ?? throw new AppException("Parking spot not found.", StatusCodes.Status404NotFound);
        }

        private async Task EnsureSpotNumberIsUnique(int lotId, string spotNumber, int? ignoreSpotId = null)
        {
            var spots = await _repository.FindByLotId(lotId);

            if (spots.Any(spot =>
                    spot.SpotNumber.Equals(spotNumber.Trim(), StringComparison.OrdinalIgnoreCase)
                    && (!ignoreSpotId.HasValue || spot.SpotId != ignoreSpotId.Value)))
            {
                throw new AppException($"Spot number '{spotNumber}' already exists in the lot.", StatusCodes.Status409Conflict);
            }
        }

        private static ParkingSpotResponse ToResponse(ParkingSpot spot)
        {
            return new ParkingSpotResponse
            {
                SpotId = spot.SpotId,
                LotId = spot.LotId,
                SpotNumber = spot.SpotNumber,
                Floor = spot.Floor,
                SpotType = spot.SpotType,
                VehicleType = spot.VehicleType,
                Status = spot.Status,
                IsHandicapped = spot.IsHandicapped,
                IsEVCharging = spot.IsEVCharging,
                PricePerHour = spot.PricePerHour,
                CreatedAt = spot.CreatedAt,
                UpdatedAt = spot.UpdatedAt
            };
        }
    }
}