using Easy.Notifications.Core.Abstractions;
using Easy.Notifications.Core.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading.Channels;

namespace Easy.Notifications.Infrastructure.Dispatcher
{
    /// <summary>
    /// Background service that consumes the notification queue and dispatches to providers.
    /// Supports optional persistence logging if INotificationStore is registered.
    /// </summary>
    public class BackgroundNotificationWorker : BackgroundService
    {
        private readonly Channel<NotificationPayload> _channel;
        private readonly IServiceProvider _serviceProvider;
        private readonly ITemplateEngine _templateEngine;
        private readonly ILogger<BackgroundNotificationWorker> _logger;

        /// <summary>
        /// Initializes a new instance of the BackgroundNotificationWorker.
        /// </summary>
        public BackgroundNotificationWorker(
            Channel<NotificationPayload> channel,
            IServiceProvider serviceProvider,
            ITemplateEngine templateEngine,
            ILogger<BackgroundNotificationWorker> logger)
        {
            _channel = channel;
            _serviceProvider = serviceProvider;
            _templateEngine = templateEngine;
            _logger = logger;
        }

        /// <summary>
        /// Core execution loop of the background service.
        /// </summary>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (await _channel.Reader.WaitToReadAsync(stoppingToken))
            {
                while (_channel.Reader.TryRead(out var payload))
                {
                    try
                    {
                        await ProcessPayloadAsync(payload);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing notification payload {Id}", payload.Id);
                    }
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
                var provider = providers.FirstOrDefault(p => p.SupportedChannel == recipient.ChannelType);

                if (provider == null)
                {
                    _logger.LogWarning("No provider registered for channel: {Channel}", recipient.ChannelType);
                    continue;
                }

                // Log as Pending if store is available
                if (store != null)
                {
                    await store.SaveLogAsync(payload.Id, recipient.Value, recipient.ChannelType.ToString(), processedSubject, processedBody);
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