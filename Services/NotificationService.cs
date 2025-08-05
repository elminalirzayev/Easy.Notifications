using Easy.Notifications.Core.Enums;
using Easy.Notifications.Core.Interfaces;
using Easy.Notifications.Core.Models;
using Microsoft.Extensions.Logging;

namespace Easy.Notifications.Services
{
    /// <summary>
    /// Service for sending notifications through various providers.
    /// </summary>
    public class NotificationService : INotificationService
    {
        private readonly IEnumerable<INotificationProvider> _providers;
        private readonly ILogger<NotificationService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationService"/> class.
        /// </summary>
        /// <param name="providers">A collection of notification providers.</param>
        /// <param name="logger">Logger instance.</param>
        public NotificationService(IEnumerable<INotificationProvider> providers, ILogger<NotificationService> logger)
        {
            _providers = providers;
            _logger = logger;
        }

        /// <summary>
        /// Sends a notification message to the specified channels.
        /// </summary>
        /// <param name="message">The notification message to send.</param>
        /// <param name="channels">The channels to send the notification to.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task SendToAsync(NotificationMessage message, params ChannelType[] channels)
        {
            // Filter providers by the specified channels
            var selectedProviders = _providers.Where(p => channels.Contains(p.Channel));
            if (!selectedProviders.Any())
            {
                // Log a warning if no providers are found for the channels
                _logger.LogWarning("No notification providers found for the specified channels: {Channels}", string.Join(", ", channels));
                return;
            }

            // Send the message using each selected provider
            foreach (var provider in selectedProviders)
            {
                try
                {
                    await provider.SendAsync(message);
                }
                catch (Exception ex)
                {
                    // Log any exception that occurs during sending
                    _logger.LogError(ex, "Failed to send notification via {Channel}", provider.Channel);
                }
            }
        }

        /// <summary>
        /// Sends a notification message to all available providers.
        /// </summary>
        /// <param name="message">The notification message to send.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task SendToAllAsync(NotificationMessage message)
        {
            // Send the message using all providers
            foreach (var provider in _providers)
            {
                try
                {
                    await provider.SendAsync(message);
                }
                catch (Exception ex)
                {
                    // Log any exception that occurs during sending
                    _logger.LogError(ex, "Failed to send notification via {Channel}", provider.Channel);
                }
            }
        }

        /// <summary>
        /// Not implemented. Reserved for future use.
        /// </summary>
        /// <param name="message">The notification message to send.</param>
        /// <returns>Throws <see cref="NotImplementedException"/>.</returns>
        public Task SendAsync(NotificationMessage message)
        {
            throw new NotImplementedException();
        }
    }
}
