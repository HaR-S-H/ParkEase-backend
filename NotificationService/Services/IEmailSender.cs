namespace NotificationService.Services
{
    public interface IEmailSender
    {
        Task SendVerificationEmail(string email, string fullName, string token);
        Task SendForgotPasswordEmail(string email, string fullName, string temporaryPassword);
        Task SendGenericEmail(string email, string subject, string htmlBody);
    }
}
