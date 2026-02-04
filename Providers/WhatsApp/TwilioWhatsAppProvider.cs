using Easy.Notifications.Core.Abstractions;
using Easy.Notifications.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Twilio.Clients;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace Easy.Notifications.Providers.Chat
{
    public class TwilioWhatsAppProvider : INotificationProvider
    {
        private readonly TwilioConfiguration _config;
        private readonly ITwilioRestClient _client;
        private readonly ILogger<TwilioWhatsAppProvider> _logger;
        public NotificationChannelType SupportedChannel => NotificationChannelType.WhatsApp;

        public TwilioWhatsAppProvider(IOptions<TwilioConfiguration> config, ITwilioRestClient client, ILogger<TwilioWhatsAppProvider> logger)
        {
            _config = config.Value;
            _client = client;
            _logger = logger;
        }

        public async Task<bool> SendAsync(Recipient recipient, string subject, string body, Dictionary<string, object>? metadata = null)
        {
            try
            {
                var toNum = recipient.Value.StartsWith("whatsapp:") ? recipient.Value : $"whatsapp:{recipient.Value}";
                var fromNum = _config.FromNumber.StartsWith("whatsapp:") ? _config.FromNumber : $"whatsapp:{_config.FromNumber}";

                var msg = await MessageResource.CreateAsync(
                    to: new PhoneNumber(toNum),
                    from: new PhoneNumber(fromNum),
                    body: body,
                    client: _client
                );
                return msg.Status != MessageResource.StatusEnum.Failed;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Twilio WhatsApp Failed to {Phone}", recipient.Value);
                return false;
            }
        }
    }
}