using Easy.Notifications.Core.Enums;
using Easy.Notifications.Core.Interfaces;
using Easy.Notifications.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace Easy.Notifications.Providers.Email
{
    /// <summary>
    /// Notification provider for sending emails using SMTP.
    /// </summary>
    public class SmtpEmailNotificationProvider : INotificationProvider
    {
        private readonly EmailConfiguration _settings;
        private readonly ILogger<SmtpEmailNotificationProvider> _logger;

        /// <summary>
        /// Gets the channel type for this provider (Email).
        /// </summary>
        public ChannelType Channel => ChannelType.Email;

        /// <summary>
        /// Gets the name of the provider.
        /// </summary>
        public string Name => "Smtp";

        /// <summary>
        /// Initializes a new instance of the <see cref="SmtpEmailNotificationProvider"/> class.
        /// </summary>
        /// <param name="options">Email configuration options.</param>
        /// <param name="logger">Logger instance.</param>
        public SmtpEmailNotificationProvider(IOptions<EmailConfiguration> options, ILogger<SmtpEmailNotificationProvider> logger)
        {
            _settings = options.Value;
            _logger = logger;
        }

        /// <summary>
        /// Sends a notification email using SMTP.
        /// </summary>
        /// <param name="message">The notification message to send.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task SendAsync(NotificationMessage message)
        {
            // Create and configure the SMTP client
            using var smtp = new SmtpClient(_settings.Host, _settings.Port)
            {
                EnableSsl = _settings.EnableSsl ?? false,
                Credentials = new NetworkCredential(_settings.Username, _settings.Password)
            };

            // Send the email to each recipient
            foreach (var to in message.Recipients)
            {
                var mail = new MailMessage
                {
                    From = new MailAddress(_settings.Sender),
                    Subject = message.Title,
                    Body = message.Body,
                    IsBodyHtml = true
                };

                mail.To.Add(to);

                try
                {
                    // Send the email using SMTP
                    await smtp.SendMailAsync(mail);
                    _logger.LogInformation("Email sent: {To}", to);
                }
                catch (Exception ex)
                {
                    // Log any exception that occurs during sending
                    _logger.LogError(ex, "Email could not be sent: {To}", to);
                }
            }
        }
    }
}
