using Microsoft.AspNetCore.Mvc;
using PaymentService.Models.Dtos;
using PaymentService.Services;

namespace PaymentService.Controllers
{
    [ApiController]
    [Route("api/v1/payments")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _payService;

        public PaymentController(IPaymentService payService)
        {
            _payService = payService;
        }

        [HttpPost("process")]
        public async Task<IActionResult> Process([FromBody] ProcessPaymentRequest request)
        {
            var response = await _payService.ProcessPayment(request);
            return CreatedAtAction(nameof(GetByBooking), new { bookingId = response.BookingId }, response);
        }

        [HttpPost("charge")]
        public async Task<IActionResult> Charge([FromBody] ProcessPaymentRequest request)
        {
            var response = await _payService.ProcessPayment(request);
            return Ok(response);
        }

        [HttpPost("refund")]
        public async Task<IActionResult> Refund([FromBody] RefundPaymentRequest request)
        {
            var response = await _payService.RefundPayment(request.BookingId, request.Reason);
            return Ok(response);
        }

        [HttpGet("booking/{bookingId:int}")]
        public async Task<IActionResult> GetByBooking([FromRoute] int bookingId)
        {
            var response = await _payService.GetByBooking(bookingId);
            return response == null ? NotFound(new { message = "Payment not found." }) : Ok(response);
        }

        [HttpGet("user/{userId:int}")]
        public async Task<IActionResult> GetByUser([FromRoute] int userId)
        {
            var response = await _payService.GetByUser(userId);
            return Ok(response);
        }

        [HttpGet("history/{userId:int}")]
        public async Task<IActionResult> GetHistory([FromRoute] int userId)
        {
            var response = await _payService.GetTransactionHistory(userId);
            return Ok(response);
        }

        [HttpGet("status/{bookingId:int}")]
        public async Task<IActionResult> GetStatus([FromRoute] int bookingId)
        {
            var status = await _payService.GetPaymentStatus(bookingId);
            if (status == "NOT_FOUND")
            {
                return NotFound(new { message = "Payment not found." });
            }

            return Ok(new { bookingId, status });
        }

        [HttpPut("status/{bookingId:int}/{status}")]
        public async Task<IActionResult> UpdateStatus([FromRoute] int bookingId, [FromRoute] string status)
        {
            await _payService.UpdateStatus(bookingId, status);
            return NoContent();
        }

        [HttpGet("receipt/{bookingId:int}")]
        public async Task<IActionResult> GenerateReceipt([FromRoute] int bookingId)
        {
            var receipt = await _payService.GenerateReceipt(bookingId);
            return File(receipt.Content, receipt.ContentType, receipt.FileName);
        }

        [HttpGet("revenue/{lotId:int}")]
        public async Task<IActionResult> GetRevenue([FromRoute] int lotId)
        {
            var total = await _payService.GetTotalRevenue(lotId);
            return Ok(new { lotId, totalRevenue = total });
        }

        [HttpGet("by-status/{status}")]
        public async Task<IActionResult> GetByStatus([FromRoute] string status)
        {
            var response = await _payService.GetByStatus(status);
            return Ok(response);
        }
    }
}
