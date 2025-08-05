using Easy.Notifications.Core.Enums;
using Easy.Notifications.Core.Interfaces;
using Easy.Notifications.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Easy.Notifications.Providers.Email
{
    /// <summary>
    /// Notification provider for sending emails using SendGrid.
    /// </summary>
    public class SendgridEmailNotificationProvider : INotificationProvider
    {
        private readonly EmailConfiguration _settings;
        private readonly ILogger<SendgridEmailNotificationProvider> _logger;
        private readonly ISendGridClient _sendGridClient;

        /// <summary>
        /// Gets the channel type for this provider (Email).
        /// </summary>
        public ChannelType Channel => ChannelType.Email;

        /// <summary>
        /// Gets the name of the provider.
        /// </summary>
        public string Name => "Sendgrid";

        /// <summary>
        /// Initializes a new instance of the <see cref="SendgridEmailNotificationProvider"/> class.
        /// </summary>
        /// <param name="options">Email configuration options.</param>
        /// <param name="sendGridClient">SendGrid client instance.</param>
        /// <param name="logger">Logger instance.</param>
        public SendgridEmailNotificationProvider(IOptions<EmailConfiguration> options, ISendGridClient sendGridClient, ILogger<SendgridEmailNotificationProvider> logger)
        {
            _settings = options.Value;
            _logger = logger;
            _sendGridClient = sendGridClient;
        }

        /// <summary>
        /// Sends a notification email using SendGrid.
        /// </summary>
        /// <param name="message">The notification message to send.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task SendAsync(NotificationMessage message)
        {
            // Prepare the sender and subject
            var from = new EmailAddress(_settings.Sender, _settings.SenderDisplayName);
            var subject = message.Title;
            var htmlContent = message.Body;

            // Send the email to each recipient
            foreach (var toEmail in message.Recipients)
            {
                var to = new EmailAddress(toEmail);
                var sendGridMessage = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent: null, htmlContent);

                try
                {
                    // Send the email using SendGrid
                    var response = await _sendGridClient.SendEmailAsync(sendGridMessage);

                    // Log the result
                    if ((int)response.StatusCode >= 200 && (int)response.StatusCode < 300)
                    {
                        _logger.LogInformation("SendGrid: Email sent -> {To}", toEmail);
                    }
                    else
                    {
                        _logger.LogWarning("SendGrid: Sending failed ({StatusCode}) -> {To}", response.StatusCode, toEmail);
                    }
                }
                catch (Exception ex)
                {
                    // Log any exception that occurs during sending
                    _logger.LogError(ex, "SendGrid: Email could not be sent -> {To}", toEmail);
                }
            }
        }
    }
}