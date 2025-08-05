using Easy.Notifications.Core.Enums;
using Easy.Notifications.Core.Interfaces;
using Easy.Notifications.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace Easy.Notifications.Providers.WhatsApp
{
    public class TwilioWhatsAppNotificationProvider : INotificationProvider
    {
        private readonly WhatsAppConfiguration _config;
        private readonly ILogger<TwilioWhatsAppNotificationProvider> _logger;

        public ChannelType Channel => ChannelType.WhatsApp;
        public string Name => "TwilioWhatsApp";

        public TwilioWhatsAppNotificationProvider(
            IOptions<WhatsAppConfiguration> options,
            ILogger<TwilioWhatsAppNotificationProvider> logger)
        {
            _config = options.Value;
            _logger = logger;
            TwilioClient.Init(_config.AccountSid, _config.AuthToken);
        }

        public async Task SendAsync(NotificationMessage message)
        {
            foreach (var recipient in message.Recipients)
            {
                var to = $"whatsapp:{recipient}";
                var from = $"whatsapp:{_config.Sender}";

                try
                {
                    var msg = await MessageResource.CreateAsync(
                        body: message.Body,
                        from: new Twilio.Types.PhoneNumber(from),
                        to: new Twilio.Types.PhoneNumber(to)
                    );

                    _logger.LogInformation("WhatsApp mesajı gönderildi: {Sid}", msg.Sid);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "WhatsApp mesajı gönderilemedi: {To}", recipient);
                }
            }
        }
    }
}
