using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace NotificationService.Services
{
    public class MailKitEmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;

        public MailKitEmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
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
            var port = int.TryParse(_configuration["Mail:Smtp:Port"], out var parsedPort) ? parsedPort : 587;
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
