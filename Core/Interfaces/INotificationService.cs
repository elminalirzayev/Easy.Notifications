using Easy.Notifications.Core.Enums;
using Easy.Notifications.Core.Models;

namespace Easy.Notifications.Core.Interfaces
{
    /// <summary>
    /// Defines the contract for a notification service that can send messages to various channels.
    /// </summary>
    public interface INotificationService
    {
        /// <summary>
        /// Sends a notification message to the specified channels.
        /// </summary>
        /// <param name="message">The notification message to send.</param>
        /// <param name="channels">The channels to send the notification to.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task SendToAsync(NotificationMessage message, params ChannelType[] channels);

        /// <summary>
        /// Sends a notification message to all available channels.
        /// </summary>
        /// <param name="message">The notification message to send.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task SendToAllAsync(NotificationMessage message);
    }
}
