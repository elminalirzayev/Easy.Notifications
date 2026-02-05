using Easy.Notifications.Core.Models;

namespace Easy.Notifications.Persistence.Entities
{
    /// <summary>
    /// Represents a persistent record of a sent notification with retry capabilities.
    /// </summary>
    public class NotificationLog
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid CorrelationId { get; set; }
        /// <summary>
        /// The group identifier for batch operations.
        /// </summary>
        public string? GroupId { get; set; } 

        /// <summary>
        /// Indicates if the notification was cancelled before sending.
        /// </summary>
        public bool IsCancelled { get; set; } // <-- İptal özelliği için
        public string Recipient { get; set; } = string.Empty;
        public NotificationChannelType Channel { get; set; }
        public NotificationPriority Priority { get; set; }
        public string? Subject { get; set; }
        public string Body { get; set; } = string.Empty;
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? SentAt { get; set; }

        /// <summary>
        /// Gets or sets the number of times this notification has been attempted.
        /// </summary>
        public int RetryCount { get; set; } = 0;

        /// <summary>
        /// Gets or sets the scheduled time for the next retry attempt.
        /// </summary>
        public DateTime? NextRetryAt { get; set; }
    }
}