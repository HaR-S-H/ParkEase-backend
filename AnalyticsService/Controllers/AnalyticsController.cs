using AnalyticsService.Services;
using Microsoft.AspNetCore.Mvc;

namespace AnalyticsService.Controllers
{
    [ApiController]
    [Route("api/v1/analytics")]
    public class AnalyticsController : ControllerBase
    {
        private readonly IAnalyticsService _analyticsService;

        public AnalyticsController(IAnalyticsService analyticsService)
        {
            _analyticsService = analyticsService;
        }

        [HttpGet("occupancyRate/{lotId:int}")]
        public async Task<IActionResult> GetOccupancyRate([FromRoute] int lotId)
        {
            var value = await _analyticsService.GetOccupancyRate(lotId);
            return Ok(new { lotId, occupancyRate = value });
        }

        [HttpGet("byHour/{lotId:int}")]
        public async Task<IActionResult> GetOccupancyByHour([FromRoute] int lotId, [FromQuery] DateOnly? date)
        {
            var value = await _analyticsService.GetOccupancyByHour(lotId, date);
            return Ok(value);
        }

        [HttpGet("peakHours/{lotId:int}")]
        public async Task<IActionResult> GetPeakHours([FromRoute] int lotId)
        {
            var value = await _analyticsService.GetPeakHours(lotId);
            return Ok(value);
        }

        [HttpGet("revenue/{lotId:int}")]
        public async Task<IActionResult> GetRevenue([FromRoute] int lotId)
        {
            var value = await _analyticsService.GetRevenueByLot(lotId);
            return Ok(new { lotId, revenue = value });
        }

        [HttpGet("revenueByDay/{lotId:int}")]
        public async Task<IActionResult> GetRevenueByDay([FromRoute] int lotId, [FromQuery] DateOnly? from, [FromQuery] DateOnly? to)
        {
            var value = await _analyticsService.GetRevenueByDay(lotId, from, to);
            return Ok(value);
        }

        [HttpGet("spotTypes/{lotId:int}")]
        public async Task<IActionResult> GetSpotTypes([FromRoute] int lotId)
        {
            var value = await _analyticsService.GetMostUsedSpotTypes(lotId);
            return Ok(value);
        }

        [HttpGet("avgDuration/{lotId:int}")]
        public async Task<IActionResult> GetAvgDuration([FromRoute] int lotId)
        {
            var value = await _analyticsService.GetAvgDuration(lotId);
            return Ok(new { lotId, avgDurationHours = value });
        }

        [HttpGet("platformSummary")]
        public async Task<IActionResult> GetPlatformSummary()
        {
            var value = await _analyticsService.GetPlatformSummary();
            return Ok(value);
        }

        [HttpGet("dailyReport/{lotId:int}")]
        public async Task<IActionResult> GetDailyReport([FromRoute] int lotId, [FromQuery] DateOnly date)
        {
            var value = await _analyticsService.GenerateDailyReport(lotId, date);
            return Ok(value);
        }

        [HttpPost("log-occupancy")]
        public async Task<IActionResult> LogOccupancyNow()
        {
            await _analyticsService.LogOccupancy();
            return Accepted(new { message = "Occupancy logging completed." });
        }
    }
}
