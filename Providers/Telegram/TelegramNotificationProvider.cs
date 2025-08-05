using Easy.Notifications.Core.Enums;
using Easy.Notifications.Core.Interfaces;
using Easy.Notifications.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Easy.Notifications.Providers.Telegram
{
    public class TelegramNotificationProvider : INotificationProvider
    {
        private readonly TelegramConfiguration _config;
        private readonly HttpClient _httpClient;
        private readonly ILogger<TelegramNotificationProvider> _logger;

        public ChannelType Channel => ChannelType.Telegram;
        public string Name => "Telegram";

        public TelegramNotificationProvider(
            IOptions<TelegramConfiguration> options,
            HttpClient httpClient,
            ILogger<TelegramNotificationProvider> logger)
        {
            _config = options.Value;
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task SendAsync(NotificationMessage message)
        {

            var url = $"https://api.telegram.org/bot{_config.BotToken}/sendMessage";

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("chat_id", _config.ChatId),
                new KeyValuePair<string, string>("text", message.Body),
                new KeyValuePair<string, string>("parse_mode", "HTML") // optional
            });

            try
            {
                var response = await _httpClient.PostAsync(url, content);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Telegram message could not be sent: {ChatId}", _config.ChatId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while sending the Telegram message.");
            }

        }
    }

}
