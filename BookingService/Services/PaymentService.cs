using BookingService.Models;
using System.Net.Http.Json;

namespace BookingService.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly HttpClient _httpClient;

        public PaymentService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("PaymentServiceClient");
        }

        public async Task Charge(int bookingId, int userId, double amount)
        {
            if (amount <= 0)
            {
                return;
            }

            var payload = new
            {
                BookingId = bookingId,
                UserId = userId,
                Amount = amount
            };

            try
            {
                var response = await _httpClient.PostAsJsonAsync("/api/v1/payments/charge", payload);
                EnsureSuccess(response, "Payment charge failed.");
            }
            catch (TaskCanceledException)
            {
                throw new AppException("Payment service timed out.", StatusCodes.Status504GatewayTimeout);
            }
            catch (HttpRequestException)
            {
                throw new AppException("Payment service is unavailable.", StatusCodes.Status503ServiceUnavailable);
            }
        }

        public async Task Refund(int bookingId, int userId, double amount)
        {
            if (amount <= 0)
            {
                return;
            }

            var payload = new
            {
                BookingId = bookingId,
                UserId = userId,
                Amount = amount
            };

            try
            {
                var response = await _httpClient.PostAsJsonAsync("/api/v1/payments/refund", payload);
                EnsureSuccess(response, "Payment refund failed.");
            }
            catch (TaskCanceledException)
            {
                throw new AppException("Payment service timed out.", StatusCodes.Status504GatewayTimeout);
            }
            catch (HttpRequestException)
            {
                throw new AppException("Payment service is unavailable.", StatusCodes.Status503ServiceUnavailable);
            }
        }

        private static void EnsureSuccess(HttpResponseMessage response, string message)
        {
            if (!response.IsSuccessStatusCode)
            {
                throw new AppException(message, StatusCodes.Status502BadGateway);
            }
        }
    }
}
