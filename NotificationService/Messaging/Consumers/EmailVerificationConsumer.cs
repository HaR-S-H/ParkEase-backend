using System.Text;
using System.Text.Json;
using NotificationService.Messaging.Messages;
using NotificationService.Services;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace NotificationService.Messaging.Consumers
{
    public class EmailVerificationConsumer : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<EmailVerificationConsumer> _logger;
        private IConnection? _connection;
        private IModel? _channel;
        private string? _consumerTag;

        public EmailVerificationConsumer(
            IConfiguration configuration,
            IServiceScopeFactory scopeFactory,
            ILogger<EmailVerificationConsumer> logger)
        {
            _configuration = configuration;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var queueName = _configuration["RabbitMQ:Queues:EmailVerification"]
                ?? throw new InvalidOperationException("Missing RabbitMQ:Queues:EmailVerification.");

            var factory = CreateFactory();
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false);

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.Received += async (_, ea) =>
            {
                if (stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation("Skipping verification message because service is stopping.");
                    return;
                }

                try
                {
                    var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                    var message = JsonSerializer.Deserialize<EmailVerificationRequestedMessage>(json);
                    if (message == null)
                    {
                        _logger.LogWarning("Failed to deserialize verification message.");
                        SafeNack(ea.DeliveryTag, requeue: false);
                        return;
                    }

                    using var scope = _scopeFactory.CreateScope();
                    var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();

                    await emailSender.SendVerificationEmail(message.Email, message.FullName, message.Token);

                    SafeAck(ea.DeliveryTag);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing verification email message.");
                    SafeNack(ea.DeliveryTag, requeue: !IsPermanentFailure(ex));
                }
            };

            _consumerTag = _channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);

            try
            {
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                if (_channel?.IsOpen == true && !string.IsNullOrWhiteSpace(_consumerTag))
                {
                    _channel.BasicCancel(_consumerTag);
                }
            }
        }

        private void SafeAck(ulong deliveryTag)
        {
            if (_channel?.IsOpen == true)
            {
                _channel.BasicAck(deliveryTag, false);
            }
        }

        private void SafeNack(ulong deliveryTag, bool requeue)
        {
            if (_channel?.IsOpen == true)
            {
                _channel.BasicNack(deliveryTag, false, requeue);
            }
        }

        private static bool IsPermanentFailure(Exception ex)
        {
            var message = ex.ToString();
            return message.Contains("Username and Password not accepted", StringComparison.OrdinalIgnoreCase)
                || message.Contains("BadCredentials", StringComparison.OrdinalIgnoreCase)
                || message.Contains("authentication", StringComparison.OrdinalIgnoreCase)
                || message.Contains("configuration", StringComparison.OrdinalIgnoreCase);
        }

        private ConnectionFactory CreateFactory()
        {
            var url = _configuration["RabbitMQ:Url"]
                ?? throw new InvalidOperationException("Missing RabbitMQ:Url.");

            return new ConnectionFactory
            {
                Uri = new Uri(url),
                DispatchConsumersAsync = true
            };
        }

        public override void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
            base.Dispose();
        }
    }
}
