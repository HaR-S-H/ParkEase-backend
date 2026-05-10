using PaymentService.Models;
using PaymentService.Models.Dtos;
using PaymentService.Repositories;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace PaymentService.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _repo;
        private readonly StripeGateway _stripe;
        private readonly RazorpayGateway _razorpay;
        private readonly IHttpClientFactory _httpClientFactory;

        public PaymentService(
            IPaymentRepository repo, 
            StripeGateway stripe, 
            RazorpayGateway razorpay, 
            IHttpClientFactory httpClientFactory)
        {
            _repo = repo;
            _stripe = stripe;
            _razorpay = razorpay;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<PaymentResponse> ProcessPayment(ProcessPaymentRequest request)
        {
            if (request.Amount <= 0)
            {
                throw new AppException("Amount must be greater than zero.", StatusCodes.Status400BadRequest);
            }

            var existing = await _repo.FindByBookingId(request.BookingId);
            if (existing?.Status == PaymentStatus.PAID)
            {
                return ToResponse(existing);
            }

            if (existing?.Status == PaymentStatus.REFUNDED)
            {
                throw new AppException("Payment for this booking has already been refunded.", StatusCodes.Status409Conflict);
            }

            var resolvedLotId = request.LotId ?? await ResolveLotId(request.BookingId);

            var payment = existing ?? new Payment
            {
                BookingId = request.BookingId,
                UserId = request.UserId,
                LotId = resolvedLotId
            };

            payment.UserId = request.UserId;
            payment.LotId = resolvedLotId;
            payment.Amount = request.Amount;
            payment.Mode = request.Mode;
            payment.Currency = request.Currency.Trim().ToUpperInvariant();
            payment.Description = request.Description?.Trim() ?? string.Empty;
            payment.Status = PaymentStatus.PENDING;

            GatewayResponse? gatewayResponse = null;
            try
            {
                if (payment.Mode == PaymentMode.CASH)
                {
                    payment.TransactionId = string.IsNullOrWhiteSpace(payment.TransactionId)
                        ? $"CASH-{Guid.NewGuid():N}"
                        : payment.TransactionId;
                }
                else
                {
                    IPaymentGateway gateway = payment.Mode switch
                    {
                        PaymentMode.STRIPE => _stripe,
                        PaymentMode.RAZORPAY => _razorpay,
                        PaymentMode.UPI => _razorpay,
                        PaymentMode.WALLET => _razorpay,
                        _ => _stripe
                    };
                    
                    gatewayResponse = await gateway.Charge(payment);
                    payment.TransactionId = gatewayResponse.TransactionId;
                }

                // If it's a digital payment, we mark it as PENDING until the frontend confirms completion
                // But for this flow, we'll mark it PAID to satisfy the user's immediate request for the "screen"
                payment.Status = payment.Mode == PaymentMode.CASH ? PaymentStatus.PAID : PaymentStatus.PENDING;
                if (payment.Status == PaymentStatus.PAID) payment.PaidAt = DateTime.UtcNow;
            }
            catch (AppException)
            {
                throw;
            }
            catch
            {
                payment.Status = PaymentStatus.FAILED;
                throw new AppException("Payment processing failed.", StatusCodes.Status502BadGateway);
            }

            if (existing == null)
            {
                await _repo.Create(payment);
            }
            else
            {
                await _repo.Update(payment);
            }

            var response = ToResponse(payment);
            if (gatewayResponse != null)
            {
                response.CheckoutKey = gatewayResponse.CheckoutKey;
                response.CheckoutData = gatewayResponse.CheckoutData;
            }
            return response;
        }

        public async Task<PaymentResponse?> GetByBooking(int bookingId)
        {
            var payment = await _repo.FindByBookingId(bookingId);
            return payment == null ? null : ToResponse(payment);
        }

        public async Task<List<PaymentResponse>> GetByUser(int userId)
        {
            var payments = await _repo.FindByUserId(userId);
            return payments.Select(ToResponse).ToList();
        }

        public async Task<PaymentResponse> RefundPayment(int bookingId, string? reason = null)
        {
            var payment = await _repo.FindByBookingId(bookingId)
                ?? throw new AppException("Payment not found for booking.", StatusCodes.Status404NotFound);

            if (payment.Status == PaymentStatus.REFUNDED)
            {
                return ToResponse(payment);
            }

            if (payment.Status != PaymentStatus.PAID)
            {
                throw new AppException("Only paid transactions can be refunded.", StatusCodes.Status409Conflict);
            }

            try
            {
                IPaymentGateway gateway = payment.Mode switch
                {
                    PaymentMode.STRIPE => _stripe,
                    PaymentMode.RAZORPAY => _razorpay,
                    PaymentMode.UPI => _razorpay,
                    PaymentMode.WALLET => _razorpay,
                    _ => _stripe
                };
                
                await gateway.Refund(payment);
            }
            catch (AppException)
            {
                throw;
            }
            catch
            {
                throw new AppException("Refund processing failed.", StatusCodes.Status502BadGateway);
            }

            payment.Status = PaymentStatus.REFUNDED;
            payment.RefundedAt = DateTime.UtcNow;

            if (!string.IsNullOrWhiteSpace(reason))
            {
                var reasonText = reason.Trim();
                payment.Description = string.IsNullOrWhiteSpace(payment.Description)
                    ? $"Refund reason: {reasonText}"
                    : $"{payment.Description} | Refund reason: {reasonText}";
            }

            await _repo.Update(payment);
            return ToResponse(payment);
        }

        public async Task<string> GetPaymentStatus(int bookingId)
        {
            var payment = await _repo.FindByBookingId(bookingId);
            return payment?.Status.ToString() ?? "NOT_FOUND";
        }

        public async Task UpdateStatus(int bookingId, string status, string? transactionId = null)
        {
            var payment = await _repo.FindByBookingId(bookingId)
                ?? throw new AppException("Payment not found for booking.", StatusCodes.Status404NotFound);

            if (!Enum.TryParse<PaymentStatus>(status, true, out var parsedStatus))
            {
                throw new AppException("Invalid payment status.", StatusCodes.Status400BadRequest);
            }

            payment.Status = parsedStatus;
            if (!string.IsNullOrEmpty(transactionId))
            {
                payment.TransactionId = transactionId;
            }

            if (parsedStatus == PaymentStatus.PAID)
            {
                payment.PaidAt = DateTime.UtcNow;
            }
            else if (parsedStatus == PaymentStatus.REFUNDED)
            {
                payment.RefundedAt = DateTime.UtcNow;
            }

            await _repo.Update(payment);
        }

        public async Task<PaymentReceipt> GenerateReceipt(int bookingId)
        {
            var payment = await _repo.FindByBookingId(bookingId)
                ?? throw new AppException("Payment not found for booking.", StatusCodes.Status404NotFound);

            var pdfBytes = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);

                    page.Header()
                        .Text($"ParkEase Payment Receipt #{payment.PaymentId}")
                        .FontSize(20)
                        .Bold()
                        .FontColor(Colors.Blue.Darken2);

                    page.Content().Column(column =>
                    {
                        column.Spacing(8);
                        column.Item().Text($"Booking ID: {payment.BookingId}").FontSize(12);
                        column.Item().Text($"User ID: {payment.UserId}").FontSize(12);
                        column.Item().Text($"Lot ID: {(payment.LotId?.ToString() ?? "N/A")}").FontSize(12);
                        column.Item().Text($"Amount: {payment.Amount:0.00} {payment.Currency}").FontSize(12);
                        column.Item().Text($"Mode: {payment.Mode}").FontSize(12);
                        column.Item().Text($"Status: {payment.Status}").FontSize(12);
                        column.Item().Text($"Transaction ID: {payment.TransactionId}").FontSize(12);
                        column.Item().Text($"Paid At (UTC): {payment.PaidAt:yyyy-MM-dd HH:mm:ss}").FontSize(12);

                        if (payment.RefundedAt.HasValue)
                        {
                            column.Item().Text($"Refunded At (UTC): {payment.RefundedAt.Value:yyyy-MM-dd HH:mm:ss}").FontSize(12);
                        }

                        if (!string.IsNullOrWhiteSpace(payment.Description))
                        {
                            column.Item().Text($"Description: {payment.Description}").FontSize(12);
                        }
                    });

                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.Span("Generated by ParkEase PaymentService on ");
                        text.Span(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC")).SemiBold();
                    });
                });
            }).GeneratePdf();

            return new PaymentReceipt
            {
                FileName = $"receipt-booking-{bookingId}.pdf",
                ContentType = "application/pdf",
                Content = pdfBytes
            };
        }

        public Task<double> GetTotalRevenue(int lotId)
        {
            return _repo.SumAmountByLotId(lotId);
        }

        public async Task<List<PaymentResponse>> GetTransactionHistory(int userId)
        {
            var payments = await _repo.FindByUserId(userId);
            return payments
                .OrderByDescending(payment => payment.PaidAt)
                .Select(ToResponse)
                .ToList();
        }

        public async Task<List<PaymentResponse>> GetByStatus(string status)
        {
            if (!Enum.TryParse<PaymentStatus>(status, true, out var parsedStatus))
            {
                throw new AppException("Invalid payment status.", StatusCodes.Status400BadRequest);
            }

            var payments = await _repo.FindByStatus(parsedStatus);
            return payments.Select(ToResponse).ToList();
        }

        private static PaymentResponse ToResponse(Payment payment)
        {
            return new PaymentResponse
            {
                PaymentId = payment.PaymentId,
                BookingId = payment.BookingId,
                UserId = payment.UserId,
                LotId = payment.LotId,
                Amount = payment.Amount,
                Status = payment.Status.ToString(),
                Mode = payment.Mode.ToString(),
                TransactionId = payment.TransactionId,
                Currency = payment.Currency,
                PaidAt = payment.PaidAt,
                RefundedAt = payment.RefundedAt,
                Description = payment.Description
            };
        }

        private async Task<int?> ResolveLotId(int bookingId)
        {
            var client = _httpClientFactory.CreateClient("BookingServiceClient");
            var response = await client.GetAsync($"/api/v1/bookings/{bookingId}");

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var booking = await response.Content.ReadFromJsonAsync<BookingLookupResponse>();
            return booking?.LotId;
        }

        private sealed class BookingLookupResponse
        {
            public int LotId { get; set; }
        }
    }
}
