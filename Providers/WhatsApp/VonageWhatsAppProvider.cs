using Easy.Notifications.Core.Abstractions;
using Easy.Notifications.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
using System.Net.Http;

namespace Easy.Notifications.Providers.Chat
{
    /// <summary>
    /// Notification provider for sending WhatsApp messages via Vonage (Nexmo) API.
    /// </summary>
    public class VonageWhatsAppProvider : INotificationProvider
    {
        private readonly VonageConfiguration _config;
        private readonly HttpClient _httpClient;
        private readonly ILogger<VonageWhatsAppProvider> _logger;

        /// <summary>
        /// Gets the supported channel type (WhatsApp).
        /// </summary>
        public NotificationChannelType SupportedChannel => NotificationChannelType.WhatsApp;

        /// <summary>
        /// Initializes a new instance of the <see cref="VonageWhatsAppProvider"/> class.
        /// </summary>
        public VonageWhatsAppProvider(IOptions<VonageConfiguration> config, IHttpClientFactory httpClientFactory, ILogger<VonageWhatsAppProvider> logger)
        {
            _config = config.Value;
            _httpClient = httpClientFactory.CreateClient("Vonage");
            _logger = logger;
        }

        /// <summary>
        /// Sends a WhatsApp message using Vonage Message API.
        /// </summary>
        /// <returns>True if the request was accepted by Vonage.</returns>
        public async Task<bool> SendAsync(Recipient recipient, string subject, string body, Dictionary<string, object>? metadata = null)
        {
            try
            {
                var payload = new
                {
                    from = _config.Sender,
                    to = recipient.Value,
                    message_type = "text",
                    text = body,
                    channel = "whatsapp"
                };

                var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_config.ApiKey}:{_config.ApiSecret}"));
                var request = new HttpRequestMessage(HttpMethod.Post, "https://messages-sandbox.nexmo.com/v1/messages");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", auth);
                request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

                var response = await _httpClient.SendAsync(request);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Vonage WhatsApp Failed to {Phone}", recipient.Value);
                return false;
            }
        }
    }
}