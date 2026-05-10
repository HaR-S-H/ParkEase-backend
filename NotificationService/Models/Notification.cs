namespace NotificationService.Models
{
    public class Notification
    {
        public int NotificationId { get; set; }
        public int RecipientId { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Channel { get; set; } = string.Empty;
        public int? RelatedId { get; set; }
        public string? RelatedType { get; set; }
        public bool IsRead { get; set; }
        public DateTime SentAt { get; set; }

        public int GetNotificationId() => NotificationId;
        public bool IsReadStatus() => IsRead;
        public void SetRead(bool isRead) => IsRead = isRead;
        public DateTime GetSentAt() => SentAt;
    }
}
