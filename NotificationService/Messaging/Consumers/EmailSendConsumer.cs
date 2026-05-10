using System.Text;
using System.Text.Json;
using NotificationService.Services;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace NotificationService.Messaging.Consumers
{
    public class EmailSendConsumer : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<EmailSendConsumer> _logger;
        private IConnection? _connection;
        private IModel? _channel;
        private string? _consumerTag;

        public EmailSendConsumer(
            IConfiguration configuration,
            IServiceScopeFactory scopeFactory,
            ILogger<EmailSendConsumer> logger)
        {
            _configuration = configuration;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var queueName = _configuration["RabbitMQ:Queues:EmailSend"];
            var rabbitUrl = _configuration["RabbitMQ:Url"];

            if (string.IsNullOrWhiteSpace(queueName) || string.IsNullOrWhiteSpace(rabbitUrl))
            {
                _logger.LogInformation("RabbitMQ configuration missing; email send consumer will not start.");
                return;
            }

            try
            {
                var factory = CreateFactory();
                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();
                _channel.QueueDeclare(queueName, durable: true, exclusive: false, autoDelete: false);

                _logger.LogInformation("Email send consumer started on queue: {QueueName}", queueName);

                var consumer = new AsyncEventingBasicConsumer(_channel);
                consumer.Received += async (_, ea) =>
                {
                    if (stoppingToken.IsCancellationRequested)
                    {
                        _logger.LogInformation("Skipping email message because service is stopping.");
                        return;
                    }

                    try
                    {
                        var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                        _logger.LogInformation("Processing email message: {Message}", json);

                        using var scope = _scopeFactory.CreateScope();
                        var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();

                        // Try to deserialize as VerificationEmailMessage
                        if (TryDeserializeAndSend<VerificationEmailMessage>(json, emailSender, out var result))
                        {
                            _logger.LogInformation("Verification email sent successfully");
                            SafeAck(ea.DeliveryTag);
                            return;
                        }

                        // Try to deserialize as ForgotPasswordEmailMessage
                        if (TryDeserializeAndSend<ForgotPasswordEmailMessage>(json, emailSender, out result))
                        {
                            _logger.LogInformation("Forgot password email sent successfully");
                            SafeAck(ea.DeliveryTag);
                            return;
                        }

                        _logger.LogWarning("Failed to deserialize email message or unknown message type.");
                        SafeNack(ea.DeliveryTag, requeue: false);
                    }
                    catch (ObjectDisposedException) when (stoppingToken.IsCancellationRequested)
                    {
                        _logger.LogInformation("Email send consumer is stopping; message processing halted.");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing email send message.");
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
                _logger.LogError(ex, "Email send consumer encountered an error");
            }
        }

        private bool TryDeserializeAndSend<T>(string json, IEmailSender emailSender, out bool success)
        {
            success = false;
            try
            {
                var message = JsonSerializer.Deserialize<T>(json);
                if (message == null)
                    return false;

                // Handle VerificationEmailMessage
                if (message is VerificationEmailMessage verifyMsg)
                {
                    emailSender.SendVerificationEmail(verifyMsg.Email, verifyMsg.FullName, verifyMsg.Token).GetAwaiter().GetResult();
                    success = true;
                    return true;
                }

                // Handle ForgotPasswordEmailMessage
                if (message is ForgotPasswordEmailMessage forgotMsg)
                {
                    emailSender.SendForgotPasswordEmail(forgotMsg.Email, forgotMsg.FullName, forgotMsg.TemporaryPassword).GetAwaiter().GetResult();
                    success = true;
                    return true;
                }

                return false;
            }
            catch (JsonException)
            {
                return false;
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

    // Message classes for deserialization
    public class VerificationEmailMessage
    {
        public required string Email { get; set; }
        public required string FullName { get; set; }
        public required string Token { get; set; }
        public int? UserId { get; set; }
    }

    public class ForgotPasswordEmailMessage
    {
        public required string Email { get; set; }
        public required string FullName { get; set; }
        public required string TemporaryPassword { get; set; }
        public int? UserId { get; set; }
    }
}
