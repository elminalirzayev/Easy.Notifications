using Easy.Notifications.Core.Abstractions;
using Easy.Notifications.Core.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Easy.Notifications.Infrastructure.Dispatcher
{
    /// <summary>
    /// Background service that periodically checks the database for failed notifications 
    /// and attempts to resend them based on retry policies from configuration.
    /// </summary>
    public class NotificationRetryWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<NotificationRetryWorker> _logger;
        private readonly IOptionsMonitor<RetryConfiguration> _retryOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationRetryWorker"/>.
        /// </summary>
        /// <param name="serviceProvider">The service provider to create scopes.</param>
        /// <param name="logger">The logger instance.</param>
        /// <param name="retryOptions">The dynamic retry configuration options.</param>
        public NotificationRetryWorker(
            IServiceProvider serviceProvider,
            ILogger<NotificationRetryWorker> logger,
            IOptionsMonitor<RetryConfiguration> retryOptions)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _retryOptions = retryOptions;
        }

        /// <summary>
        /// Executes the retry logic in a periodic loop.
        /// </summary>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Notification Retry Worker is starting with Interval: {Interval} mins.",
                _retryOptions.CurrentValue.IntervalInMinutes);

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

                // Wait for the next interval based on current configuration
                var delay = TimeSpan.FromMinutes(_retryOptions.CurrentValue.IntervalInMinutes);
                await Task.Delay(delay, stoppingToken);
            }
        }

        /// <summary>
        /// Retrieves pending retries from the store and dispatches them through the registered providers.
        /// </summary>
        private async Task ProcessRetriesAsync(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var store = scope.ServiceProvider.GetService<INotificationStore>();

            if (store == null)
            {
                _logger.LogDebug("No persistence store registered. Retry worker skipped.");
                return;
            }

            var config = _retryOptions.CurrentValue;
            var providers = scope.ServiceProvider.GetServices<INotificationProvider>().ToList();

            // Use MaxRetryCount from configuration
            var pendingNotifications = await store.GetPendingRetriesAsync(config.MaxRetryCount);

            foreach (var payload in pendingNotifications)
            {
                if (stoppingToken.IsCancellationRequested) break;

                _logger.LogInformation("Attempting to retry notification {Id}. Recipients: {Count}",
                    payload.Id, payload.Recipients.Count);

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

                    // Update the status in the database
                    await store.UpdateStatusAsync(payload.Id, isSuccess, isSuccess ? null : "Retry attempt failed.");

                    if (isSuccess)
                    {
                        _logger.LogInformation("Notification {Id} successfully resent to {Recipient}", payload.Id, recipient.Value);
                    }
                }
            }
        }
    }
}