namespace Easy.Notifications.Core.Models
{
    /// <summary>
    /// Represents a recipient (User, Phone, Email, ChatId) for a notification.
    /// </summary>
    public class Recipient
    {
        /// <summary>
        /// Gets the value (Email address, Phone number, Chat ID, User ID).
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Gets the display name (optional).
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// Gets the target channel type.
        /// </summary>
        public NotificationChannelType ChannelType { get; }

        private Recipient(string value, NotificationChannelType channelType, string displayName)
        {
            Value = value;
            ChannelType = channelType;
            DisplayName = displayName;
        }

        /// <summary>
        /// Creates an Email recipient.
        /// </summary>
        public static Recipient Email(string email, string displayName = "")
            => new Recipient(email, NotificationChannelType.Email, displayName);

        /// <summary>
        /// Creates an SMS recipient.
        /// </summary>
        public static Recipient Sms(string phoneNumber, string displayName = "")
            => new Recipient(phoneNumber, NotificationChannelType.Sms, displayName);

        /// <summary>
        /// Creates a WhatsApp recipient.
        /// </summary>
        public static Recipient WhatsApp(string phoneNumber, string displayName = "")
            => new Recipient(phoneNumber, NotificationChannelType.WhatsApp, displayName);

        /// <summary>
        /// Creates a Chat recipient (Slack/Teams/Telegram).
        /// </summary>
        public static Recipient Chat(string webhookOrChatId, NotificationChannelType type, string displayName = "")
            => new Recipient(webhookOrChatId, type, displayName);

        /// <summary>
        /// Creates a SignalR User recipient.
        /// </summary>
        public static Recipient SignalR(string userId, string displayName = "")
            => new Recipient(userId, NotificationChannelType.SignalR, displayName);
    }
}