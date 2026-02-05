using Easy.Notifications.Core.Abstractions;
using Easy.Notifications.Core.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Easy.Notifications.Infrastructure.Dispatcher
{
    /// <summary>
    /// Background service that periodically checks the database for failed notifications 
    /// and attempts to resend them based on retry policies.
    /// </summary>
    public class NotificationRetryWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<NotificationRetryWorker> _logger;
        private const int MaxRetryCount = 3;
        private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(5);

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationRetryWorker"/>.
        /// </summary>
        public NotificationRetryWorker(
            IServiceProvider serviceProvider,
            ILogger<NotificationRetryWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        /// <summary>
        /// Executes the retry logic in a periodic loop.
        /// </summary>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Notification Retry Worker is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessRetriesAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while processing notification retries.");
                }

                // Wait for the next interval before checking the database again
                await Task.Delay(_checkInterval, stoppingToken);
            }
        }

        /// <summary>
        /// Retrieves pending retries from the store and dispatches them through the registered providers.
        /// </summary>
        private async Task ProcessRetriesAsync(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var store = scope.ServiceProvider.GetService<INotificationStore>();

            // If no persistence store is registered, this worker has nothing to do.
            if (store == null) return;

            var providers = scope.ServiceProvider.GetServices<INotificationProvider>().ToList();
            var pendingNotifications = await store.GetPendingRetriesAsync(MaxRetryCount);

            foreach (var payload in pendingNotifications)
            {
                if (stoppingToken.IsCancellationRequested) break;

                _logger.LogInformation("Attempting to retry notification {Id}", payload.Id);

                foreach (var recipient in payload.Recipients)
                {
                    var provider = providers.FirstOrDefault(p => p.SupportedChannel == recipient.ChannelType);

                    if (provider == null)
                    {
                        _logger.LogWarning("No provider found for retry channel: {Channel}", recipient.ChannelType);
                        continue;
                    }

                    // Dispatch the notification again
                    var isSuccess = await provider.SendAsync(recipient, payload.Subject, payload.Body, payload.Metadata);

                    // Update the status in the database (this will increment RetryCount or set IsSuccess)
                    await store.UpdateStatusAsync(payload.Id, isSuccess, isSuccess ? null : "Retry attempt failed.");
                }
            }
        }
    }
}