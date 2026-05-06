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

            try
            {
                var factory = CreateFactory();
                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();
                _channel.QueueDeclare(queueName, durable: true, exclusive: false, autoDelete: false);

                _logger.LogInformation("Email verification consumer started on queue: {QueueName}", queueName);

                var consumer = new AsyncEventingBasicConsumer(_channel);
                consumer.Received += async (_, ea) =>
                {
                    if (stoppingToken.IsCancellationRequested)
                    {
                        _logger.LogInformation("Skipping email verification message because service is stopping.");
                        return;
                    }

                    try
                    {
                        var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                        var message = JsonSerializer.Deserialize<EmailVerificationRequestedMessage>(json);

                        if (message == null)
                        {
                            _logger.LogWarning("Failed to deserialize email verification message.");
                            SafeNack(ea.DeliveryTag, requeue: false);
                            return;
                        }

                        _logger.LogInformation("Processing email verification for {Email}", message.Email);

                        using var scope = _scopeFactory.CreateScope();
                        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

                        await notificationService.SendVerificationEmail(message.Email, message.FullName, message.Token);

                        _logger.LogInformation("Email verification sent for {Email}", message.Email);
                        SafeAck(ea.DeliveryTag);
                    }
                    catch (ObjectDisposedException) when (stoppingToken.IsCancellationRequested)
                    {
                        _logger.LogInformation("Email verification consumer is stopping; message processing halted.");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing email verification message.");
                        SafeNack(ea.DeliveryTag, requeue: true);
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Email verification consumer encountered an error and will exit");
                throw;
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

        private ConnectionFactory CreateFactory()
        {
            var url = _configuration["RabbitMQ:Url"]
                ?? throw new InvalidOperationException("Missing RabbitMQ:Url.");

            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                throw new InvalidOperationException($"Invalid RabbitMQ:Url value: {url}");
            }

            return new ConnectionFactory
            {
                Uri = uri,
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
