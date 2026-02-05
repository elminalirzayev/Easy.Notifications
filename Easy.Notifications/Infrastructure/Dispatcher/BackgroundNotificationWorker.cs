using Easy.Notifications.Core.Abstractions;
using Easy.Notifications.Core.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading.Channels;

namespace Easy.Notifications.Infrastructure.Dispatcher
{
    /// <summary>
    /// Background service that consumes prioritized notification queues and dispatches to providers.
    /// Processes channels in order: Urgent > High > Normal > Low.
    /// </summary>
    public class BackgroundNotificationWorker : BackgroundService
    {
        private readonly IDictionary<NotificationPriority, Channel<NotificationPayload>> _priorityChannels;
        private readonly IServiceProvider _serviceProvider;
        private readonly ITemplateEngine _templateEngine;
        private readonly ILogger<BackgroundNotificationWorker> _logger;

        /// <summary>
        /// Initializes a new instance of the BackgroundNotificationWorker with priority channels.
        /// </summary>
        public BackgroundNotificationWorker(
            IDictionary<NotificationPriority, Channel<NotificationPayload>> priorityChannels,
            IServiceProvider serviceProvider,
            ITemplateEngine templateEngine,
            ILogger<BackgroundNotificationWorker> logger)
        {
            _priorityChannels = priorityChannels;
            _serviceProvider = serviceProvider;
            _templateEngine = templateEngine;
            _logger = logger;
        }

        /// <summary>
        /// Core execution loop that monitors all priority channels.
        /// </summary>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // We define the order of processing explicitly
            var priorityOrder = new[]
            {
                NotificationPriority.Urgent,
                NotificationPriority.High,
                NotificationPriority.Normal,
                NotificationPriority.Low
            };

            while (!stoppingToken.IsCancellationRequested)
            {
                bool processedAny = false;

                // Scan channels from highest to lowest priority
                foreach (var priority in priorityOrder)
                {
                    if (_priorityChannels.TryGetValue(priority, out var channel))
                    {
                        // Try to process one message from the current channel
                        if (channel.Reader.TryRead(out var payload))
                        {
                            try
                            {
                                await ProcessPayloadAsync(payload);
                                processedAny = true;
                                // After processing a high priority message, 
                                // we immediately jump back to the start of the loop 
                                // to ensure the next message processed is also the highest possible priority.
                                break;
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Error processing notification payload {Id} from {Priority} queue.", payload.Id, priority);
                            }
                        }
                    }
                }

                // If no channels had any messages, wait for any channel to have data to avoid 100% CPU usage
                if (!processedAny)
                {
                    // Wait for the Normal channel as a baseline, or use a small Task.Delay
                    // A better approach is to WaitToReadAsync on all channels, but for simplicity:
                    await Task.Delay(100, stoppingToken);
                }
            }
        }

        /// <summary>
        /// Processes a single notification payload, manages scopes, and interacts with providers/stores.
        /// </summary>
        private async Task ProcessPayloadAsync(NotificationPayload payload)
        {
            using var scope = _serviceProvider.CreateScope();
            var providers = scope.ServiceProvider.GetServices<INotificationProvider>();
            var store = scope.ServiceProvider.GetService<INotificationStore>();

            var processedBody = _templateEngine.Process(payload.Body, payload.TemplateData);
            var processedSubject = _templateEngine.Process(payload.Subject, payload.TemplateData);

            foreach (var recipient in payload.Recipients)
            {

                var logEntryId = Guid.NewGuid();

                var provider = providers.FirstOrDefault(p => p.SupportedChannel == recipient.ChannelType);

                if (provider == null)
                {
                    _logger.LogWarning("No provider registered for channel: {Channel}", recipient.ChannelType);
                    continue;
                }

                // Log as Pending if store is available
                if (store != null)
                {
                    await store.SaveLogAsync(logEntryId, payload.Id, recipient.Value, recipient.ChannelType.ToString(), processedSubject, processedBody, payload.Priority.ToString());
                }

                // Execute the actual dispatch
                var isSuccess = await provider.SendAsync(recipient, processedSubject, processedBody, payload.Metadata);

                // Update status in the database
                if (store != null)
                {
                    await store.UpdateStatusAsync(payload.Id, isSuccess, isSuccess ? null : "Provider delivery failed.");
                }
            }
        }
    }
}