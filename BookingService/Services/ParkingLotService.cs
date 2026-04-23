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
            var response = await _httpClient.PutAsync($"/api/v1/lots/{lotId}/available/decrement?quantity={quantity}", null);
            EnsureSuccess(response, "Failed to decrement lot availability.");
        }

        public async Task IncrementAvailable(int lotId, int quantity = 1)
        {
            var response = await _httpClient.PutAsync($"/api/v1/lots/{lotId}/available/increment?quantity={quantity}", null);
            EnsureSuccess(response, "Failed to increment lot availability.");
        }

        private static void EnsureSuccess(HttpResponseMessage response, string message)
        {
            if (response.IsSuccessStatusCode)
            {
                return;
            }

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                throw new AppException("Parking lot not found.", StatusCodes.Status404NotFound);
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                throw new AppException("Parking lot availability conflict.", StatusCodes.Status409Conflict);
            }

            throw new AppException(message, StatusCodes.Status502BadGateway);
        }
    }
}
