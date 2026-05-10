namespace NotificationService.Services
{
    public interface ISmsSender
    {
        Task SendSms(string phoneNumber, string body);
    }
}
