namespace AuthService.Messaging.Messages
{
    public class ForgotPasswordRequestedMessage
    {
        public required string Email { get; set; }
        public required string FullName { get; set; }
        public required string TemporaryPassword { get; set; }
    }
}
