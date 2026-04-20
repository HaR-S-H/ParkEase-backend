using System.Text;
using System.Text.Json;
using AuthService.Messaging.Messages;
using AuthService.Services;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace AuthService.Messaging.Consumers
{
    public class ForgotPasswordConsumer : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ForgotPasswordConsumer> _logger;
        private IConnection? _connection;
        private IModel? _channel;
        private string? _consumerTag;

        public ForgotPasswordConsumer(
            IConfiguration configuration,
            IServiceScopeFactory scopeFactory,
            ILogger<ForgotPasswordConsumer> logger)
        {
            _configuration = configuration;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var queueName = _configuration["RabbitMQ:Queues:ForgotPassword"]
                ?? throw new InvalidOperationException("Missing RabbitMQ:Queues:ForgotPassword.");
            var factory = CreateFactory();

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(queueName, durable: true, exclusive: false, autoDelete: false);

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.Received += async (_, ea) =>
            {
                if (stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation("Skipping forgot-password message because service is stopping.");
                    return;
                }

                try
                {
                    var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                    var message = JsonSerializer.Deserialize<ForgotPasswordRequestedMessage>(json);
                    if (message == null)
                    {
                        _logger.LogWarning("Failed to deserialize forgot-password message");
                        SafeNack(ea.DeliveryTag, requeue: false);
                        return;
                    }

                    _logger.LogInformation($"Processing forgot-password email for {message.Email}");
                    using var scope = _scopeFactory.CreateScope();
                    var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                    await emailService.SendTemporaryPasswordEmail(message.Email, message.FullName, message.TemporaryPassword);
                    _logger.LogInformation($"Forgot-password email sent to {message.Email}");
                    SafeAck(ea.DeliveryTag);
                }
                catch (ObjectDisposedException) when (stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation("Forgot-password consumer is stopping; message processing halted.");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error processing forgot-password email: {ex.Message}\n{ex.StackTrace}");
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
                // Expected during graceful shutdown.
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
                || message.Contains("authentication", StringComparison.OrdinalIgnoreCase);
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
