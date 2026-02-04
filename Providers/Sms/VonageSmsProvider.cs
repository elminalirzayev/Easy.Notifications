using Easy.Notifications.Core.Abstractions;
using Easy.Notifications.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace Easy.Notifications.Providers.Sms
{
    public class VonageSmsProvider : INotificationProvider
    {
        private readonly VonageConfiguration _config;
        private readonly HttpClient _httpClient;
        private readonly ILogger<VonageSmsProvider> _logger;
        public NotificationChannelType SupportedChannel => NotificationChannelType.Sms;

        public VonageSmsProvider(IOptions<VonageConfiguration> config, IHttpClientFactory factory, ILogger<VonageSmsProvider> logger)
        {
            _config = config.Value;
            _httpClient = factory.CreateClient("Vonage");
            _logger = logger;
        }

        public async Task<bool> SendAsync(Recipient recipient, string subject, string body, Dictionary<string, object>? metadata = null)
        {
            try
            {
                var payload = new
                {
                    api_key = _config.ApiKey,
                    api_secret = _config.ApiSecret,
                    from = _config.Sender,
                    to = recipient.Value,
                    text = body
                };

                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("https://rest.nexmo.com/sms/json", content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Vonage SMS Failed to {Phone}", recipient.Value);
                return false;
            }
        }
    }
}