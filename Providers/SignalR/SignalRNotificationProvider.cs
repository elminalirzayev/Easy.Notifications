using Easy.Notifications.Core.Enums;
using Easy.Notifications.Core.Interfaces;
using Easy.Notifications.Core.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Easy.Notifications.Providers.SignalR
{
    public class SignalRNotificationProvider : INotificationProvider
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<SignalRNotificationProvider> _logger;

        public ChannelType Channel => ChannelType.SignalR;
        public string Name => "SignalR";

        public SignalRNotificationProvider(IHubContext<NotificationHub> hubContext, ILogger<SignalRNotificationProvider> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task SendAsync(NotificationMessage message)
        {
            try
            {
                // message.Recipients burada kullanıcı bağlantı Id'leri ya da kullanıcı isimleri olabilir
                foreach (var recipient in message.Recipients)
                {
                    await _hubContext.Clients.User(recipient).SendAsync("ReceiveNotification", new
                    {
                        Title = message.Title,
                        Body = message.Body,
                        Metadata = message.Metadata
                    });
                }

                _logger.LogInformation("SignalR notification sent to {Count} recipients.", message.Recipients.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending SignalR notification.");
                throw;
            }
        }
    }
}
