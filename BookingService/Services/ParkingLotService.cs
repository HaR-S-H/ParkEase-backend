using BookingService.Models;
using System.Net.Http.Json;

namespace BookingService.Services
{
    public class ParkingLotService : IParkingLotService
    {
        private readonly HttpClient _httpClient;

        public ParkingLotService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("ParkingLotServiceClient");
        }

        public async Task<LotDetails> GetLotById(int lotId)
        {
            var response = await _httpClient.GetAsync($"/api/v1/lots/{lotId}");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                throw new AppException("Parking lot not found.", StatusCodes.Status404NotFound);
            }

            if (!response.IsSuccessStatusCode)
            {
                throw new AppException("Failed to fetch parking lot details.", StatusCodes.Status502BadGateway);
            }

            var lot = await response.Content.ReadFromJsonAsync<LotDetails>();
            if (lot == null)
            {
                throw new AppException("Invalid parking lot response.", StatusCodes.Status502BadGateway);
            }

            return lot;
        }

        public async Task DecrementAvailable(int lotId, int quantity = 1)
        {
            var response = await _httpClient.PutAsJsonAsync($"/api/v1/lots/{lotId}/available/decrement?quantity={quantity}", new { });
            await EnsureSuccess(response, "Failed to decrement lot availability.");
        }

        public async Task IncrementAvailable(int lotId, int quantity = 1)
        {
            var response = await _httpClient.PutAsJsonAsync($"/api/v1/lots/{lotId}/available/increment?quantity={quantity}", new { });
            await EnsureSuccess(response, "Failed to increment lot availability.");
        }

        private static async Task EnsureSuccess(HttpResponseMessage response, string defaultMessage)
        {
            if (response.IsSuccessStatusCode)
            {
                return;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            var message = defaultMessage;

            try
            {
                var errorObj = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.Nodes.JsonNode>(errorContent);
                message = errorObj?["message"]?.ToString() ?? errorContent;
            }
            catch
            {
                if (!string.IsNullOrWhiteSpace(errorContent)) message = errorContent;
            }

            var statusCode = (int)response.StatusCode;
            throw new AppException(message, statusCode);
        }
    }
}
