namespace NotificationService.Messaging.Messages
{
    public class ForgotPasswordRequestedMessage
    {
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string TemporaryPassword { get; set; } = string.Empty;
    }
}
