namespace AuthService.Messaging.Messages
{
    public class VerificationEmailMessage
    {
        public required string Email { get; set; }
        public required string FullName { get; set; }
        public required string Token { get; set; }
        public int? UserId { get; set; }
    }

    public class ForgotPasswordEmailMessage
    {
        public required string Email { get; set; }
        public required string FullName { get; set; }
        public required string TemporaryPassword { get; set; }
        public int? UserId { get; set; }
    }
}
