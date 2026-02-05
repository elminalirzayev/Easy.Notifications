using Easy.Notifications.Core.Models;

namespace Easy.Notifications.Core.Abstractions
{
    /// <summary>
    /// Defines storage operations for notification history logging.
    /// This interface allows the Core project to log messages without depending on a specific database technology.
    /// </summary>
    public interface INotificationStore
    {
        /// <summary>
        /// Creates a pending log entry in the database.
        /// </summary>
        /// <param name="id">The unique notification ID.</param>
        /// <param name="recipient">The recipient's address or identifier.</param>
        /// <param name="channel">The channel type as a string.</param>
        /// <param name="subject">The final processed subject.</param>
        /// <param name="body">The final processed body content.</param>
        /// <param name="priority" >The priority level as a string.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task SaveLogAsync(Guid id, string recipient, string channel, string subject, string body, string priority);

        /// <summary>
        /// Updates an existing log with the transmission result.
        /// </summary>
        /// <param name="id">The unique notification ID to update.</param>
        /// <param name="isSuccess">Indicates whether the provider successfully sent the message.</param>
        /// <param name="errorMessage">Detailed error message if the operation failed.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task UpdateStatusAsync(Guid id, bool isSuccess, string? errorMessage = null);

        /// <summary>
        /// Retrieves notifications that failed and are scheduled for a retry.
        /// </summary>
        /// <param name="maxRetryCount">The maximum allowed retry attempts.</param>
        /// <returns>A list of notification payloads ready to be re-dispatched.</returns>
        Task<IEnumerable<NotificationPayload>> GetPendingRetriesAsync(int maxRetryCount);
    }
}