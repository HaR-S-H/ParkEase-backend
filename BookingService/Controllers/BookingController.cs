using BookingService.Models.Dtos;
using BookingService.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookingService.Controllers
{
    [ApiController]
    [Route("api/v1/bookings")]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public BookingController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateBookingRequest request)
        {
            var response = await _bookingService.CreateBooking(request);
            return CreatedAtAction(nameof(GetById), new { bookingId = response.BookingId }, response);
        }

        [HttpGet("{bookingId:int}")]
        public async Task<IActionResult> GetById([FromRoute] int bookingId)
        {
            var response = await _bookingService.GetBookingById(bookingId);
            return response == null ? NotFound(new { message = "Booking not found." }) : Ok(response);
        }

        [HttpGet("user/{userId:int}")]
        public async Task<IActionResult> GetByUser([FromRoute] int userId)
        {
            var response = await _bookingService.GetBookingsByUser(userId);
            return Ok(response);
        }

        [HttpGet("lot/{lotId:int}")]
        public async Task<IActionResult> GetByLot([FromRoute] int lotId)
        {
            var response = await _bookingService.GetBookingsByLot(lotId);
            return Ok(response);
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActive()
        {
            var response = await _bookingService.GetActiveBookings();
            return Ok(response);
        }

        [HttpPut("{bookingId:int}/cancel")]
        public async Task<IActionResult> Cancel([FromRoute] int bookingId)
        {
            await _bookingService.CancelBooking(bookingId);
            return NoContent();
        }

        [HttpPut("{bookingId:int}/checkin")]
        public async Task<IActionResult> CheckIn([FromRoute] int bookingId)
        {
            var response = await _bookingService.CheckIn(bookingId);
            return Ok(response);
        }

        [HttpPut("{bookingId:int}/checkout")]
        public async Task<IActionResult> CheckOut([FromRoute] int bookingId)
        {
            var response = await _bookingService.CheckOut(bookingId);
            return Ok(response);
        }

        [HttpPut("{bookingId:int}/extend")]
        public async Task<IActionResult> Extend([FromRoute] int bookingId, [FromBody] ExtendBookingRequest request)
        {
            var response = await _bookingService.ExtendBooking(bookingId, request.NewEndTime);
            return Ok(response);
        }

        [HttpGet("{bookingId:int}/calculate-amount")]
        public async Task<IActionResult> CalculateAmount([FromRoute] int bookingId)
        {
            var amount = await _bookingService.CalculateAmount(bookingId);
            return Ok(new CalculateAmountResponse { BookingId = bookingId, Amount = amount });
        }

        [HttpGet("user/{userId:int}/history")]
        public async Task<IActionResult> GetBookingHistory([FromRoute] int userId)
        {
            var response = await _bookingService.GetBookingHistory(userId);
            return Ok(response);
        }
    }
}
