using AuthService.Messaging;
using AuthService.Messaging.Messages;
using Microsoft.Extensions.Logging;

namespace AuthService.Services
{
    public class NotificationDispatcher : INotificationDispatcher
    {
        private readonly IRabbitMqPublisher _publisher;
        private readonly IConfiguration _configuration;
        private readonly ILogger<NotificationDispatcher> _logger;

        public NotificationDispatcher(
            IRabbitMqPublisher publisher,
            IConfiguration configuration,
            ILogger<NotificationDispatcher> logger)
        {
            _publisher = publisher;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendVerificationEmail(string email, string fullName, string token, int? recipientId = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var queueName = _configuration["RabbitMQ:Queues:EmailSend"]
                    ?? throw new InvalidOperationException("Missing RabbitMQ:Queues:EmailSend configuration.");

                var message = new VerificationEmailMessage
                {
                    Email = email,
                    FullName = fullName,
                    Token = token,
                    UserId = recipientId
                };

                await _publisher.Publish(queueName, message);
                _logger.LogInformation("Verification email queued for {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to queue verification email to {Email}", email);
                throw;
            }
        }

        public async Task SendForgotPasswordEmail(string email, string fullName, string temporaryPassword, int? recipientId = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var queueName = _configuration["RabbitMQ:Queues:EmailSend"]
                    ?? throw new InvalidOperationException("Missing RabbitMQ:Queues:EmailSend configuration.");

                var message = new ForgotPasswordEmailMessage
                {
                    Email = email,
                    FullName = fullName,
                    TemporaryPassword = temporaryPassword,
                    UserId = recipientId
                };

                await _publisher.Publish(queueName, message);
                _logger.LogInformation("Forgot password email queued for {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to queue forgot password email to {Email}", email);
                throw;
            }
        }
    }
}