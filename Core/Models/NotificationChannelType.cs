namespace Easy.Notifications.Core.Models
{
    /// <summary>
    /// Defines the supported communication channels.
    /// </summary>
    public enum NotificationChannelType
    {
        Email,
        Sms,
        WhatsApp,
        Slack,
        Teams,
        Telegram,
        SignalR
    }
}