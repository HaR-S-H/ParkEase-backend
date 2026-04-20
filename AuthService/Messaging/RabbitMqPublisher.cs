using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

namespace AuthService.Messaging
{
    public class RabbitMqPublisher : IRabbitMqPublisher, IDisposable
    {
        private readonly IConfiguration _configuration;
        private IConnection? _connection;
        private IModel? _channel;
        private readonly object _sync = new();

        public RabbitMqPublisher(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Task Publish<T>(string queueName, T message, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            EnsureConnection();

            var payload = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(payload);

            var channel = _channel ?? throw new InvalidOperationException("RabbitMQ channel is not initialized.");

            channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false);

            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;

            channel.BasicPublish(
                exchange: string.Empty,
                routingKey: queueName,
                basicProperties: properties,
                body: body);

            return Task.CompletedTask;
        }

        private void EnsureConnection()
        {
            if (_connection != null && _channel != null && _connection.IsOpen && _channel.IsOpen)
            {
                return;
            }

            lock (_sync)
            {
                if (_connection != null && _channel != null && _connection.IsOpen && _channel.IsOpen)
                {
                    return;
                }

                _channel?.Dispose();
                _connection?.Dispose();

                var factory = CreateFactory();

                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();
            }
        }

        private ConnectionFactory CreateFactory()
        {
            var url = _configuration["RabbitMQ:Url"]
                ?? throw new InvalidOperationException("Missing RabbitMQ:Url.");

            return new ConnectionFactory
            {
                Uri = new Uri(url)
            };
        }

        public void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
        }
    }
}
