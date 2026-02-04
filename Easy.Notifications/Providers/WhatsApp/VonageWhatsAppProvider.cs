using Easy.Notifications.Core.Abstractions;
using Easy.Notifications.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Net.Http;

namespace Easy.Notifications.Providers.Chat
{
    /// <summary>
    /// Notification provider for sending WhatsApp messages via Vonage Messages API (Sandbox).
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
        /// <param name="config">Vonage configuration settings.</param>
        /// <param name="httpClientFactory">Factory to create HttpClient instances.</param>
        /// <param name="logger">Logger for error tracking.</param>
        public VonageWhatsAppProvider(
            IOptions<VonageConfiguration> config,
            IHttpClientFactory httpClientFactory,
            ILogger<VonageWhatsAppProvider> logger)
        {
            _config = config.Value;
            _httpClient = httpClientFactory.CreateClient("Vonage");
            _logger = logger;
        }

        /// <summary>
        /// Sends a WhatsApp text message using Vonage Messages API Sandbox.
        /// </summary>
        /// <param name="recipient">The recipient's phone number.</param>
        /// <param name="subject">The subject (unused for WhatsApp).</param>
        /// <param name="body">The message content.</param>
        /// <param name="metadata">Optional metadata.</param>
        /// <returns>True if the message was successfully queued by Vonage.</returns>
        public async Task<bool> SendAsync(Recipient recipient, string subject, string body, Dictionary<string, object>? metadata = null)
        {
            try
            {
                // 1. Prepare the JSON payload based on Vonage Messages API
                var payload = new
                {
                    from = _config.FromNumber, // Your Vonage Sandbox number
                    to = recipient.Value,      // Recipient number
                    message_type = "text",
                    text = body,
                    channel = "whatsapp"
                };

                // 2. Prepare the request
                var request = new HttpRequestMessage(HttpMethod.Post, "https://messages-sandbox.nexmo.com/v1/messages");

                // Set Basic Authentication header (ApiKey:ApiSecret encoded in Base64)
                var authString = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_config.ApiKey}:{_config.ApiSecret}"));
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authString);

                // Set Content
                var json = JsonSerializer.Serialize(payload);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // 3. Execute request
                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorDetails = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Vonage WhatsApp API Error: {Status} - {Details}", response.StatusCode, errorDetails);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while sending Vonage WhatsApp message to {Phone}", recipient.Value);
                return false;
            }
        }
    }
}