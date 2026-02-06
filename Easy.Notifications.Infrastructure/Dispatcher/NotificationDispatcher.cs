using Easy.Notifications.Core.Abstractions;
using Easy.Notifications.Core.Models;
using System.Threading.Channels;

namespace Easy.Notifications.Infrastructure.Dispatcher
{
    /// <summary>
    /// Dispatches notification payloads to appropriate priority channels for background processing.
    /// </summary>
    public class NotificationDispatcher : INotificationService
    {
        private readonly IDictionary<NotificationPriority, Channel<NotificationPayload>> _channels;

        /// <summary>
        /// Initializes a new instance with a dictionary of priority channels.
        /// </summary>
        /// <param name="channels">The dictionary containing channels for each priority level.</param>
        public NotificationDispatcher(IDictionary<NotificationPriority, Channel<NotificationPayload>> channels)
        {
            _channels = channels;
        }

        /// <summary>
        /// Routes the notification to the channel corresponding to its priority.
        /// </summary>
        public async Task SendAsync(NotificationPayload payload)
        {
            if (_channels.TryGetValue(payload.Priority, out var channel))
            {
                await channel.Writer.WriteAsync(payload);
            }
            else
            {
                // Fallback to Normal if specific priority channel is missing
                await _channels[NotificationPriority.Normal].Writer.WriteAsync(payload);
            }
        }
    }
}