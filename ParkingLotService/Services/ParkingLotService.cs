using ParkingLotService.Messaging;
using ParkingLotService.Messaging.Messages;
using ParkingLotService.Models;
using ParkingLotService.Models.Dtos;
using ParkingLotService.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace ParkingLotService.Services
{
    public class ParkingLotService : IParkingLotService
    {
        private readonly IParkingLotRepository _repository;
        private readonly IRabbitMqPublisher _rabbitMqPublisher;
        private readonly IConfiguration _configuration;

        public ParkingLotService(
            IParkingLotRepository repository,
            IRabbitMqPublisher rabbitMqPublisher,
            IConfiguration configuration)
        {
            _repository = repository;
            _rabbitMqPublisher = rabbitMqPublisher;
            _configuration = configuration;
        }

        public async Task<ParkingLotResponse> CreateLot(CreateParkingLotRequest request)
        {
            // if (request.AvailableSpots.HasValue && request.AvailableSpots.Value > request.TotalSpots)
            // {
            //     throw new AppException("Available spots cannot exceed total spots.", StatusCodes.Status400BadRequest);
            // }

            var parkingLot = new ParkingLot
            {
                Name = request.Name.Trim(),
                Address = request.Address.Trim(),
                City = request.City.Trim(),
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                ManagerId = request.ManagerId,
                IsOpen = request.IsOpen,
                IsApproved = request.IsApproved,
                OpenTime = request.OpenTime,
                CloseTime = request.CloseTime,
                TotalSpots = request.TotalSpots,
                AvailableSpots = request.AvailableSpots,
                ImageUrl = null,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            parkingLot = await _repository.Create(parkingLot);

            if (request.ImageFile != null && request.ImageFile.Length > 0)
            {
                await QueueImageUpload(parkingLot.LotId, request.ImageFile);
            }

            return ToResponse(parkingLot);
        }

        public async Task QueueImageUpload(int lotId, IFormFile imageFile, CancellationToken cancellationToken = default)
        {
            if (imageFile.Length <= 0)
            {
                throw new AppException("Image file cannot be empty.", StatusCodes.Status400BadRequest);
            }

            _ = await RequireLot(lotId);

            await using var stream = imageFile.OpenReadStream();
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream, cancellationToken);

            var message = new ParkingLotImageUploadRequestedMessage
            {
                LotId = lotId,
                FileName = imageFile.FileName,
                ContentType = string.IsNullOrWhiteSpace(imageFile.ContentType) ? "application/octet-stream" : imageFile.ContentType,
                Base64Content = Convert.ToBase64String(memoryStream.ToArray())
            };

            await _rabbitMqPublisher.Publish(GetImageUploadQueueName(), message, cancellationToken);
        }

        public async Task<ParkingLotResponse?> GetLotById(int lotId)
        {
            var parkingLot = await _repository.FindByLotId(lotId);
            return parkingLot == null ? null : ToResponse(parkingLot);
        }

        public async Task<List<ParkingLotResponse>> GetLotsByCity(string city)
        {
            var lots = await _repository.FindByCity(city);
            return lots
                .Where(lot => lot.IsApproved)
                .Select(ToResponse)
                .ToList();
        }

        public async Task<List<ParkingLotResponse>> GetNearbyLots(double latitude, double longitude, double radiusKm)
        {
            var lots = await _repository.FindNearby(latitude, longitude, radiusKm);
            return lots
                .Where(lot => lot.IsApproved)
                .Select(ToResponse)
                .ToList();
        }

        public async Task<List<ParkingLotResponse>> GetLotsByManager(int managerId)
        {
            var lots = await _repository.FindByManagerId(managerId);
            return lots.Select(ToResponse).ToList();
        }

        public async Task<ParkingLotResponse> UpdateLot(int lotId, UpdateParkingLotRequest request)
        {
            return await ExecuteWithConcurrencyRetry(async () =>
            {
                var parkingLot = await RequireLot(lotId);

                var nextTotalSpots = request.TotalSpots ?? parkingLot.TotalSpots;
                var nextAvailableSpots = request.AvailableSpots ?? parkingLot.AvailableSpots;

                if (nextAvailableSpots > nextTotalSpots)
                {
                    throw new AppException("Available spots cannot exceed total spots.", StatusCodes.Status400BadRequest);
                }

                if (!string.IsNullOrWhiteSpace(request.Name))
                {
                    parkingLot.Name = request.Name.Trim();
                }

                if (!string.IsNullOrWhiteSpace(request.Address))
                {
                    parkingLot.Address = request.Address.Trim();
                }

                if (!string.IsNullOrWhiteSpace(request.City))
                {
                    parkingLot.City = request.City.Trim();
                }

                if (request.Latitude.HasValue)
                {
                    parkingLot.Latitude = request.Latitude.Value;
                }

                if (request.Longitude.HasValue)
                {
                    parkingLot.Longitude = request.Longitude.Value;
                }

                if (request.TotalSpots.HasValue)
                {
                    parkingLot.TotalSpots = request.TotalSpots.Value;
                }

                if (request.AvailableSpots.HasValue)
                {
                    parkingLot.AvailableSpots = request.AvailableSpots.Value;
                }

                if (request.ManagerId.HasValue)
                {
                    parkingLot.ManagerId = request.ManagerId.Value;
                }

                if (request.IsOpen.HasValue)
                {
                    parkingLot.IsOpen = request.IsOpen.Value;
                }

                if (request.IsApproved.HasValue)
                {
                    parkingLot.IsApproved = request.IsApproved.Value;
                }

                if (request.OpenTime.HasValue)
                {
                    parkingLot.OpenTime = request.OpenTime.Value;
                }

                if (request.CloseTime.HasValue)
                {
                    parkingLot.CloseTime = request.CloseTime.Value;
                }

                parkingLot.UpdatedAt = DateTime.UtcNow;

                await _repository.Update(parkingLot);

                if (request.ImageFile != null && request.ImageFile.Length > 0)
                {
                    await QueueImageUpload(parkingLot.LotId, request.ImageFile);
                }

                return ToResponse(parkingLot);
            });
        }

        public async Task<ParkingLotResponse> ToggleOpen(int lotId)
        {
            return await ExecuteWithConcurrencyRetry(async () =>
            {
                var parkingLot = await RequireLot(lotId);
                parkingLot.IsOpen = !parkingLot.IsOpen;
                parkingLot.UpdatedAt = DateTime.UtcNow;

                await _repository.Update(parkingLot);
                return ToResponse(parkingLot);
            });
        }

        public async Task<ParkingLotResponse> ApproveLot(int lotId)
        {
            return await ExecuteWithConcurrencyRetry(async () =>
            {
                var parkingLot = await RequireLot(lotId);
                parkingLot.IsApproved = true;
                parkingLot.UpdatedAt = DateTime.UtcNow;

                await _repository.Update(parkingLot);
                return ToResponse(parkingLot);
            });
        }

        public async Task DeleteLot(int lotId)
        {
            await _repository.DeleteByLotId(lotId);
        }

        public async Task<ParkingLotResponse> DecrementAvailable(int lotId, int quantity = 1)
        {
            if (quantity <= 0)
            {
                throw new AppException("Quantity must be greater than zero.", StatusCodes.Status400BadRequest);
            }

            return await ExecuteWithConcurrencyRetry(async () =>
            {
                var lot = await RequireLot(lotId);

                if (lot.AvailableSpots < quantity)
                {
                    throw new AppException($"Not enough available spots. (Available: {lot.AvailableSpots}, Requested: {quantity})", StatusCodes.Status409Conflict);
                }

                lot.AvailableSpots -= quantity;
                lot.UpdatedAt = DateTime.UtcNow;

                await _repository.Update(lot);
                return ToResponse(lot);
            });
        }

        public async Task<ParkingLotResponse> IncrementAvailable(int lotId, int quantity = 1)
        {
            if (quantity <= 0)
            {
                throw new AppException("Quantity must be greater than zero.", StatusCodes.Status400BadRequest);
            }

            return await ExecuteWithConcurrencyRetry(async () =>
            {
                var lot = await RequireLot(lotId);

                if (lot.AvailableSpots + quantity > lot.TotalSpots)
                {
                    throw new AppException("Available spots cannot exceed total spots.", StatusCodes.Status400BadRequest);
                }

                lot.AvailableSpots += quantity;
                lot.UpdatedAt = DateTime.UtcNow;

                await _repository.Update(lot);
                return ToResponse(lot);
            });
        }

        public async Task<ParkingLotResponse> IncrementTotalSpots(int lotId, int quantity = 1)
        {
            if (quantity <= 0)
            {
                throw new AppException("Quantity must be greater than zero.", StatusCodes.Status400BadRequest);
            }

            return await ExecuteWithConcurrencyRetry(async () =>
            {
                var lot = await RequireLot(lotId);

                lot.TotalSpots += quantity;
                lot.UpdatedAt = DateTime.UtcNow;

                if (lot.AvailableSpots > lot.TotalSpots)
                {
                    lot.AvailableSpots = lot.TotalSpots;
                }

                await _repository.Update(lot);
                return ToResponse(lot);
            });
        }

        public async Task<List<ParkingLotResponse>> SearchLots(string? query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                var allLots = await _repository.FindByIsOpen(true);
                return allLots
                    .Where(lot => lot.IsApproved)
                    .Select(ToResponse)
                    .ToList();
            }

            var lots = await _repository.SearchLots(query);
            return lots
                .Where(lot => lot.IsApproved)
                .Select(ToResponse)
                .ToList();
        }

        public async Task<List<ParkingLotResponse>> GetAllLotsForAdmin()
        {
            var lots = await _repository.GetAll();
            return lots.Select(ToResponse).ToList();
        }

        private async Task<ParkingLot> RequireLot(int lotId)
        {
            var parkingLot = await _repository.FindByLotId(lotId);
            if (parkingLot == null)
            {
                throw new AppException("Parking lot not found.", StatusCodes.Status404NotFound);
            }

            return parkingLot;
        }

        private async Task<ParkingLotResponse> ExecuteWithConcurrencyRetry(Func<Task<ParkingLotResponse>> operation, int maxAttempts = 3)
        {
            for (var attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    return await operation();
                }
                catch (DbUpdateConcurrencyException) when (attempt < maxAttempts)
                {
                    continue;
                }
            }

            throw new AppException("Concurrency conflict: The parking lot was updated by another operation. Please retry.", StatusCodes.Status409Conflict);
        }

        private static ParkingLotResponse ToResponse(ParkingLot parkingLot)
        {
            return new ParkingLotResponse
            {
                LotId = parkingLot.LotId,
                Name = parkingLot.Name,
                Address = parkingLot.Address,
                City = parkingLot.City,
                Latitude = parkingLot.Latitude,
                Longitude = parkingLot.Longitude,
                ManagerId = parkingLot.ManagerId,
                IsOpen = parkingLot.IsOpen,
                IsApproved = parkingLot.IsApproved,
                OpenTime = parkingLot.OpenTime,
                CloseTime = parkingLot.CloseTime,
                TotalSpots = parkingLot.TotalSpots,
                AvailableSpots = parkingLot.AvailableSpots,
                ImageUrl = parkingLot.ImageUrl,
                CreatedAt = parkingLot.CreatedAt,
                UpdatedAt = parkingLot.UpdatedAt
            };
        }

        private string GetImageUploadQueueName()
        {
            return _configuration["RabbitMQ:Queues:ParkingLotImageUpload"]
                ?? throw new InvalidOperationException("Missing RabbitMQ:Queues:ParkingLotImageUpload.");
        }
    }
}