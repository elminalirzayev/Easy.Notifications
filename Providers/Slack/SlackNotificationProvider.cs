using Easy.Notifications.Core.Enums;
using Easy.Notifications.Core.Interfaces;
using Easy.Notifications.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

namespace Easy.Notifications.Providers.Slack
{
    /// <summary>
    /// Notification provider for sending messages to Slack using a webhook.
    /// </summary>
    public class SlackNotificationProvider : INotificationProvider
    {
        private readonly SlackConfiguration _config;
        private readonly HttpClient _httpClient;
        private readonly ILogger<SlackNotificationProvider> _logger;

        /// <summary>
        /// Gets the channel type for this provider (Slack).
        /// </summary>
        public ChannelType Channel => ChannelType.Slack;

        /// <summary>
        /// Gets the name of the provider.
        /// </summary>
        public string Name => "Slack";

        /// <summary>
        /// Initializes a new instance of the <see cref="SlackNotificationProvider"/> class.
        /// </summary>
        /// <param name="options">Slack configuration options.</param>
        /// <param name="httpClient">HTTP client for sending requests.</param>
        /// <param name="logger">Logger instance.</param>
        public SlackNotificationProvider(
            IOptions<SlackConfiguration> options,
            HttpClient httpClient,
            ILogger<SlackNotificationProvider> logger)
        {
            _config = options.Value;
            _httpClient = httpClient;
            _logger = logger;
        }

        /// <summary>
        /// Sends a notification message to Slack.
        /// </summary>
        /// <param name="message">The notification message to send.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task SendAsync(NotificationMessage message)
        {
            // If there is a custom block in metadata, use it; otherwise, create a default block payload
            var payload = message.Metadata.TryGetValue("slackBlock", out var obj)
                ? obj
                : CreateDefaultBlockPayload(message);

            // Serialize the payload to JSON and prepare the HTTP content
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            try
            {
                // Send the POST request to the Slack webhook URL
                var response = await _httpClient.PostAsync(_config.WebhookUrl, content);

                // Log the result
                if (!response.IsSuccessStatusCode)
                    _logger.LogError("Slack message could not be sent. Status: {StatusCode}", response.StatusCode);
                else
                    _logger.LogInformation("Slack message sent.");
            }
            catch (Exception ex)
            {
                // Log any exception that occurs during sending
                _logger.LogError(ex, "An error occurred while sending the Slack message.");
            }
        }

        /// <summary>
        /// Creates a default Slack block payload if no custom block is provided.
        /// </summary>
        /// <param name="message">The notification message.</param>
        /// <returns>An object representing the default Slack block payload.</returns>
        private object CreateDefaultBlockPayload(NotificationMessage message)
        {
            // The payload contains a section with the message and a context with the sent time
            return new
            {
                blocks = new object[]
                {
                    new
                    {
                        type = "section",
                        text = new
                        {
                            type = "mrkdwn",
                            text = $"*{message.Title}*\n{message.Body}"
                        }
                    },
                    new
                    {
                        type = "context",
                        elements = new object[]
                        {
                            new { type = "mrkdwn", text = $"_Sent time: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}_" }
                        }
                    }
                }
            };
        }
    }
}
