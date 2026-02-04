using Easy.Notifications.Core.Models;
using System.Threading.Tasks;

namespace Easy.Notifications.Core.Abstractions
{
    /// <summary>
    /// Defines the contract for the main notification service used by the application.
    /// This is the entry point for sending notifications.
    /// </summary>
    public interface INotificationService
    {
        /// <summary>
        /// Asynchronously queues a notification payload to be processed by background workers.
        /// This method is non-blocking (Fire-and-Forget).
        /// </summary>
        /// <param name="payload">The notification data containing recipients, subject, body, and template data.</param>
        /// <returns>A task that represents the asynchronous queue operation.</returns>
        Task SendAsync(NotificationPayload payload);
    }
}