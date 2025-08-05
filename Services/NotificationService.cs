using Easy.Notifications.Core.Enums;
using Easy.Notifications.Core.Interfaces;
using Easy.Notifications.Core.Models;
using Microsoft.Extensions.Logging;

namespace Easy.Notifications.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IEnumerable<INotificationProvider> _providers;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(IEnumerable<INotificationProvider> providers, ILogger<NotificationService> logger)
        {
            _providers = providers;
            _logger = logger;
        }

        public async Task SendToAsync(NotificationMessage message, params ChannelType[] channels)
        {
            var selectedProviders = _providers.Where(p => channels.Contains(p.Channel));
            if (!selectedProviders.Any())
            {
                _logger.LogWarning("No notification providers found for the specified channels: {Channels}", string.Join(", ", channels));
                return;
            }


            foreach (var provider in selectedProviders)
            {
                try
                {
                    await provider.SendAsync(message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send notification via {Channel}", provider.Channel);
                }
            }
        }

        public async Task SendToAllAsync(NotificationMessage message)
        {
            foreach (var provider in _providers)
            {
                try
                {
                    await provider.SendAsync(message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send notification via {Channel}", provider.Channel);
                }
            }
        }

        public Task SendAsync(NotificationMessage message)
        {
            throw new NotImplementedException();
        }
    }
}
