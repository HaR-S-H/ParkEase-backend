using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace NotificationService.Services
{
    public class MailKitEmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<MailKitEmailSender> _logger;

        public MailKitEmailSender(IConfiguration configuration, ILogger<MailKitEmailSender> logger)
        {
            _configuration = configuration;
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
            var message = new MimeMessage();
            message.To.Add(MailboxAddress.Parse(email));
            message.Subject = subject;
            message.Body = new TextPart("html") { Text = htmlBody };

            var host = _configuration["Mail:Smtp:Host"];
            var port = int.TryParse(_configuration["Mail:Smtp:Port"], out var parsedPort) ? parsedPort : 465;
            var username = _configuration["Mail:Smtp:Username"];
            var password = _configuration["Mail:Smtp:Password"];
            var fromEmail = _configuration["Mail:From:Email"];
            var fromName = _configuration["Mail:From:Name"] ?? "ParkEase";

            if (!string.IsNullOrWhiteSpace(password))
            {
                password = password.Replace(" ", string.Empty, StringComparison.Ordinal);
            }

            if (string.IsNullOrWhiteSpace(host)
                || string.IsNullOrWhiteSpace(username)
                || string.IsNullOrWhiteSpace(password)
                || string.IsNullOrWhiteSpace(fromEmail))
            {
                _logger.LogError("Mail SMTP configuration incomplete: Host={Host}, Username={Username}, HasPassword={HasPassword}, FromEmail={FromEmail}",
                    host, username, !string.IsNullOrWhiteSpace(password), fromEmail);
                throw new InvalidOperationException("Mail SMTP configuration is incomplete.");
            }

            message.From.Add(new MailboxAddress(fromName, fromEmail));

            _logger.LogInformation("Sending email to {To} via {Host}:{Port} from {From}", email, host, port, fromEmail);

            using var client = new SmtpClient();
            try
            {
                // Increase timeout to 30 seconds to account for Render's network latency
                client.Timeout = 30000;
                
                _logger.LogInformation("[SMTP Step 1] About to connect to {Host}:{Port}", host, port);
                // Use SSL on connect (port 465) instead of STARTTLS (port 587) for better Render compatibility
                var secureOption = port == 465 ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls;
                await client.ConnectAsync(host, port, secureOption);
                _logger.LogInformation("[SMTP Step 2] Connected to SMTP server {Host}:{Port}", host, port);

                _logger.LogInformation("[SMTP Step 3] About to authenticate as {Username}", username);
                await client.AuthenticateAsync(username, password);
                _logger.LogInformation("[SMTP Step 4] Authenticated to SMTP as {Username}", username);

                _logger.LogInformation("[SMTP Step 5] About to send message");
                await client.SendAsync(message);
                _logger.LogInformation("[SMTP Step 6] Email sent successfully to {To}", email);

                _logger.LogInformation("[SMTP Step 7] About to disconnect");
                await client.DisconnectAsync(true);
                _logger.LogInformation("[SMTP Step 8] Disconnected from SMTP");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[SMTP ERROR] Failed to send email to {To} via {Host}:{Port}. Exception type: {ExType}", email, host, port, ex.GetType().Name);
                throw;
            }
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
