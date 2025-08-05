using Easy.Notifications.Core.Enums;
using Easy.Notifications.Core.Interfaces;
using Easy.Notifications.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

namespace Easy.Notifications.Providers.Slack
{
    public class SlackNotificationProvider : INotificationProvider
    {
        private readonly SlackConfiguration _config;
        private readonly HttpClient _httpClient;
        private readonly ILogger<SlackNotificationProvider> _logger;

        public ChannelType Channel => ChannelType.Slack;
        public string Name => "Slack";

        public SlackNotificationProvider(
            IOptions<SlackConfiguration> options,
            HttpClient httpClient,
            ILogger<SlackNotificationProvider> logger)
        {
            _config = options.Value;
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task SendAsync(NotificationMessage message)
        {
            // If there is a custom block, use it; otherwise, create a simple block
            var payload = message.Metadata.TryGetValue("slackBlock", out var obj)
                ? obj
                : CreateDefaultBlockPayload(message);

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");


            try
            {
                var response = await _httpClient.PostAsync(_config.WebhookUrl, content);

                if (!response.IsSuccessStatusCode)
                    _logger.LogError("Slack message could not be sent. Status: {StatusCode}", response.StatusCode);
                else
                    _logger.LogInformation("Slack message sent.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while sending the Slack message.");
            }
        }

        private object CreateDefaultBlockPayload(NotificationMessage message)
        {
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
                            new { type = "mrkdwn", text = $"_Sent time: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}_"}
                        }
                    }
                }
            };
        }
    }
}
