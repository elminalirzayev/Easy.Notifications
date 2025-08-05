using Easy.Notifications.Core.Enums;
using Easy.Notifications.Core.Interfaces;
using Easy.Notifications.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

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
            // Eğer özel block varsa kullan, yoksa basit block oluştur
            var payload = message.Metadata.TryGetValue("slackBlock", out var obj)
                ? obj
                : CreateDefaultBlockPayload(message);

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");


            try
            {
                var response = await _httpClient.PostAsync(_config.WebhookUrl, content);

                if (!response.IsSuccessStatusCode)
                    _logger.LogError("Slack mesajı gönderilemedi. Status: {StatusCode}", response.StatusCode);
                else
                    _logger.LogInformation("Slack mesajı gönderildi.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Slack mesajı gönderilirken hata oluştu.");
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
                            new { type = "mrkdwn", text = $"_Gönderim zamanı: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}_"}
                        }
                    }
                }
            };
        }
    }
}
