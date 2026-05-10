using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

namespace AuthService.Messaging
{
    public class RabbitMqPublisher : IRabbitMqPublisher
    {
        private readonly IConfiguration _configuration;

        public RabbitMqPublisher(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Task Publish<T>(string queueName, T message)
        {
            var factory = new ConnectionFactory
            {
                Uri = new Uri(_configuration["RabbitMQ:Url"]!)
            };

            using var connection = factory.CreateConnection();

            using var channel = connection.CreateModel();

            channel.QueueDeclare(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false);

            var body = Encoding.UTF8.GetBytes(
                JsonSerializer.Serialize(message));

            channel.BasicPublish(
                exchange: "",
                routingKey: queueName,
                body: body);

            return Task.CompletedTask;
        }
    }
}