using Microsoft.EntityFrameworkCore;
using NotificationService.Data;
using NotificationService.Models;

namespace NotificationService.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly NotificationDbContext _context;

        public NotificationRepository(NotificationDbContext context)
        {
            _context = context;
        }

        public Task<List<Notification>> FindByRecipientId(int recipientId)
        {
            return _context.Notifications
                .AsNoTracking()
                .Where(notification => notification.RecipientId == recipientId)
                .OrderByDescending(notification => notification.SentAt)
                .ToListAsync();
        }

        public Task<List<Notification>> FindByRecipientIdAndIsRead(int recipientId, bool isRead)
        {
            return _context.Notifications
                .AsNoTracking()
                .Where(notification => notification.RecipientId == recipientId && notification.IsRead == isRead)
                .OrderByDescending(notification => notification.SentAt)
                .ToListAsync();
        }

        public Task<int> CountByRecipientIdAndIsRead(int recipientId, bool isRead)
        {
            return _context.Notifications.CountAsync(notification => notification.RecipientId == recipientId && notification.IsRead == isRead);
        }

        public Task<List<Notification>> FindByType(string type)
        {
            return _context.Notifications
                .AsNoTracking()
                .Where(notification => notification.Type == type)
                .OrderByDescending(notification => notification.SentAt)
                .ToListAsync();
        }

        public Task<List<Notification>> FindByRelatedId(int relatedId)
        {
            return _context.Notifications
                .AsNoTracking()
                .Where(notification => notification.RelatedId == relatedId)
                .OrderByDescending(notification => notification.SentAt)
                .ToListAsync();
        }

        public Task<Notification?> FindByNotificationId(int notificationId)
        {
            return _context.Notifications.FirstOrDefaultAsync(notification => notification.NotificationId == notificationId);
        }

        public async Task<Notification> Create(Notification notification)
        {
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
            return notification;
        }

        public async Task Update(Notification notification)
        {
            _context.Notifications.Update(notification);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteByNotificationId(int notificationId)
        {
            var notification = await _context.Notifications.FindAsync(notificationId);
            if (notification != null)
            {
                _context.Notifications.Remove(notification);
                await _context.SaveChangesAsync();
            }
        }

        public Task<List<Notification>> FindAll()
        {
            return _context.Notifications
                .AsNoTracking()
                .OrderByDescending(notification => notification.SentAt)
                .ToListAsync();
        }
    }
}
