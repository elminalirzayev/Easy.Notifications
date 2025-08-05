using Easy.Notifications.Core.Enums;
using Easy.Notifications.Core.Interfaces;
using Easy.Notifications.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

namespace Easy.Notifications.Providers.Teams
{
    /// <summary>
    /// Notification provider for sending messages to Microsoft Teams using a webhook.
    /// </summary>
    public class TeamsNotificationProvider : INotificationProvider
    {
        private readonly TeamsConfiguration _config;
        private readonly HttpClient _httpClient;
        private readonly ILogger<TeamsNotificationProvider> _logger;

        /// <summary>
        /// Gets the channel type for this provider (Teams).
        /// </summary>
        public ChannelType Channel => ChannelType.Teams;

        /// <summary>
        /// Gets the name of the provider.
        /// </summary>
        public string Name => "Teams";

        /// <summary>
        /// Initializes a new instance of the <see cref="TeamsNotificationProvider"/> class.
        /// </summary>
        /// <param name="options">Teams configuration options.</param>
        /// <param name="httpClient">HTTP client for sending requests.</param>
        /// <param name="logger">Logger instance.</param>
        public TeamsNotificationProvider(
            IOptions<TeamsConfiguration> options,
            HttpClient httpClient,
            ILogger<TeamsNotificationProvider> logger)
        {
            _config = options.Value;
            _httpClient = httpClient;
            _logger = logger;
        }

        /// <summary>
        /// Sends a notification message to Microsoft Teams.
        /// </summary>
        /// <param name="message">The notification message to send.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task SendAsync(NotificationMessage message)
        {
            // Try to get a custom Teams card from metadata, otherwise create a default card
            var card = message.Metadata.TryGetValue("teamsCard", out var obj)
               ? obj as TeamsMessageCard
               : new TeamsMessageCard
               {
                   Summary = message.Title,
                   Title = message.Title,
                   ThemeColor = "0076D7",
                   Sections = new List<TeamsSection>
                   {
                        new TeamsSection
                        {
                            Text = message.Body
                        }
                   }
               };

            // Serialize the card to JSON with camelCase property names
            var json = JsonSerializer.Serialize(card, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                // Send the POST request to the Teams webhook URL
                var response = await _httpClient.PostAsync(_config.WebhookUrl, content);

                // Log the result
                if (!response.IsSuccessStatusCode)
                    _logger.LogError("Teams message could not be sent. Status: {StatusCode}", response.StatusCode);
                else
                    _logger.LogInformation("Teams message sent.");
            }
            catch (Exception ex)
            {
                // Log any exception that occurs during sending
                _logger.LogError(ex, "An error occurred while sending the Teams message.");
            }
        }
    }
}
