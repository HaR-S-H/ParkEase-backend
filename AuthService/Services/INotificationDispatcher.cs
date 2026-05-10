namespace AuthService.Services
{
    public interface INotificationDispatcher
    {
        Task SendVerificationEmail(string email, string fullName, string token, int? recipientId = null, CancellationToken cancellationToken = default);
        Task SendForgotPasswordEmail(string email, string fullName, string temporaryPassword, int? recipientId = null, CancellationToken cancellationToken = default);
    }
}