using Easy.Notifications.Core.Models;

namespace Easy.Notifications.Core.Abstractions
{
    /// <summary>
    /// Defines a specific channel provider (e.g. SMTP, Twilio, Slack).
    /// </summary>
    public interface INotificationProvider
    {
        /// <summary>
        /// Gets the channel type supported by this provider.
        /// </summary>
        NotificationChannelType SupportedChannel { get; }

        /// <summary>
        /// Sends the notification to a single recipient.
        /// </summary>
        /// <param name="recipient">The target recipient.</param>
        /// <param name="subject">The processed subject.</param>
        /// <param name="body">The processed body.</param>
        /// <param name="metadata">Optional metadata.</param>
        /// <returns>True if successful, otherwise false.</returns>
        Task<bool> SendAsync(Recipient recipient, string subject, string body, Dictionary<string, object>? metadata = null);
    }
}