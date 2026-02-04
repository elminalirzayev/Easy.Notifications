namespace Easy.Notifications.Core.Models
{
    /// <summary>
    /// Represents the full content and metadata of a notification request.
    /// </summary>
    public class NotificationPayload
    {
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
        /// Gets or sets additional metadata for providers (e.g. Attachments).
        /// </summary>
        public Dictionary<string, object>? Metadata { get; set; }
    }
}