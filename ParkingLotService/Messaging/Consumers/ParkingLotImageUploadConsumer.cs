using System.Text;
using System.Text.Json;
using ParkingLotService.Messaging.Messages;
using ParkingLotService.Repositories;
using ParkingLotService.Services;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ParkingLotService.Messaging.Consumers
{
    public class ParkingLotImageUploadConsumer : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ParkingLotImageUploadConsumer> _logger;
        private IConnection? _connection;
        private IModel? _channel;
        private string? _consumerTag;

        public ParkingLotImageUploadConsumer(
            IConfiguration configuration,
            IServiceScopeFactory scopeFactory,
            ILogger<ParkingLotImageUploadConsumer> logger)
        {
            _configuration = configuration;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var queueName = _configuration["RabbitMQ:Queues:ParkingLotImageUpload"]
                ?? throw new InvalidOperationException("Missing RabbitMQ:Queues:ParkingLotImageUpload.");
            var factory = CreateFactory();

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(queueName, durable: true, exclusive: false, autoDelete: false);

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.Received += async (_, ea) =>
            {
                if (stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation("Skipping parking lot image message because service is stopping.");
                    return;
                }

                try
                {
                    var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                    var message = JsonSerializer.Deserialize<ParkingLotImageUploadRequestedMessage>(json);

                    if (message == null)
                    {
                        _logger.LogWarning("Failed to deserialize parking lot image upload message.");
                        SafeNack(ea.DeliveryTag, requeue: false);
                        return;
                    }

                    _logger.LogInformation("Processing parking lot image upload for lotId {LotId}.", message.LotId);

                    var bytes = Convert.FromBase64String(message.Base64Content);

                    using var scope = _scopeFactory.CreateScope();
                    var storageService = scope.ServiceProvider.GetRequiredService<IStorageService>();
                    var parkingLotRepository = scope.ServiceProvider.GetRequiredService<IParkingLotRepository>();

                    await using var stream = new MemoryStream(bytes);
                    var imageUrl = await storageService.UploadParkingLotImage(stream, message.FileName, message.ContentType, message.LotId);

                    var updated = await parkingLotRepository.UpdateImageUrl(message.LotId, imageUrl);
                    if (!updated)
                    {
                        _logger.LogWarning("Parking lot {LotId} not found while setting image URL.", message.LotId);
                    }

                    SafeAck(ea.DeliveryTag);
                }
                catch (ObjectDisposedException) when (stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation("Parking lot image consumer is stopping; message processing halted.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing parking lot image upload message.");
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

            return message.Contains("AWS Access Key Id you provided does not exist", StringComparison.OrdinalIgnoreCase)
                || message.Contains("InvalidAccessKeyId", StringComparison.OrdinalIgnoreCase)
                || message.Contains("SignatureDoesNotMatch", StringComparison.OrdinalIgnoreCase)
                || message.Contains("The security token included in the request is invalid", StringComparison.OrdinalIgnoreCase);
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