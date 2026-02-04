using Easy.Notifications.Core.Abstractions;
using Easy.Notifications.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;

namespace Easy.Notifications.Providers.Email
{
    public class MailgunProvider : INotificationProvider
    {
        private readonly MailgunConfiguration _config;
        private readonly HttpClient _httpClient;
        private readonly ILogger<MailgunProvider> _logger;
        public NotificationChannelType SupportedChannel => NotificationChannelType.Email;

        public MailgunProvider(IOptions<MailgunConfiguration> config, IHttpClientFactory factory, ILogger<MailgunProvider> logger)
        {
            _config = config.Value;
            _httpClient = factory.CreateClient("Mailgun");
            _logger = logger;
        }

        public async Task<bool> SendAsync(Recipient recipient, string subject, string body, Dictionary<string, object>? metadata = null)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, $"https://api.mailgun.net/v3/{_config.Domain}/messages");
                var auth = Convert.ToBase64String(Encoding.ASCII.GetBytes($"api:{_config.ApiKey}"));
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", auth);

                var content = new MultipartFormDataContent
                {
                    { new StringContent($"{_config.SenderName} <{_config.SenderEmail}>"), "from" },
                    { new StringContent(recipient.Value), "to" },
                    { new StringContent(subject), "subject" },
                    { new StringContent(body), "html" }
                };
                request.Content = content;

                var response = await _httpClient.SendAsync(request);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Mailgun Failed to {Email}", recipient.Value);
                return false;
            }
        }
    }
}