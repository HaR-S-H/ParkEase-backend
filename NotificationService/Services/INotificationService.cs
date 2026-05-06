using NotificationService.Models;

namespace NotificationService.Services
{
    public interface INotificationService
    {
        Task Send(Notification notification);
        Task SendBulk(List<int> recipientIds, string title, string message);
        Task SendVerificationEmail(string email, string fullName, string token);
        Task SendForgotPasswordEmail(string email, string fullName, string temporaryPassword);
        Task MarkAsRead(int notificationId);
        Task MarkAllRead(int recipientId);
        Task<List<Notification>> GetByRecipient(int recipientId);
        Task<int> GetUnreadCount(int recipientId);
        Task DeleteNotification(int notificationId);
        Task SendEmail(string email, string subject, string body);
        Task SendSms(string phoneNumber, string body);
        Task<List<Notification>> GetAll();
    }
}
