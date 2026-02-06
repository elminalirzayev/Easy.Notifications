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
        public async Task SaveLogAsync(Guid id, Guid correlationId, string recipient, string channel, string subject, string body, string priority, string? groupId, CancellationToken cancellationToken = default)
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


        public async Task UpdateStatusAsync(Guid id, bool isSuccess, string? errorMessage = null, CancellationToken cancellationToken = default)
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

        public async Task<IEnumerable<NotificationPayload>> GetPendingRetriesAsync(int maxRetryCount, CancellationToken cancellationToken = default)
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

        public async Task CancelGroupAsync(string groupId, CancellationToken cancellationToken = default)
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

        public async Task<bool> IsCancelledAsync(Guid id, string? groupId, CancellationToken cancellationToken = default)
        {
            return await _context.NotificationLogs
                .AnyAsync(x => (x.Id == id || (groupId != null && x.GroupId == groupId)) && x.IsCancelled);
        }
    }
}