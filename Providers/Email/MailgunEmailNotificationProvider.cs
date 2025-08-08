using Easy.Notifications.Core.Enums;
using Easy.Notifications.Core.Interfaces;
using Easy.Notifications.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;

namespace Easy.Notifications.Providers.Email
{
    /// <summary>
    /// Notification provider for sending emails using Mailgun.
    /// </summary>
    public class MailgunEmailNotificationProvider : INotificationProvider
    {
        private readonly EmailConfiguration _settings;
        private readonly ILogger<MailgunEmailNotificationProvider> _logger;
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Gets the channel type for this provider (Email).
        /// </summary>
        public ChannelType Channel => ChannelType.Email;

        /// <summary>
        /// Gets the name of the provider.
        /// </summary>
        public string Name => "Mailgun";

        /// <summary>
        /// Initializes a new instance of the <see cref="MailgunEmailNotificationProvider"/> class.
        /// </summary>
        /// <param name="options">Email configuration options.</param>
        /// <param name="httpClient">HTTP client for sending requests.</param>
        /// <param name="logger">Logger instance.</param>
        public MailgunEmailNotificationProvider(IOptions<EmailConfiguration> options, HttpClient httpClient, ILogger<MailgunEmailNotificationProvider> logger)
        {
            _settings = options.Value;
            _logger = logger;
            _httpClient = httpClient;
        }

        /// <summary>
        /// Sends a notification email using Mailgun.
        /// </summary>
        /// <param name="message">The notification message to send.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task SendAsync(NotificationMessage message)
        {
            // Prepare the basic authentication header for Mailgun API
            var base64Auth = Convert.ToBase64String(Encoding.ASCII.GetBytes($"api:{_settings.ApiKey}"));

            // Set default values on the HttpClient for Mailgun
            _httpClient.BaseAddress = new Uri($"https://api.mailgun.net/v3/{_settings.Domain}/messages");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64Auth);

            // Send the email to each recipient
            foreach (var toEmail in message.Recipients)
            {
                using MultipartFormDataContent form = new();

                // Helper function to add form parameters
                void SetFormParam(string key, string value) =>
                    form.Add(new StringContent(value, Encoding.UTF8, MediaTypeNames.Text.Plain), key);

                SetFormParam("from", $"Test User <postmaster@{_settings.Domain}>");
                SetFormParam("to", toEmail);
                SetFormParam("subject", message.Title);
                // SetFormParam("text", ""); // Uncomment to add plain text content
                SetFormParam("html", message.Body);

                // Send the POST request to Mailgun
                var result = await this._httpClient.PostAsync(string.Empty, form);

                // Log the result
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