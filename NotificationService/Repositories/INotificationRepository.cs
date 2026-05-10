using NotificationService.Models;

namespace NotificationService.Repositories
{
    public interface INotificationRepository
    {
        Task<List<Notification>> FindByRecipientId(int recipientId);
        Task<List<Notification>> FindByRecipientIdAndIsRead(int recipientId, bool isRead);
        Task<int> CountByRecipientIdAndIsRead(int recipientId, bool isRead);
        Task<List<Notification>> FindByType(string type);
        Task<List<Notification>> FindByRelatedId(int relatedId);
        Task<Notification?> FindByNotificationId(int notificationId);
        Task<Notification> Create(Notification notification);
        Task Update(Notification notification);
        Task DeleteByNotificationId(int notificationId);
        Task<List<Notification>> FindAll();
    }
}
