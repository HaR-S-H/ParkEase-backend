namespace AuthService.Services
{
    public interface INotificationDispatcher
    {
        Task SendVerificationEmail(string email, string fullName, string token, CancellationToken cancellationToken = default);
        Task SendForgotPasswordEmail(string email, string fullName, string temporaryPassword, CancellationToken cancellationToken = default);
    }
}