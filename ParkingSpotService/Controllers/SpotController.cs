using ParkingSpotService.Models.Dtos;
using ParkingSpotService.Models;
using ParkingSpotService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ParkingSpotService.Controllers
{
    [ApiController]
    [Route("api/v1/spots")]
    public class SpotController : ControllerBase
    {
        private readonly ISpotService _spotService;

        public SpotController(ISpotService spotService)
        {
            _spotService = spotService;
        }

        [Authorize(Roles = "MANAGER")]
        [HttpPost]
        public async Task<IActionResult> AddSpot([FromBody] AddSpotRequest request)
        {
            var response = await _spotService.AddSpot(request);
            return CreatedAtAction(nameof(GetSpotById), new { spotId = response.SpotId }, response);
        }

        [Authorize(Roles = "MANAGER")]
        [HttpPost("bulk")]
        public async Task<IActionResult> AddBulkSpots([FromBody] AddBulkSpotsRequest request)
        {
            var response = await _spotService.AddBulkSpots(request);
            return StatusCode(StatusCodes.Status201Created, response);
        }

        [HttpGet("{spotId:int}")]
        public async Task<IActionResult> GetSpotById([FromRoute] int spotId)
        {
            var response = await _spotService.GetSpotById(spotId);
            return response == null ? NotFound(new { message = "Parking spot not found." }) : Ok(response);
        }

        [HttpGet("lot/{lotId:int}")]
        public async Task<IActionResult> GetSpotsByLot([FromRoute] int lotId)
        {
            var response = await _spotService.GetSpotsByLot(lotId);
            return Ok(response);
        }

        [HttpGet("lot/{lotId:int}/available")]
        public async Task<IActionResult> GetAvailableSpots([FromRoute] int lotId)
        {
            var response = await _spotService.GetAvailableSpots(lotId);
            return Ok(response);
        }

        [HttpGet("lot/{lotId:int}/type/{spotType}")]
        public async Task<IActionResult> GetByTypeAndLot([FromRoute] int lotId, [FromRoute] SpotType spotType)
        {
            var response = await _spotService.GetByTypeAndLot(lotId, spotType);
            return Ok(response);
        }

        [HttpGet("lot/{lotId:int}/count")]
        public async Task<IActionResult> CountAvailable([FromRoute] int lotId)
        {
            var count = await _spotService.CountAvailable(lotId);
            return Ok(new CountAvailableResponse { LotId = lotId, AvailableCount = count });
        }

        [HttpPut("{spotId:int}/occupy")]
        public async Task<IActionResult> OccupySpot([FromRoute] int spotId)
        {
            var response = await _spotService.OccupySpot(spotId);
            return Ok(response);
        }

        [HttpPut("{spotId:int}/release")]
        public async Task<IActionResult> ReleaseSpot([FromRoute] int spotId)
        {
            var response = await _spotService.ReleaseSpot(spotId);
            return Ok(response);
        }

        [HttpPut("{spotId:int}")]
        public async Task<IActionResult> UpdateSpot([FromRoute] int spotId, [FromBody] UpdateSpotRequest request)
        {
            var response = await _spotService.UpdateSpot(spotId, request);
            return Ok(response);
        }

        [Authorize(Roles = "MANAGER")]
        [HttpDelete("{spotId:int}")]
        public async Task<IActionResult> DeleteSpot([FromRoute] int spotId)
        {
            await _spotService.DeleteSpot(spotId);
            return NoContent();
        }
    }
}