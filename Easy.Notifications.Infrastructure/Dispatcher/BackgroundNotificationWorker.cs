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
        private readonly INotificationCancellationManager _cancellationManager; // <-- Yeni

        /// <summary>
        /// Initializes a new instance of the BackgroundNotificationWorker.
        /// </summary>
        public BackgroundNotificationWorker(
            IDictionary<NotificationPriority, Channel<NotificationPayload>> priorityChannels,
            IServiceProvider serviceProvider,
            ITemplateEngine templateEngine,
            ILogger<BackgroundNotificationWorker> logger,
            INotificationCancellationManager cancellationManager) // <-- Inject edildi
        {
            _priorityChannels = priorityChannels;
            _serviceProvider = serviceProvider;
            _templateEngine = templateEngine;
            _logger = logger;
            _cancellationManager = cancellationManager;
        }
        /// <summary>
        /// Core execution loop that monitors all priority channels.
        /// </summary>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
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

                foreach (var priority in priorityOrder)
                {
                    if (_priorityChannels.TryGetValue(priority, out var channel))
                    {
                        if (channel.Reader.TryRead(out var payload))
                        {
                            try
                            {
                                await ProcessPayloadAsync(payload);
                                processedAny = true;
                                break;
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Error processing notification payload {Id} from {Priority} queue.", payload.Id, priority);
                            }
                        }
                    }
                }

                if (!processedAny)
                {
                    await Task.Delay(100, stoppingToken);
                }
            }
        }

        /// <summary>
        /// Processes a single notification payload, manages scopes, and interacts with providers/stores.
        /// </summary>
        private async Task ProcessPayloadAsync(NotificationPayload payload)
        {
            if (_cancellationManager.IsGroupCancelled(payload.GroupId))
            {
                _logger.LogInformation("Notification {Id} skipped because group {Group} is cancelled (Memory Check).", payload.Id, payload.GroupId);

                using var cancelScope = _serviceProvider.CreateScope();
                var cancelStore = cancelScope.ServiceProvider.GetService<INotificationStore>();

                if (cancelStore != null)
                {
                    foreach (var recipient in payload.Recipients)
                    {
                        var logId = Guid.NewGuid();
                        await cancelStore.SaveLogAsync(
                            logId,
                            payload.Id,
                            recipient.Value,
                            recipient.ChannelType.ToString(),
                            payload.Subject, 
                            payload.Body,   
                            payload.Priority.ToString(),
                            payload.GroupId);

                        await cancelStore.UpdateStatusAsync(logId, false, "Cancelled by Group Request (Pre-check).");
                    }
                }
                return; 
            }

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

                if (store != null)
                {
                    await store.SaveLogAsync(logEntryId, payload.Id, recipient.Value, recipient.ChannelType.ToString(), processedSubject, processedBody, payload.Priority.ToString(), payload.GroupId);
                }

                var isSuccess = await provider.SendAsync(recipient, processedSubject, processedBody, payload.Metadata);

                if (store != null)
                {
                    await store.UpdateStatusAsync(logEntryId, isSuccess, isSuccess ? null : "Provider delivery failed.");
                }
            }
        }
    }
}