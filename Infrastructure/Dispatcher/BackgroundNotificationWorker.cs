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
    /// </summary>
    public class BackgroundNotificationWorker : BackgroundService
    {
        private readonly Channel<NotificationPayload> _channel;
        private readonly IServiceProvider _serviceProvider;
        private readonly ITemplateEngine _templateEngine;
        private readonly ILogger<BackgroundNotificationWorker> _logger;

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
                        _logger.LogError(ex, "Error processing notification payload.");
                    }
                }
            }
        }

        private async Task ProcessPayloadAsync(NotificationPayload payload)
        {
            using var scope = _serviceProvider.CreateScope();
            var providers = scope.ServiceProvider.GetServices<INotificationProvider>();

            var processedBody = _templateEngine.Process(payload.Body, payload.TemplateData);
            var processedSubject = _templateEngine.Process(payload.Subject, payload.TemplateData);

            // Group recipients by channel type to minimize provider lookups
            var recipientGroups = payload.Recipients.GroupBy(r => r.ChannelType);

            foreach (var group in recipientGroups)
            {
                var channelType = group.Key;

                // Find the registered provider for this channel type
                var provider = providers.FirstOrDefault(p => p.SupportedChannel == channelType);

                if (provider == null)
                {
                    _logger.LogWarning("No provider registered for channel: {Channel}", channelType);
                    continue;
                }

                foreach (var recipient in group)
                {
                    // In a production scenario, you might want to execute these in parallel using Task.WhenAll
                    await provider.SendAsync(recipient, processedSubject, processedBody, payload.Metadata);
                }
            }
        }
    }
}