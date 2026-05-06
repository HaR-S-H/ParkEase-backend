using Microsoft.AspNetCore.SignalR;
using NotificationService.Hubs;
using NotificationService.Models;
using NotificationService.Repositories;

namespace NotificationService.Services
{
    public class NotificationService : INotificationService
    {
        private static readonly HashSet<string> AllowedTypes =
        ["BOOKING", "CHECKIN", "EXPIRY", "CHECKOUT", "PAYMENT", "PROMO"];

        private static readonly HashSet<string> AllowedChannels =
        ["APP", "EMAIL", "SMS"];

        private readonly INotificationRepository _repo;
        private readonly IEmailSender _emailSender;
        private readonly ISmsSender _smsSender;
        private readonly IHubContext<NotificationHub> _hub;

        public NotificationService(
            INotificationRepository repo,
            IEmailSender emailSender,
            ISmsSender smsSender,
            IHubContext<NotificationHub> hub)
        {
            _repo = repo;
            _emailSender = emailSender;
            _smsSender = smsSender;
            _hub = hub;
        }

        public async Task Send(Notification notification)
        {
            NormalizeNotification(notification);
            notification.IsRead = false;
            notification.SentAt = DateTime.UtcNow;

            var created = await _repo.Create(notification);
            await PublishInApp(created);
        }

        public async Task SendBulk(List<int> recipientIds, string title, string message)
        {
            if (recipientIds == null || recipientIds.Count == 0)
            {
                return;
            }

            var sanitizedTitle = (title ?? string.Empty).Trim();
            var sanitizedMessage = (message ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(sanitizedTitle) || string.IsNullOrWhiteSpace(sanitizedMessage))
            {
                throw new AppException("Title and message are required.", StatusCodes.Status400BadRequest);
            }

            foreach (var recipientId in recipientIds.Distinct())
            {
                if (recipientId <= 0)
                {
                    continue;
                }

                var notification = new Notification
                {
                    RecipientId = recipientId,
                    Type = "PROMO",
                    Title = sanitizedTitle,
                    Message = sanitizedMessage,
                    Channel = "APP",
                    IsRead = false,
                    SentAt = DateTime.UtcNow
                };

                var created = await _repo.Create(notification);
                await PublishInApp(created);
            }
        }

        public Task SendVerificationEmail(string email, string fullName, string token)
        {
            return _emailSender.SendVerificationEmail(email, fullName, token);
        }

        public Task SendForgotPasswordEmail(string email, string fullName, string temporaryPassword)
        {
            return _emailSender.SendForgotPasswordEmail(email, fullName, temporaryPassword);
        }

        public async Task MarkAsRead(int notificationId)
        {
            var notification = await _repo.FindByNotificationId(notificationId)
                ?? throw new AppException("Notification not found.", StatusCodes.Status404NotFound);

            if (notification.IsRead)
            {
                return;
            }

            notification.IsRead = true;
            await _repo.Update(notification);
        }

        public async Task MarkAllRead(int recipientId)
        {
            var notifications = await _repo.FindByRecipientIdAndIsRead(recipientId, false);
            foreach (var notification in notifications)
            {
                notification.IsRead = true;
                await _repo.Update(notification);
            }
        }

        public Task<List<Notification>> GetByRecipient(int recipientId)
        {
            return _repo.FindByRecipientId(recipientId);
        }

        public Task<int> GetUnreadCount(int recipientId)
        {
            return _repo.CountByRecipientIdAndIsRead(recipientId, false);
        }

        public async Task DeleteNotification(int notificationId)
        {
            var existing = await _repo.FindByNotificationId(notificationId)
                ?? throw new AppException("Notification not found.", StatusCodes.Status404NotFound);

            await _repo.DeleteByNotificationId(existing.NotificationId);
        }

        public Task SendEmail(string email, string subject, string body)
        {
            if (string.IsNullOrWhiteSpace(email)
                || string.IsNullOrWhiteSpace(subject)
                || string.IsNullOrWhiteSpace(body))
            {
                throw new AppException("Email, subject and body are required.", StatusCodes.Status400BadRequest);
            }

            return _emailSender.SendGenericEmail(email.Trim(), subject.Trim(), body.Trim());
        }

        public Task SendSms(string phoneNumber, string body)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber) || string.IsNullOrWhiteSpace(body))
            {
                throw new AppException("Phone number and SMS body are required.", StatusCodes.Status400BadRequest);
            }

            return _smsSender.SendSms(phoneNumber.Trim(), body.Trim());
        }

        public Task<List<Notification>> GetAll()
        {
            return _repo.FindAll();
        }

        private static void NormalizeNotification(Notification notification)
        {
            if (notification.RecipientId <= 0)
            {
                throw new AppException("RecipientId must be greater than zero.", StatusCodes.Status400BadRequest);
            }

            notification.Type = (notification.Type ?? string.Empty).Trim().ToUpperInvariant();
            notification.Channel = (notification.Channel ?? string.Empty).Trim().ToUpperInvariant();
            notification.Title = (notification.Title ?? string.Empty).Trim();
            notification.Message = (notification.Message ?? string.Empty).Trim();
            notification.RelatedType = string.IsNullOrWhiteSpace(notification.RelatedType)
                ? null
                : notification.RelatedType.Trim().ToUpperInvariant();

            if (!AllowedTypes.Contains(notification.Type))
            {
                throw new AppException("Invalid notification type.", StatusCodes.Status400BadRequest);
            }

            if (!AllowedChannels.Contains(notification.Channel))
            {
                throw new AppException("Invalid notification channel.", StatusCodes.Status400BadRequest);
            }

            if (string.IsNullOrWhiteSpace(notification.Title) || string.IsNullOrWhiteSpace(notification.Message))
            {
                throw new AppException("Title and message are required.", StatusCodes.Status400BadRequest);
            }
        }

        private Task PublishInApp(Notification notification)
        {
            return _hub.Clients
                .Group(NotificationHub.GroupNameForRecipient(notification.RecipientId))
                .SendAsync("notification-received", new
                {
                    notification.NotificationId,
                    notification.RecipientId,
                    notification.Type,
                    notification.Title,
                    notification.Message,
                    notification.Channel,
                    notification.RelatedId,
                    notification.RelatedType,
                    notification.IsRead,
                    notification.SentAt
                });
        }
    }
}
