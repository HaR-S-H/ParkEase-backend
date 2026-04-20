using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace AuthService.Services
{
    public class MailKitEmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public MailKitEmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendVerificationEmail(string email, string fullName, string token)
        {
            var verificationUrl = BuildVerificationUrl(email, token);
            var message = new MimeMessage();
            message.To.Add(MailboxAddress.Parse(email));
            message.Subject = "Verify your ParkEase account";
            message.Body = new TextPart("html")
            {
                Text = $"<p>Hi {fullName},</p><p>Please verify your email by clicking the link below:</p><p><a href=\"{verificationUrl}\">Verify Email</a></p><p>If you did not create this account, please ignore this email.</p>"
            };

            await SendEmail(message);
        }

        public async Task SendTemporaryPasswordEmail(string email, string fullName, string temporaryPassword)
        {
            var message = new MimeMessage();
            message.To.Add(MailboxAddress.Parse(email));
            message.Subject = "Your ParkEase temporary password";
            message.Body = new TextPart("html")
            {
                Text = $"<p>Hi {fullName},</p><p>Your temporary password is:</p><p><strong>{temporaryPassword}</strong></p><p>Please login and change your password immediately from profile settings.</p><p>If you did not request this, contact support immediately.</p>"
            };

            await SendEmail(message);
        }

        private async Task SendEmail(MimeMessage message)
        {
            var host = _configuration["Mail:Smtp:Host"];
            var port = int.TryParse(_configuration["Mail:Smtp:Port"], out var parsedPort) ? parsedPort : 587;
            var username = _configuration["Mail:Smtp:Username"];
            var password = _configuration["Mail:Smtp:Password"];
            var fromEmail = _configuration["Mail:From:Email"];
            var fromName = _configuration["Mail:From:Name"] ?? "ParkEase";

            if (!string.IsNullOrWhiteSpace(password))
            {
                password = password.Replace(" ", string.Empty, StringComparison.Ordinal);
            }

            if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(fromEmail))
            {
                throw new InvalidOperationException("Mail SMTP configuration is incomplete.");
            }

            message.From.Add(new MailboxAddress(fromName, fromEmail));

            using var client = new SmtpClient();
            await client.ConnectAsync(host, port, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(username, password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
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
