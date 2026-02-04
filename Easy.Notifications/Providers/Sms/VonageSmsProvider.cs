using Easy.Notifications.Core.Abstractions;
using Easy.Notifications.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace Easy.Notifications.Providers.Sms
{
    /// <summary>
    /// Notification provider for sending SMS via Vonage (Nexmo) REST API.
    /// </summary>
    public class VonageSmsProvider : INotificationProvider
    {
        private readonly VonageConfiguration _config;
        private readonly HttpClient _httpClient;
        private readonly ILogger<VonageSmsProvider> _logger;

        /// <summary>
        /// Gets the supported channel type (Sms).
        /// </summary>
        public NotificationChannelType SupportedChannel => NotificationChannelType.Sms;

        /// <summary>
        /// Initializes a new instance of the <see cref="VonageSmsProvider"/> class.
        /// </summary>
        /// <param name="config">Vonage API settings (ApiKey, ApiSecret, Sender).</param>
        /// <param name="factory">IHttpClientFactory to manage HttpClient lifetime.</param>
        /// <param name="logger">Logger for capturing transmission errors.</param>
        public VonageSmsProvider(IOptions<VonageConfiguration> config, IHttpClientFactory factory, ILogger<VonageSmsProvider> logger)
        {
            _config = config.Value;
            _httpClient = factory.CreateClient("Vonage");
            _logger = logger;
        }

        /// <summary>
        /// Sends an SMS message using Vonage JSON API.
        /// </summary>
        /// <param name="recipient">The recipient containing the phone number.</param>
        /// <param name="subject">The subject (not used in standard SMS).</param>
        /// <param name="body">The SMS text content.</param>
        /// <param name="metadata">Optional metadata.</param>
        /// <returns>A task representing the operation result. Returns true if the message status is 0 (Success).</returns>
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

                if (!response.IsSuccessStatusCode)
                {
                    var errorMsg = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Vonage SMS HTTP Error: {Status} - {Details}", response.StatusCode, errorMsg);
                    return false;
                }

                // IMPORTANT: Vonage returns 200 OK even if the message fails (e.g., invalid balance).
                // We must check the "status" field in the response body.
                var responseBody = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(responseBody);
                var messages = doc.RootElement.GetProperty("messages");

                if (messages.GetArrayLength() > 0)
                {
                    var status = messages[0].GetProperty("status").GetString();

                    // Status "0" means success in Vonage API
                    if (status == "0") return true;

                    var errorText = messages[0].GetProperty("error-text").GetString();
                    _logger.LogWarning("Vonage SMS Rejected: {ErrorText} (Status: {Status})", errorText, status);
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Vonage SMS Exception for {Phone}", recipient.Value);
                return false;
            }
        }
    }
}