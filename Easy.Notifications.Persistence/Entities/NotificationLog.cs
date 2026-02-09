using Easy.Notifications.Core.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Easy.Notifications.Persistence.EntityFramework.Entities
{
    /// <summary>
    /// Represents a persistent record of a sent notification with retry capabilities.
    /// </summary>
    [Table("NotificationLogs")]
    public class NotificationLog
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid CorrelationId { get; set; }
        public string? GroupId { get; set; }
        public bool IsCancelled { get; set; }
        [Required, MaxLength(255)]
        public string Recipient { get; set; } = string.Empty;
        [Column("Channel")]
        public NotificationChannelType Channel { get; set; }
        [Column("Priority")]
        public NotificationPriority Priority { get; set; }
        public string? Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? SentAt { get; set; }
        public int RetryCount { get; set; } = 0;
        public DateTime? NextRetryAt { get; set; }
        [ForeignKey(nameof(Channel))]
        public virtual ChannelTypeLookup? ChannelLookup { get; set; }
        [ForeignKey(nameof(Priority))]
        public virtual PriorityTypeLookup? PriorityLookup { get; set; }
    }
}