using Easy.Notifications.Core.Abstractions;
using Easy.Notifications.Core.Models;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
using System.Net.Http;

namespace Easy.Notifications.Providers.Chat
{
    public class TelegramProvider : INotificationProvider
    {
        private readonly TelegramConfiguration _config;
        private readonly HttpClient _httpClient;
        public NotificationChannelType SupportedChannel => NotificationChannelType.Telegram;

        public TelegramProvider(IOptions<TelegramConfiguration> config, IHttpClientFactory factory)
        {
            _config = config.Value;
            _httpClient = factory.CreateClient("Telegram");
        }

        public async Task<bool> SendAsync(Recipient recipient, string subject, string body, Dictionary<string, object>? metadata = null)
        {
            var url = $"https://api.telegram.org/bot{_config.BotToken}/sendMessage";
            var payload = new { chat_id = recipient.Value, text = $"{subject}\n\n{body}" };
            var response = await _httpClient.PostAsync(url, new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json"));
            return response.IsSuccessStatusCode;
        }
    }
}