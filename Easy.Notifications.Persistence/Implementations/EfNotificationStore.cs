using Easy.Notifications.Core.Abstractions;
using Easy.Notifications.Core.Models;
using Easy.Notifications.Persistence.Entities;


// Modern .NET (EF Core)
#if !NETFRAMEWORK
using Microsoft.EntityFrameworkCore;
#else 
using System.Data.Entity;
#endif

namespace Easy.Notifications.Persistence.Implementations
{
    /// <summary>
    /// Entity Framework Core implementation of the notification log store.
    /// </summary>
    public class EfNotificationStore : INotificationStore
    {
        private readonly NotificationDbContext _context;

        /// <summary>
        /// Initializes a new instance of the EfNotificationStore.
        /// </summary>
        public EfNotificationStore(NotificationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Records a new notification log in the SQL database.
        /// </summary>
        public async Task SaveLogAsync(Guid id, string recipient, string channel, string subject, string body)
        {

            var channelType = (NotificationChannelType)Enum.Parse(typeof(NotificationChannelType), channel);

            var log = new NotificationLog
            {
                Id = id,
                Recipient = recipient,
                Channel = channelType,
                Subject = subject,
                Body = body,
                IsSuccess = false
            };

            _context.NotificationLogs.Add(log);

#if !NETFRAMEWORK
            await _context.SaveChangesAsync();
#else
            _context.SaveChanges();
#endif
        }

        /// <summary>
        /// Updates an existing log entry status based on provider results.
        /// </summary>
        public async Task UpdateStatusAsync(Guid id, bool isSuccess, string? errorMessage = null)
        {
            var log = await _context.NotificationLogs.FirstOrDefaultAsync(x => x.Id == id);
            if (log != null)
            {
                log.IsSuccess = isSuccess;
                log.ErrorMessage = errorMessage;
                log.SentAt = DateTime.UtcNow;
#if !NETFRAMEWORK
                await _context.SaveChangesAsync();
#else
            _context.SaveChanges();
#endif
            }
        }
    }
}