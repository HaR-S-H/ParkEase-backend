namespace NotificationService.Messaging
{
    public interface IRabbitMqPublisher
    {
        Task Publish<T>(string queueName, T message, CancellationToken cancellationToken = default);
    }
}
