using Easy.Notifications.Core.Abstractions;
using Easy.Notifications.Core.Models;
using Easy.Notifications.Persistence.EntityFramework.Entities;

#if NETFRAMEWORK
using System.Data.Entity;
#else
using Microsoft.EntityFrameworkCore;
#endif

namespace Easy.Notifications.Persistence.EntityFramework.Implementations
{
    /// <summary>
    /// Implementation of the notification store using Entity Framework.
    /// Compatible with both EF Core and EF 6.
    /// </summary>
    public class EfNotificationStore : INotificationStore
    {
        private readonly NotificationDbContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="EfNotificationStore"/> class.
        /// </summary>
        /// <param name="context">The database context.</param>
        public EfNotificationStore(NotificationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Records a new notification log into the database. 
        /// The initial state is set to Failed/Pending with a RetryCount of 0.
        /// </summary>
        /// <param name="id">The unique identifier of the notification.</param>
        /// <param name="correlationId">The correlation ID for tracking.</param>
        /// <param name="recipient">The recipient address (email, phone, webhook url).</param>
        /// <param name="channel">The channel type as a string (Email, Sms, etc.).</param>
        /// <param name="subject">The notification subject.</param>
        /// <param name="body">The notification body content.</param>
        /// <param name="priority">The priority level as a string.</param>
        /// <param name="groupId">The optional group/campaign ID.</param>
        /// <returns>A task that represents the asynchronous save operation.</returns>
        public async Task SaveLogAsync(Guid id, Guid correlationId, string recipient, string channel, string subject, string body, string priority, string? groupId)
        {
            // Note: Ensure input strings match Enum names exactly, or this will throw.
            var channelType = (NotificationChannelType)Enum.Parse(typeof(NotificationChannelType), channel);
            var priorityType = (NotificationPriority)Enum.Parse(typeof(NotificationPriority), priority);

            var log = new NotificationLog
            {
                Id = id,
                CorrelationId = correlationId,
                GroupId = groupId,
                Recipient = recipient,
                Channel = channelType,
                Subject = subject,
                Body = body,
                Priority = priorityType,
                IsSuccess = false, // Initially false until confirmed sent
                RetryCount = 0,
                CreatedAt = DateTime.UtcNow
            };

            _context.NotificationLogs.Add(log);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Updates the status of a notification log after a delivery attempt.
        /// If delivery failed, it increments the retry count and schedules the next retry.
        /// </summary>
        /// <param name="id">The unique identifier of the notification.</param>
        /// <param name="isSuccess">True if sent successfully; otherwise, false.</param>
        /// <param name="errorMessage">The error message provider if failed.</param>
        /// <returns>A task that represents the asynchronous update operation.</returns>
        public async Task UpdateStatusAsync(Guid id, bool isSuccess, string? errorMessage = null)
        {
            var log = await _context.NotificationLogs.FirstOrDefaultAsync(x => x.Id == id);

            if (log != null)
            {
                log.IsSuccess = isSuccess;
                log.ErrorMessage = errorMessage;
                log.SentAt = isSuccess ? DateTime.UtcNow : (DateTime?)null;

                if (!isSuccess)
                {
                    log.RetryCount++;

                    // Linear Backoff Strategy: 5m, 10m, 15m, etc.
                    // If you want Exponential (5, 25, 125), use Math.Pow(5, log.RetryCount)
                    log.NextRetryAt = DateTime.UtcNow.AddMinutes(5 * log.RetryCount);
                }

                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Fetches failed logs that are eligible for a retry and maps them back to the payload model.
        /// </summary>
        /// <param name="maxRetryCount">The maximum number of retries allowed before giving up.</param>
        /// <returns>A collection of <see cref="NotificationPayload"/> ready to be re-queued.</returns>
        public async Task<IEnumerable<NotificationPayload>> GetPendingRetriesAsync(int maxRetryCount)
        {
            var now = DateTime.UtcNow;

            var logs = await _context.NotificationLogs
                .Where(x => !x.IsSuccess &&
                            !x.IsCancelled && // Do not retry cancelled items
                            x.RetryCount < maxRetryCount &&
                            (x.NextRetryAt == null || x.NextRetryAt <= now))
                .ToListAsync();

            // Mapping to Payload
            return logs.Select(l => new NotificationPayload
            {
                Id = l.Id,
                Subject = l.Subject ?? string.Empty,
                Body = l.Body,
                Priority = l.Priority,
                GroupId = l.GroupId,
                Recipients = new List<Recipient>
                {
                    new Recipient(l.Recipient, l.Channel,string.Empty)
                }
            });
        }

        /// <summary>
        /// Cancels all pending notifications for a specific group (e.g., a Campaign).
        /// </summary>
        /// <param name="groupId">The group identifier to cancel.</param>
        /// <returns>A task that represents the asynchronous cancellation operation.</returns>
        public async Task CancelGroupAsync(string groupId)
        {
            if (string.IsNullOrWhiteSpace(groupId)) return;

            var pendingLogs = await _context.NotificationLogs
                .Where(x => x.GroupId == groupId && !x.IsSuccess && !x.IsCancelled)
                .ToListAsync();

            foreach (var log in pendingLogs)
            {
                log.IsCancelled = true;
                log.ErrorMessage = "Cancelled by user request (Group Cancel).";
            }

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Checks if a specific notification or its group has been cancelled.
        /// </summary>
        /// <param name="id">The notification ID.</param>
        /// <param name="groupId">The optional group ID.</param>
        /// <returns>True if the notification is marked as cancelled; otherwise, false.</returns>
        public async Task<bool> IsCancelledAsync(Guid id, string? groupId)
        {
            return await _context.NotificationLogs
                .AnyAsync(x => (x.Id == id || (groupId != null && x.GroupId == groupId)) && x.IsCancelled);
        }
    }
}