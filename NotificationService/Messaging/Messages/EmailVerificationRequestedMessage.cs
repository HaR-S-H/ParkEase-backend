namespace NotificationService.Messaging.Messages
{
    public class EmailVerificationRequestedMessage
    {
        public required string Email { get; set; }
        public required string FullName { get; set; }
        public required string Token { get; set; }
    }
}
