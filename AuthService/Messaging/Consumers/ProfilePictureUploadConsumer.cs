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

        public ProfilePictureUploadConsumer(
            IConfiguration configuration,
            IServiceScopeFactory scopeFactory)
        {
            _configuration = configuration;
            _scopeFactory = scopeFactory;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var factory = new ConnectionFactory
            {
                Uri = new Uri(_configuration["RabbitMQ:Url"]!),
                DispatchConsumersAsync = true
            };

            var connection = factory.CreateConnection();

            var channel = connection.CreateModel();

            var queueName = _configuration["RabbitMQ:Queues:ProfilePictureUpload"]!;

            channel.QueueDeclare(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false);

            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.Received += async (_, ea) =>
            {
                try
                {
                    var json = Encoding.UTF8.GetString(ea.Body.ToArray());

                    var message = JsonSerializer.Deserialize<ProfilePictureUploadRequestedMessage>(json);

                    if (message == null)
                        return;

                    using var scope = _scopeFactory.CreateScope();

                    var storageService = scope.ServiceProvider.GetRequiredService<IStorageService>();

                    var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

                    var bytes = Convert.FromBase64String(message.Base64Content);

                    await using var stream = new MemoryStream(bytes);

                    var imageUrl = await storageService.UploadProfilePicture(
                        stream,
                        message.FileName,
                        message.ContentType,
                        message.UserId);

                    var user = await userRepository.FindByUserId(message.UserId);

                    if (user == null)
                        return;

                    user.ProfilePicUrl = imageUrl;

                    await userRepository.Update(user);

                    channel.BasicAck(ea.DeliveryTag, false);
                }
                catch
                {
                    channel.BasicNack(ea.DeliveryTag, false, true);
                }
            };

            channel.BasicConsume(
                queue: queueName,
                autoAck: false,
                consumer: consumer);

            return Task.CompletedTask;
        }
    }
}