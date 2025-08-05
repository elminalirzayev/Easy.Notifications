using Easy.Notifications.Core.Enums;
using Easy.Notifications.Core.Interfaces;
using Easy.Notifications.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Twilio;
using Twilio.Clients;
using Twilio.Rest.Api.V2010.Account;

namespace Easy.Notifications.Providers.Sms
{

    public class TwilioSmsProvider : ISmsProvider
    {
        private readonly SmsConfiguration _settings;
        private readonly ILogger<TwilioSmsProvider> _logger;

        public ChannelType Channel => ChannelType.Sms;
        private readonly ITwilioRestClient _twilioClient;
        public string Name => "Twilio";


        public TwilioSmsProvider(IOptions<SmsConfiguration> options, ITwilioRestClient twilioClient, ILogger<TwilioSmsProvider> logger)
        {
            _settings = options.Value;
            _logger = logger;
            _twilioClient = twilioClient;
            TwilioClient.Init(_settings.Username, _settings.Password);
        }

        public async Task SendAsync(NotificationMessage message)
        {

            if (!message.Recipients.Any())
            {
                _logger.LogWarning("Twilio: Alıcı listesi boş.");
                return;
            }

            if (string.IsNullOrEmpty(message.Body))
            {
                _logger.LogWarning("Twilio: Mesaj içeriği boş.");
                return;
            }
            _logger.LogInformation("Twilio SMS gönderiliyor: {RecipientsCount} alıcı", message.Recipients.Count);



            foreach (var to in message.Recipients)
            {
                try
                {
                    var response = await MessageResource.CreateAsync(
               body: message.Body,
               from: new Twilio.Types.PhoneNumber(_settings.Sender),
               to: new Twilio.Types.PhoneNumber(to),
               client: _twilioClient);


                    _logger.LogInformation("SMS gönderildi: {To}, SID: {Sid}", to, response.Sid);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Twilio ile SMS gönderilemedi: {To}", to);
                }
            }
        }
    }
}
