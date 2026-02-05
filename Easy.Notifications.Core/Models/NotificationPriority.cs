namespace Easy.Notifications.Core.Models
{
    /// <summary>
    /// Defines the priority levels for notifications.
    /// Higher priority notifications are processed first.
    /// </summary>
    public enum NotificationPriority
    {
        /// <summary>
        /// Non-urgent notifications, like newsletters or promotions.
        /// </summary>
        Low = 0,

        /// <summary>
        /// Default priority for regular notifications.
        /// </summary>
        Normal = 1,

        /// <summary>
        /// Important notifications, like account updates.
        /// </summary>
        High = 2,

        /// <summary>
        /// Critical notifications that must be sent immediately, like OTP or security alerts.
        /// </summary>
        Urgent = 3
    }
}