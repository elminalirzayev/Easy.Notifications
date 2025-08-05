using Easy.Notifications.Core.Enums;
using Easy.Notifications.Core.Interfaces;
using Easy.Notifications.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Vonage;
using Vonage.Messaging;
using Vonage.Request;

namespace Easy.Notifications.Providers.Sms
{
    public class VonageSmsProvider : ISmsProvider
    {


        private readonly SmsConfiguration _settings;
        private readonly VonageClient _client;
        private readonly ILogger<VonageSmsProvider> _logger;

        public ChannelType Channel => ChannelType.Sms;
        public string Name => "Vonage";


        public VonageSmsProvider(IOptions<SmsConfiguration> options, ILogger<VonageSmsProvider> logger)
        {
            _settings = options.Value;
            _logger = logger;

            var credentials = Credentials.FromApiKeyAndSecret(_settings.Username, _settings.Password);
            _client = new VonageClient(credentials);
        }

        public async Task SendAsync(NotificationMessage message)
        {
            if (!message.Recipients.Any())
            {
                _logger.LogWarning("Vonage: Alıcı listesi boş.");
                return;
            }

            if (string.IsNullOrEmpty(message.Body))
            {
                _logger.LogWarning("Vonage: Mesaj içeriği boş.");
                return;
            }
            _logger.LogInformation("Vonage SMS gönderiliyor: {RecipientsCount} alıcı", message.Recipients.Count);


            foreach (var to in message.Recipients)
            {
                try
                {
                    var request = new SendSmsRequest
                    {
                        To = to,
                        From = _settings.Sender,
                        Text = message.Body
                    };

                    var response = await _client.SmsClient.SendAnSmsAsync(request);

                    if (response.Messages[0].Status != "0")
                    {
                        throw new InvalidOperationException($"Vonage SMS gönderimi başarısız: {response.Messages[0].ErrorText}");
                    }


                    _logger.LogInformation("SMS gönderildi: {To}, SID: {Sid}", to, response.Messages[0].MessageId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Vonage ile SMS gönderilemedi: {To}", to);
                }
            }



        }
    }
}