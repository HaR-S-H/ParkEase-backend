using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace NotificationService.Services
{
    public class ResendEmailSender : IEmailSender
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly ILogger<ResendEmailSender> _logger;

        public ResendEmailSender(IConfiguration configuration, HttpClient httpClient, ILogger<ResendEmailSender> logger)
        {
            _configuration = configuration;
            _httpClient = httpClient;
            _logger = logger;
        }

        public Task SendVerificationEmail(string email, string fullName, string token)
        {
            var verificationUrl = BuildVerificationUrl(email, token);
            var subject = "Verify your ParkEase account";
            var body = $"<p>Hi {fullName},</p><p>Please verify your email by clicking the link below:</p><p><a href=\"{verificationUrl}\">Verify Email</a></p><p>If you did not create this account, please ignore this email.</p>";
            return SendGenericEmail(email, subject, body);
        }
        public Task SendForgotPasswordEmail(string email, string fullName, string temporaryPassword)
        {
            var subject = "Password Reset - Temporary Password";
            var body = $"<p>Hi {fullName},</p><p>Your temporary password is: <strong>{temporaryPassword}</strong></p><p>Please log in and change your password immediately for security.</p>";
            return SendGenericEmail(email, subject, body);
        }

        public async Task SendGenericEmail(string email, string subject, string htmlBody)
        {
            var apiKey = _configuration["Resend:ApiKey"];
            var baseUrl = (_configuration["Resend:BaseUrl"] ?? "https://api.resend.com").TrimEnd('/');
            var fromEmail = _configuration["Mail:From:Email"];
            var fromName = _configuration["Mail:From:Name"] ?? "ParkEase";

            if (string.IsNullOrWhiteSpace(apiKey)
                || string.IsNullOrWhiteSpace(fromEmail))
            {
                _logger.LogError("Resend configuration incomplete: HasApiKey={HasApiKey}, FromEmail={FromEmail}",
                    !string.IsNullOrWhiteSpace(apiKey), fromEmail);
                throw new InvalidOperationException("Resend configuration is incomplete.");
            }

            var requestPayload = new
            {
                from = $"{fromName} <{fromEmail}>",
                to = new[] { email },
                subject,
                html = htmlBody
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/emails");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            request.Content = new StringContent(JsonSerializer.Serialize(requestPayload, JsonOptions), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Email sent via Resend to {To}", email);
                return;
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            _logger.LogError("Resend send failed for {To}. Status={StatusCode}, Body={Body}", email, (int)response.StatusCode, responseBody);
            throw new InvalidOperationException($"Failed to send email via Resend. Status code: {(int)response.StatusCode}.");
        }

        private string BuildVerificationUrl(string email, string token)
        {
            var template = _configuration["Mail:VerificationUrlTemplate"];
            var escapedEmail = Uri.EscapeDataString(email);
            var escapedToken = Uri.EscapeDataString(token);

            if (!string.IsNullOrWhiteSpace(template))
            {
                return template
                    .Replace("{email}", escapedEmail, StringComparison.OrdinalIgnoreCase)
                    .Replace("{token}", escapedToken, StringComparison.OrdinalIgnoreCase);
            }

            var baseUrl = _configuration["App:BaseUrl"]?.TrimEnd('/');
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                throw new InvalidOperationException("Set either Mail:VerificationUrlTemplate or App:BaseUrl in configuration.");
            }

            return $"{baseUrl}/api/v1/auth/verify-email?email={escapedEmail}&token={escapedToken}";
        }
    }
}
