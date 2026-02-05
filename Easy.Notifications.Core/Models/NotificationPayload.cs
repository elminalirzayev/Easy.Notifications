namespace Easy.Notifications.Core.Models
{
    /// <summary>
    /// Represents the full content and metadata of a notification request.
    /// Includes a unique identifier for persistence tracking.
    /// </summary>
    public class NotificationPayload
    {
        /// <summary>
        /// Gets or sets the unique identifier for this notification. 
        /// Used for tracking logs across different persistence layers.
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Gets or sets the priority level. Defaults to Normal.
        /// </summary>
        public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;

        /// <summary>
        /// Gets or sets the subject or title of the notification.
        /// </summary>
        public string Subject { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the body content. Supports placeholders like {{Name}}.
        /// </summary>
        public string Body { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the data used to replace placeholders in the Body/Subject.
        /// </summary>
        public Dictionary<string, string> TemplateData { get; set; } = new();

        /// <summary>
        /// Gets or sets the list of recipients.
        /// </summary>
        public List<Recipient> Recipients { get; set; } = new();

        /// <summary>
        /// Gets or sets additional metadata for providers (e.g. Teams ThemeColor, Slack Context).
        /// </summary>
        public Dictionary<string, object>? Metadata { get; set; }
    }
}