using Microsoft.AspNetCore.Mvc;
using VehicleService.Models;
using VehicleService.Models.Dtos;
using VehicleService.Services;

namespace VehicleService.Controllers
{
    [ApiController]
    [Route("api/v1/vehicles")]
    public class VehicleController : ControllerBase
    {
        private readonly IVehicleService _vehicleService;

        public VehicleController(IVehicleService vehicleService)
        {
            _vehicleService = vehicleService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterVehicleRequest request)
        {
            var vehicle = new Vehicle
            {
                OwnerId = request.OwnerId,
                LicensePlate = request.LicensePlate,
                Make = request.Make,
                Model = request.Model,
                Color = request.Color,
                VehicleType = request.VehicleType,
                IsEV = request.IsEV
            };

            var created = await _vehicleService.RegisterVehicle(vehicle);
            var response = ToResponse(created);

            return CreatedAtAction(nameof(GetById), new { vehicleId = response.VehicleId }, response);
        }

        [HttpGet("{vehicleId:int}")]
        public async Task<IActionResult> GetById([FromRoute] int vehicleId)
        {
            var vehicle = await _vehicleService.GetVehicleById(vehicleId);
            return vehicle == null
                ? NotFound(new { message = "Vehicle not found." })
                : Ok(ToResponse(vehicle));
        }

        [HttpGet("owner/{ownerId:int}")]
        public async Task<IActionResult> GetByOwner([FromRoute] int ownerId)
        {
            var vehicles = await _vehicleService.GetVehiclesByOwner(ownerId);
            return Ok(vehicles.Select(ToResponse).ToList());
        }

        [HttpGet("plate/{licensePlate}")]
        public async Task<IActionResult> GetByPlate([FromRoute] string licensePlate)
        {
            var vehicle = await _vehicleService.GetByLicensePlate(licensePlate);
            return vehicle == null
                ? NotFound(new { message = "Vehicle not found." })
                : Ok(ToResponse(vehicle));
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            var vehicles = await _vehicleService.GetAllVehicles();
            return Ok(vehicles.Select(ToResponse).ToList());
        }

        [HttpPut("{vehicleId:int}")]
        public async Task<IActionResult> Update([FromRoute] int vehicleId, [FromBody] UpdateVehicleRequest request)
        {
            var updated = await _vehicleService.UpdateVehicle(vehicleId, new Vehicle
            {
                LicensePlate = request.LicensePlate,
                Make = request.Make,
                Model = request.Model,
                Color = request.Color,
                VehicleType = request.VehicleType,
                IsEV = request.IsEV,
                IsActive = request.IsActive
            });

            return Ok(ToResponse(updated));
        }

        [HttpDelete("{vehicleId:int}")]
        public async Task<IActionResult> Delete([FromRoute] int vehicleId)
        {
            await _vehicleService.DeleteVehicle(vehicleId);
            return NoContent();
        }

        [HttpGet("type/{vehicleId:int}")]
        public async Task<IActionResult> GetType([FromRoute] int vehicleId)
        {
            var vehicleType = await _vehicleService.GetVehicleType(vehicleId);
            return Ok(new { vehicleId, vehicleType });
        }

        [HttpGet("is-ev/{vehicleId:int}")]
        public async Task<IActionResult> IsEV([FromRoute] int vehicleId)
        {
            var isEV = await _vehicleService.IsEVVehicle(vehicleId);
            return Ok(new { vehicleId, isEV });
        }

        private static VehicleResponse ToResponse(Vehicle vehicle)
        {
            return new VehicleResponse
            {
                VehicleId = vehicle.VehicleId,
                OwnerId = vehicle.OwnerId,
                LicensePlate = vehicle.LicensePlate,
                Make = vehicle.Make,
                Model = vehicle.Model,
                Color = vehicle.Color,
                VehicleType = vehicle.VehicleType,
                IsEV = vehicle.IsEV,
                RegisteredAt = vehicle.RegisteredAt,
                IsActive = vehicle.IsActive
            };
        }
    }
}
