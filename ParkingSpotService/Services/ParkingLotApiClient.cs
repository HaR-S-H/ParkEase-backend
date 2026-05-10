using System.Net.Http.Json;

namespace ParkingSpotService.Services
{
    public class ParkingLotApiClient : IParkingLotApiClient
    {
        private readonly HttpClient _httpClient;

        public ParkingLotApiClient(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("ParkingLotServiceClient");
        }

        public async Task IncrementTotalSpots(int lotId, int quantity = 1)
        {
            var response = await _httpClient.PutAsJsonAsync($"/api/v1/lots/{lotId}/total/increment?quantity={quantity}", new { });
            await EnsureSuccess(response, "Failed to increment lot total spots.");
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
                if (!string.IsNullOrWhiteSpace(errorContent))
                {
                    message = errorContent;
                }
            }

            var statusCode = (int)response.StatusCode;
            throw new Models.AppException(message, statusCode);
        }
    }
}