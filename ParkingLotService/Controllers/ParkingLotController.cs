using ParkingLotService.Models.Dtos;
using ParkingLotService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ParkingLotService.Controllers
{
    [ApiController]
    [Route("api/v1/lots")]
    public class ParkingLotController : ControllerBase
    {
        private readonly IParkingLotService _parkingLotService;

        public ParkingLotController(IParkingLotService parkingLotService)
        {
            _parkingLotService = parkingLotService;
        }

        [HttpPost]
        [Authorize(Roles = "MANAGER")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CreateLot([FromForm] CreateParkingLotRequest request)
        {
            var response = await _parkingLotService.CreateLot(request);
            return CreatedAtAction(nameof(GetLotById), new { lotId = response.LotId }, response);
        }

        [HttpGet("{lotId:int}")]
        public async Task<IActionResult> GetLotById([FromRoute] int lotId)
        {
            var response = await _parkingLotService.GetLotById(lotId);
            return response == null ? NotFound(new { message = "Parking lot not found." }) : Ok(response);
        }

        [HttpGet("city/{city}")]
        public async Task<IActionResult> GetLotsByCity([FromRoute] string city)
        {
            var response = await _parkingLotService.GetLotsByCity(city);
            return Ok(response);
        }

        [HttpGet("manager/{managerId:int}")]
        public async Task<IActionResult> GetLotsByManager([FromRoute] int managerId)
        {
            var response = await _parkingLotService.GetLotsByManager(managerId);
            return Ok(response);
        }

        [HttpGet("nearby")]
        public async Task<IActionResult> GetNearbyLots([FromQuery] double latitude, [FromQuery] double longitude, [FromQuery] double radiusKm = 5)
        {
            var response = await _parkingLotService.GetNearbyLots(latitude, longitude, radiusKm);
            return Ok(response);
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchLots([FromQuery] string query)
        {
            var response = await _parkingLotService.SearchLots(query);
            return Ok(response);
        }

        [HttpPut("{lotId:int}")]
        [Authorize(Roles = "MANAGER")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateLot([FromRoute] int lotId, [FromForm] UpdateParkingLotRequest request)
        {
            var response = await _parkingLotService.UpdateLot(lotId, request);
            return Ok(response);
        }

        [HttpPut("{lotId:int}/toggle-open")]
        [Authorize(Roles = "MANAGER")]
        public async Task<IActionResult> ToggleOpen([FromRoute] int lotId)
        {
            var response = await _parkingLotService.ToggleOpen(lotId);
            return Ok(response);
        }

        [HttpDelete("{lotId:int}")]
        [Authorize(Roles = "MANAGER")]
        public async Task<IActionResult> DeleteLot([FromRoute] int lotId)
        {
            await _parkingLotService.DeleteLot(lotId);
            return NoContent();
        }
    }
}