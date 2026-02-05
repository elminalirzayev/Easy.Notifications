using Easy.Notifications.Core.Models;

namespace Easy.Notifications.Persistence.Entities
{
    /// <summary>
    /// Represents a persistent record of a sent notification.
    /// </summary>
    public class NotificationLog
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Recipient { get; set; } = string.Empty;
        public NotificationChannelType Channel { get; set; }
        public string? Subject { get; set; }
        public string Body { get; set; } = string.Empty;
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? SentAt { get; set; }
    }
}