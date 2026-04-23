namespace NotificationService.Services
{
    public interface IEmailSender
    {
        Task SendVerificationEmail(string email, string fullName, string token);
        Task SendGenericEmail(string email, string subject, string htmlBody);
    }
}
