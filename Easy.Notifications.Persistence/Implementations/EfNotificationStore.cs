using Easy.Notifications.Core.Abstractions;
using Easy.Notifications.Core.Models;
using Easy.Notifications.Persistence.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#if NETFRAMEWORK
using System.Data.Entity;
#else
using Microsoft.EntityFrameworkCore;
#endif

namespace Easy.Notifications.Persistence.Implementations
{
    public class EfNotificationStore : INotificationStore
    {
        private readonly NotificationDbContext _context;

        public EfNotificationStore(NotificationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Records a new notification log. Initial state has RetryCount as 0.
        /// </summary>
        public async Task SaveLogAsync(Guid id, string recipient, string channel, string subject, string body,string priority)
        {
            var channelType = (NotificationChannelType)Enum.Parse(typeof(NotificationChannelType), channel);
            var priorityType = (NotificationPriority)Enum.Parse(typeof(NotificationPriority), priority);

            var log = new NotificationLog
            {
                Id = id,
                Recipient = recipient,
                Channel = channelType,
                Subject = subject,
                Body = body,
                Priority= priorityType,
                IsSuccess = false,
                RetryCount = 0,
                CreatedAt = DateTime.UtcNow
            };

            _context.NotificationLogs.Add(log);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Updates status. If failed, increments RetryCount and sets NextRetryAt (e.g., +5 minutes).
        /// </summary>
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
                    // Exponential backoff or simple fixed interval (e.g., 5 mins)
                    log.NextRetryAt = DateTime.UtcNow.AddMinutes(5 * log.RetryCount);
                }

                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Fetches failed logs that are due for a retry and maps them back to NotificationPayload.
        /// </summary>
        public async Task<IEnumerable<NotificationPayload>> GetPendingRetriesAsync(int maxRetryCount)
        {
            var now = DateTime.UtcNow;

            var logs = await _context.NotificationLogs
                .Where(x => !x.IsSuccess &&
                            x.RetryCount < maxRetryCount &&
                            (x.NextRetryAt == null || x.NextRetryAt <= now))
                .ToListAsync();

            // Mapping using static factory methods to satisfy private constructor
            return logs.Select(l => new NotificationPayload
            {
                Id = l.Id,
                Subject = l.Subject ?? string.Empty,
                Body = l.Body,
                Recipients = new List<Recipient>
                    {
                        Recipient.Chat(l.Recipient, l.Channel)
                    }
            });
        }
    }
}