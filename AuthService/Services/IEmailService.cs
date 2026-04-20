namespace AuthService.Services
{
    public interface IEmailService
    {
        Task SendVerificationEmail(string email, string fullName, string token);
        Task SendTemporaryPasswordEmail(string email, string fullName, string temporaryPassword);
    }
}
