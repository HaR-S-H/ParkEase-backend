using System.Net.Http.Json;
using Microsoft.Extensions.Logging;

namespace AuthService.Services
{
    public class NotificationDispatcher : INotificationDispatcher
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<NotificationDispatcher> _logger;

        public NotificationDispatcher(HttpClient httpClient, ILogger<NotificationDispatcher> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public Task SendVerificationEmail(string email, string fullName, string token, int? recipientId = null, CancellationToken cancellationToken = default)
        {
            return PostSafeAsync("api/v1/notifications/send-verification-email", new
            {
                Email = email,
                FullName = fullName,
                Token = token,
                RecipientId = recipientId
            }, cancellationToken);
        }

        public Task SendForgotPasswordEmail(string email, string fullName, string temporaryPassword, int? recipientId = null, CancellationToken cancellationToken = default)
        {
            return PostSafeAsync("api/v1/notifications/send-forgot-password-email", new
            {
                Email = email,
                FullName = fullName,
                TemporaryPassword = temporaryPassword,
                RecipientId = recipientId
            }, cancellationToken);
        }

        private async Task PostSafeAsync(string path, object body, CancellationToken cancellationToken)
        {
            try
            {
                using var response = await _httpClient.PostAsJsonAsync(path, body, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    var text = await response.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogWarning("Notification POST {Path} returned {Status}: {Body}", path, response.StatusCode, text);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to POST notification to {Path}", path);
            }
        }
    }
}