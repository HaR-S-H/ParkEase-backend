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
            message.Subject = "Your ParkEase Temporary Password";
            message.Body = new TextPart("html")
            {
                Text = $@"
                <div style='font-family: sans-serif; padding: 20px; border: 1px solid #eee; border-radius: 10px;'>
                    <h2 style='color: #3b82f6;'>Password Reset Request</h2>
                    <p>Hi {fullName},</p>
                    <p>We received a request to reset your password. Here is your temporary password:</p>
                    <div style='background: #f5f5f5; padding: 15px; font-size: 20px; font-weight: bold; text-align: center; border-radius: 5px; margin: 20px 0; color: #000;'>
                        {temporaryPassword}
                    </div>
                    <p>Please use this to log in and change your password immediately in your profile settings.</p>
                    <p style='color: #ef4444; font-size: 13px;'>Note: This password was generated automatically. Do not share it with anyone.</p>
                    <hr style='border: 0; border-top: 1px solid #eee; margin: 20px 0;'>
                    <p style='font-size: 12px; color: #777;'>ParkEase Parking Solutions</p>
                </div>"
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
