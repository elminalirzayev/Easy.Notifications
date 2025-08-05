using Easy.Notifications.Core.Enums;
using Easy.Notifications.Core.Interfaces;
using Easy.Notifications.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Easy.Notifications.Providers.Telegram
{
    /// <summary>
    /// Notification provider for sending messages to Telegram using the Bot API.
    /// </summary>
    public class TelegramNotificationProvider : INotificationProvider
    {
        private readonly TelegramConfiguration _config;
        private readonly HttpClient _httpClient;
        private readonly ILogger<TelegramNotificationProvider> _logger;

        /// <summary>
        /// Gets the channel type for this provider (Telegram).
        /// </summary>
        public ChannelType Channel => ChannelType.Telegram;

        /// <summary>
        /// Gets the name of the provider.
        /// </summary>
        public string Name => "Telegram";

        /// <summary>
        /// Initializes a new instance of the <see cref="TelegramNotificationProvider"/> class.
        /// </summary>
        /// <param name="options">Telegram configuration options.</param>
        /// <param name="httpClient">HTTP client for sending requests.</param>
        /// <param name="logger">Logger instance.</param>
        public TelegramNotificationProvider(
            IOptions<TelegramConfiguration> options,
            HttpClient httpClient,
            ILogger<TelegramNotificationProvider> logger)
        {
            _config = options.Value;
            _httpClient = httpClient;
            _logger = logger;
        }

        /// <summary>
        /// Sends a notification message to Telegram.
        /// </summary>
        /// <param name="message">The notification message to send.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task SendAsync(NotificationMessage message)
        {
            // Build the Telegram Bot API URL for sending a message
            var url = $"https://api.telegram.org/bot{_config.BotToken}/sendMessage";

            // Prepare the form content for the POST request
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("chat_id", _config.ChatId),
                new KeyValuePair<string, string>("text", message.Body),
                new KeyValuePair<string, string>("parse_mode", "HTML") // optional: allows HTML formatting in the message
            });

            try
            {
                // Send the POST request to the Telegram API
                var response = await _httpClient.PostAsync(url, content);
                if (!response.IsSuccessStatusCode)
                {
                    // Log an error if the message could not be sent
                    _logger.LogError("Telegram message could not be sent: {ChatId}", _config.ChatId);
                }
            }
            catch (Exception ex)
            {
                // Log any exception that occurs during sending
                _logger.LogError(ex, "An error occurred while sending the Telegram message.");
            }
        }
    }
}
