using Easy.Notifications.Core.Abstractions;
using Easy.Notifications.Core.Models;
using Easy.Notifications.Providers.Slack.Models;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Net.Http;

namespace Easy.Notifications.Providers.Chat
{
    /// <summary>
    /// Notification provider for sending rich messages to Slack via Incoming Webhooks.
    /// Uses Block Kit for advanced formatting.
    /// </summary>
    public class SlackProvider : INotificationProvider
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<SlackProvider> _logger;

        /// <summary>
        /// Gets the supported channel type (Slack).
        /// </summary>
        public NotificationChannelType SupportedChannel => NotificationChannelType.Slack;

        /// <summary>
        /// Initializes a new instance of the <see cref="SlackProvider"/> class.
        /// </summary>
        /// <param name="httpClientFactory">Factory to create HttpClient instances.</param>
        /// <param name="logger">Logger instance to capture errors and diagnostics.</param>
        public SlackProvider(IHttpClientFactory httpClientFactory, ILogger<SlackProvider> logger)
        {
            _httpClient = httpClientFactory.CreateClient("Slack");
            _logger = logger;
        }

        /// <summary>
        /// Sends a rich Block Kit message to the specified Slack webhook.
        /// </summary>
        /// <param name="recipient">The recipient containing the Webhook URL.</param>
        /// <param name="subject">The header/title of the message.</param>
        /// <param name="body">The main content body (supports Markdown).</param>
        /// <param name="metadata">Optional metadata to add as context blocks (footer).</param>
        /// <returns>A task representing the result of the operation. Returns true if Slack responds with success.</returns>
        public async Task<bool> SendAsync(Recipient recipient, string subject, string body, Dictionary<string, object>? metadata = null)
        {
            try
            {
                // 1. Prepare Payload
                var payload = new SlackPayload
                {
                    Text = subject // Fallback text for notifications
                };

                // Add Header Block (Must be 'plain_text')
                payload.Blocks.Add(new SlackBlock
                {
                    Type = "header",
                    Text = new SlackText(subject, "plain_text")
                });

                // Add Divider Block
                payload.Blocks.Add(new SlackBlock { Type = "divider" });

                // Add Section Block (Main Body, supports Markdown)
                payload.Blocks.Add(new SlackBlock
                {
                    Type = "section",
                    Text = new SlackText(body, "mrkdwn")
                });

                // 2. Add Metadata as Context (Footer)
                if (metadata != null && metadata.Any())
                {
                    var contextElements = new List<SlackText>();
                    foreach (var item in metadata)
                    {
                        // Format: *Key:* Value
                        contextElements.Add(new SlackText($"*{item.Key}:* {item.Value}", "mrkdwn"));

                        // Slack Context block allows max 10 elements. Break to prevent API error.
                        if (contextElements.Count >= 10) break;
                    }

                    payload.Blocks.Add(new SlackBlock
                    {
                        Type = "context",
                        Elements = contextElements
                    });
                }

                // 3. Configure JSON Serialization
                // Use 'UnsafeRelaxedJsonEscaping' to preserve non-ASCII characters (e.g., Turkish characters)
                // Use 'WhenWritingNull' to exclude null properties (required by Slack API)
                var options = new JsonSerializerOptions
                {
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };

                var jsonPayload = JsonSerializer.Serialize(payload, options);
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                // 4. Send Request
                var response = await _httpClient.PostAsync(recipient.Value, content);

                // 5. Log Error if Failed
                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync();

                    _logger.LogError("SLACK API ERROR \nStatus: {StatusCode}\nMessage: {ErrorBody}\nSent JSON: {JsonPayload}",
                        response.StatusCode, errorBody, jsonPayload);
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, " SLACK EXCEPTION");
                return false;
            }
        }
    }
}