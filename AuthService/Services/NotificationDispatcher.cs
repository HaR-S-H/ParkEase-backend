using System.Net.Http.Json;

namespace AuthService.Services
{
    public class NotificationDispatcher : INotificationDispatcher
    {
        private readonly HttpClient _httpClient;

        public NotificationDispatcher(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public Task SendVerificationEmail(string email, string fullName, string token, CancellationToken cancellationToken = default)
        {
            return PostAsync("api/v1/notifications/send-verification-email", new
            {
                Email = email,
                FullName = fullName,
                Token = token
            }, cancellationToken);
        }

        public Task SendForgotPasswordEmail(string email, string fullName, string temporaryPassword, CancellationToken cancellationToken = default)
        {
            return PostAsync("api/v1/notifications/send-forgot-password-email", new
            {
                Email = email,
                FullName = fullName,
                TemporaryPassword = temporaryPassword
            }, cancellationToken);
        }

        private async Task PostAsync(string path, object body, CancellationToken cancellationToken)
        {
            using var response = await _httpClient.PostAsJsonAsync(path, body, cancellationToken);
            response.EnsureSuccessStatusCode();
        }
    }
}