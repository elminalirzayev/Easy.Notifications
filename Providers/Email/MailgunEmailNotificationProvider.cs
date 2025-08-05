using Easy.Notifications.Core.Enums;
using Easy.Notifications.Core.Interfaces;
using Easy.Notifications.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;

namespace Easy.Notifications.Providers.Email
{
    public class MailgunEmailNotificationProvider : INotificationProvider
    {
        private readonly EmailConfiguration _settings;
        private readonly ILogger<MailgunEmailNotificationProvider> _logger;
        private readonly HttpClient _httpClient;
        public ChannelType Channel => ChannelType.Email;
        public string Name => "Mailgun";


        public MailgunEmailNotificationProvider(IOptions<EmailConfiguration> options, HttpClient httpClient, ILogger<MailgunEmailNotificationProvider> logger)
        {
            _settings = options.Value;
            _logger = logger;
            _httpClient = httpClient;
        }

        public async Task SendAsync(NotificationMessage message)
        {


            var base64Auth = Convert.ToBase64String(Encoding.ASCII.GetBytes($"api:{_settings.ApiKey}"));


            // Set default values on the HttpClient
            _httpClient.BaseAddress = new Uri($"https://api.mailgun.net/v3/{_settings.Domain}/messages");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64Auth);



            foreach (var toEmail in message.Recipients)
            {

                using MultipartFormDataContent form = new();


                // Local function keeps this code a bit clearer
                void SetFormParam(string key, string value) =>
                form.Add(new StringContent(value, Encoding.UTF8, MediaTypeNames.Text.Plain), key);

                SetFormParam("from", $"Test User <postmaster@{_settings.Domain}>");
                SetFormParam("to", toEmail);
                SetFormParam("subject", message.Title);
                //  SetFormParam("text", "");
                SetFormParam("html", message.Body);

                var result = await this._httpClient.PostAsync(string.Empty, form);

                if (!result.IsSuccessStatusCode)
                {
                    _logger.LogError("Mailgun: Email could not be sent -> {To}", toEmail);
                }
                else
                {
                    _logger.LogInformation("Mailgun: Email sent -> {To}", toEmail);
                }

            }

        }
    }
}