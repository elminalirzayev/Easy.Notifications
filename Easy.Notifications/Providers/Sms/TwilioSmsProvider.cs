using Easy.Notifications.Core.Abstractions;
using Easy.Notifications.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Twilio.Clients;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace Easy.Notifications.Providers.Sms
{
    public class TwilioSmsProvider : INotificationProvider
    {
        private readonly TwilioConfiguration _config;
        private readonly ITwilioRestClient _client;
        private readonly ILogger<TwilioSmsProvider> _logger;
        public NotificationChannelType SupportedChannel => NotificationChannelType.Sms;

        public TwilioSmsProvider(IOptions<TwilioConfiguration> config, ITwilioRestClient client, ILogger<TwilioSmsProvider> logger)
        {
            _config = config.Value;
            _client = client;
            _logger = logger;
        }

        public async Task<bool> SendAsync(Recipient recipient, string subject, string body, Dictionary<string, object>? metadata = null)
        {
            try
            {
                var msg = await MessageResource.CreateAsync(
                    to: new PhoneNumber(recipient.Value),
                    from: new PhoneNumber(_config.FromNumber),
                    body: body,
                    client: _client
                );
                return msg.Status != MessageResource.StatusEnum.Failed;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Twilio SMS Failed to {Phone}", recipient.Value);
                return false;
            }
        }
    }
}