namespace NotificationService.Messaging.Messages
{
    public class EmailSendRequestMessage
    {
        public required string Email { get; set; }
        public required string Subject { get; set; }
        public required string HtmlBody { get; set; }
    }
}
