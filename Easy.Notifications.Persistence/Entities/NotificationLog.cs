using Easy.Notifications.Core.Models;

namespace Easy.Notifications.Persistence.EntityFramework.Entities
{
    /// <summary>
    /// Represents a persistent record of a sent notification with retry capabilities.
    /// </summary>
    public class NotificationLog
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid CorrelationId { get; set; }
        public string? GroupId { get; set; }
        public bool IsCancelled { get; set; }
        public string Recipient { get; set; } = string.Empty;
        public NotificationChannelType Channel { get; set; }
        public NotificationPriority Priority { get; set; }
        public string? Subject { get; set; }
        public string Body { get; set; } = string.Empty;
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? SentAt { get; set; }
        public int RetryCount { get; set; } = 0;
        public DateTime? NextRetryAt { get; set; }
    }
}