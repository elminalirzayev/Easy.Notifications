using Easy.Notifications.Core.Abstractions;
using Easy.Notifications.Core.Models;
using Easy.Notifications.Providers.Teams.Models;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Net.Http;

namespace Easy.Notifications.Providers.Chat
{
    /// <summary>
    /// Notification provider for sending rich cards to Microsoft Teams via Incoming Webhooks.
    /// Supports Legacy MessageCard format.
    /// </summary>
    public class TeamsProvider : INotificationProvider
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<TeamsProvider> _logger;

        /// <summary>
        /// Gets the supported channel type (Teams).
        /// </summary>
        public NotificationChannelType SupportedChannel => NotificationChannelType.Teams;

        /// <summary>
        /// Initializes a new instance of the <see cref="TeamsProvider"/> class.
        /// </summary>
        /// <param name="httpClientFactory">Factory to create HttpClient instances.</param>
        /// <param name="logger">Logger to capture runtime errors.</param>
        public TeamsProvider(IHttpClientFactory httpClientFactory, ILogger<TeamsProvider> logger)
        {
            _httpClient = httpClientFactory.CreateClient("Teams");
            _logger = logger;
        }

        /// <summary>
        /// Sends a Teams Message Card to the specified webhook.
        /// </summary>
        /// <param name="recipient">The recipient containing the Webhook URL.</param>
        /// <param name="subject">The title of the card.</param>
        /// <param name="body">The main content of the card.</param>
        /// <param name="metadata">Optional metadata to customize color or facts (e.g., ThemeColor).</param>
        /// <returns>A task representing the result of the operation. Returns true if successful.</returns>
        public async Task<bool> SendAsync(Recipient recipient, string subject, string body, Dictionary<string, object>? metadata = null)
        {
            try
            {
                // 1. Create the rich card object
                var card = new TeamsMessageCard
                {
                    Title = subject,
                    Text = body,
                    Summary = subject, // Required for notification popups
                    ThemeColor = "0078D7" // Default Teams Blue
                };

                // 2. Enhance card with metadata if provided
                if (metadata != null)
                {
                    // Check for custom theme color
                    if (metadata.ContainsKey("ThemeColor"))
                    {
                        card.ThemeColor = metadata["ThemeColor"]?.ToString() ?? "0078D7";
                    }

                    // Add a "Details" section for other metadata items
                    var section = new TeamsSection { ActivityTitle = "Details" };
                    foreach (var item in metadata)
                    {
                        // Skip ThemeColor as it is already used
                        if (item.Key != "ThemeColor")
                        {
                            section.Facts.Add(new TeamsFact(item.Key, item.Value?.ToString() ?? ""));
                        }
                    }

                    // Only add section if it has facts
                    if (section.Facts.Any())
                    {
                        card.Sections.Add(section);
                    }
                }

                // 3. Configure JSON Serialization
                // Use 'UnsafeRelaxedJsonEscaping' for proper character encoding (e.g., Turkish characters)
                // Use 'WhenWritingNull' to exclude empty fields to keep payload clean
                var options = new JsonSerializerOptions
                {
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };

                var jsonPayload = JsonSerializer.Serialize(card, options);
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                // 4. Send Request (Recipient.Value is the Webhook URL)
                var response = await _httpClient.PostAsync(recipient.Value, content);

                // 5. Log Error if Failed
                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync();

                    _logger.LogError(" TEAMS API ERROR \nStatus: {StatusCode}\nMessage: {ErrorBody}\nSent JSON: {JsonPayload}",
                        response.StatusCode, errorBody, jsonPayload);
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, " TEAMS EXCEPTION ");
                return false;
            }
        }
    }
}