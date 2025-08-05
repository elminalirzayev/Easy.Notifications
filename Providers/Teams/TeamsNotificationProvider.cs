using Easy.Notifications.Core.Enums;
using Easy.Notifications.Core.Interfaces;
using Easy.Notifications.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Easy.Notifications.Providers.Teams
{
    public class TeamsNotificationProvider : INotificationProvider
    {
        private readonly TeamsConfiguration _config;
        private readonly HttpClient _httpClient;
        private readonly ILogger<TeamsNotificationProvider> _logger;

        public ChannelType Channel => ChannelType.Teams;
        public string Name => "Teams";

        public TeamsNotificationProvider(
            IOptions<TeamsConfiguration> options,
            HttpClient httpClient,
            ILogger<TeamsNotificationProvider> logger)
        {
            _config = options.Value;
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task SendAsync(NotificationMessage message)
        {
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
            var json = JsonSerializer.Serialize(card, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            var content = new StringContent(json, Encoding.UTF8, "application/json");


            try
            {
                var response = await _httpClient.PostAsync(_config.WebhookUrl, content);

                if (!response.IsSuccessStatusCode)
                    _logger.LogError("Teams mesajı gönderilemedi. Status: {StatusCode}", response.StatusCode);
                else
                    _logger.LogInformation("Teams mesajı gönderildi.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Teams mesajı gönderilirken hata oluştu.");
            }
        }
    }
}
