using BookingService.Models;
using System.Net.Http.Json;

namespace BookingService.Services
{
    public class SpotService : ISpotService
    {
        private readonly HttpClient _httpClient;

        public SpotService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("SpotServiceClient");
        }

        public async Task<SpotDetails> GetSpotById(int spotId)
        {
            var response = await _httpClient.GetAsync($"/api/v1/spots/{spotId}");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                throw new AppException("Parking spot not found.", StatusCodes.Status404NotFound);
            }

            if (!response.IsSuccessStatusCode)
            {
                throw new AppException("Failed to fetch parking spot details.", StatusCodes.Status502BadGateway);
            }

            var spot = await response.Content.ReadFromJsonAsync<SpotDetails>();
            if (spot == null)
            {
                throw new AppException("Invalid parking spot response.", StatusCodes.Status502BadGateway);
            }

            return spot;
        }

        public async Task ReserveSpot(int spotId)
        {
            await UpdateSpotStatus(spotId, "RESERVED");
        }

        public async Task OccupySpot(int spotId)
        {
            var response = await _httpClient.PutAsync($"/api/v1/spots/{spotId}/occupy", null);
            EnsureSuccess(response, "Failed to mark parking spot as occupied.");
        }

        public async Task ReleaseSpot(int spotId)
        {
            var response = await _httpClient.PutAsync($"/api/v1/spots/{spotId}/release", null);
            EnsureSuccess(response, "Failed to release parking spot.");
        }

        private async Task UpdateSpotStatus(int spotId, string status)
        {
            var payload = new { Status = status };
            var response = await _httpClient.PutAsJsonAsync($"/api/v1/spots/{spotId}", payload);
            EnsureSuccess(response, "Failed to update parking spot status.");
        }

        private static void EnsureSuccess(HttpResponseMessage response, string message)
        {
            if (response.IsSuccessStatusCode)
            {
                return;
            }

            var statusCode = (int)response.StatusCode;
            throw new AppException($"{message} (Status: {statusCode})", statusCode);
        }
    }
}
