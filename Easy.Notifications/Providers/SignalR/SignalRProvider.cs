using Easy.Notifications.Core.Abstractions;
using Easy.Notifications.Core.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Easy.Notifications.Providers.Realtime
{
    // Default Hub
    public class NotificationHub : Hub { }

    public class SignalRProvider : INotificationProvider
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<SignalRProvider> _logger;
        public NotificationChannelType SupportedChannel => NotificationChannelType.SignalR;

        public SignalRProvider(IHubContext<NotificationHub> hub, ILogger<SignalRProvider> logger)
        {
            _hubContext = hub;
            _logger = logger;
        }

        public async Task<bool> SendAsync(Recipient recipient, string subject, string body, Dictionary<string, object>? metadata = null)
        {
            try
            {
                // recipient.Value is UserId
                await _hubContext.Clients.User(recipient.Value).SendAsync("ReceiveNotification", new { Subject = subject, Body = body });
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SignalR Error for {User}", recipient.Value);
                return false;
            }
        }
    }
}