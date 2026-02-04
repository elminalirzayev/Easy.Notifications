using Easy.Notifications.Core.Abstractions;
using Easy.Notifications.Core.Models;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Easy.Notifications.Infrastructure.Dispatcher
{
    /// <summary>
    /// The default implementation of <see cref="INotificationService"/>.
    /// It dispatches notification payloads to an in-memory channel for background processing.
    /// </summary>
    public class NotificationDispatcher : INotificationService
    {
        private readonly Channel<NotificationPayload> _channel;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationDispatcher"/> class.
        /// </summary>
        /// <param name="channel">The background channel to write notifications to.</param>
        public NotificationDispatcher(Channel<NotificationPayload> channel)
        {
            _channel = channel;
        }

        /// <inheritdoc />
        public async Task SendAsync(NotificationPayload payload)
        {
            // We write to the channel asynchronously.
            // If the channel is bounded (has a limit) and full, this might wait or drop depending on configuration.
            // In our setup, we use an Unbounded channel, so it returns immediately.
            await _channel.Writer.WriteAsync(payload);
        }
    }
}