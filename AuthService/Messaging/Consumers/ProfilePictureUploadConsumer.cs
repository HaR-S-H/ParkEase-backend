using System.Text;
using System.Text.Json;
using AuthService.Messaging.Messages;
using AuthService.Repositories;
using AuthService.Services;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace AuthService.Messaging.Consumers
{
    public class ProfilePictureUploadConsumer : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ProfilePictureUploadConsumer> _logger;
        private IConnection? _connection;
        private IModel? _channel;
        private string? _consumerTag;

        public ProfilePictureUploadConsumer(IConfiguration configuration, IServiceScopeFactory scopeFactory, ILogger<ProfilePictureUploadConsumer> logger)
        {
            _configuration = configuration;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var queueName = _configuration["RabbitMQ:Queues:ProfilePictureUpload"]
                ?? throw new InvalidOperationException("Missing RabbitMQ:Queues:ProfilePictureUpload.");
            var factory = CreateFactory();

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(queueName, durable: true, exclusive: false, autoDelete: false);

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.Received += async (_, ea) =>
            {
                if (stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation("Skipping profile picture message because service is stopping.");
                    return;
                }

                try
                {
                    var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                    var message = JsonSerializer.Deserialize<ProfilePictureUploadRequestedMessage>(json);
                    if (message == null)
                    {
                        _logger.LogWarning("Failed to deserialize profile picture upload message");
                        SafeNack(ea.DeliveryTag, requeue: false);
                        return;
                    }

                    _logger.LogInformation($"Processing profile picture upload for userId {message.UserId}");
                    var bytes = Convert.FromBase64String(message.Base64Content);

                    using var scope = _scopeFactory.CreateScope();
                    var storageService = scope.ServiceProvider.GetRequiredService<IStorageService>();
                    var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

                    await using var stream = new MemoryStream(bytes);
                    var pictureUrl = await storageService.UploadProfilePicture(stream, message.FileName, message.ContentType, message.UserId);
                    _logger.LogInformation($"Picture uploaded to S3: {pictureUrl}");

                    var user = await userRepository.FindByUserId(message.UserId);
                    if (user != null)
                    {
                        user.ProfilePicUrl = pictureUrl;
                        await userRepository.Update(user);
                        _logger.LogInformation($"User profile updated with picture URL");
                    }
                    else
                    {
                        _logger.LogWarning($"User {message.UserId} not found for picture update");
                    }

                    SafeAck(ea.DeliveryTag);
                }
                catch (ObjectDisposedException) when (stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation("Profile picture consumer is stopping; message processing halted.");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error processing profile picture upload: {ex.Message}\n{ex.StackTrace}");
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

            // Permanent AWS credential/config failures should not be requeued forever.
            return message.Contains("AWS Access Key Id you provided does not exist", StringComparison.OrdinalIgnoreCase)
                || message.Contains("InvalidAccessKeyId", StringComparison.OrdinalIgnoreCase)
                || message.Contains("SignatureDoesNotMatch", StringComparison.OrdinalIgnoreCase)
                || message.Contains("The security token included in the request is invalid", StringComparison.OrdinalIgnoreCase);
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
