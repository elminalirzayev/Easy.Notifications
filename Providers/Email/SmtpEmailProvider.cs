using Easy.Notifications.Core.Abstractions;
using Easy.Notifications.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace Easy.Notifications.Providers.Email
{
    public class SmtpEmailProvider : INotificationProvider
    {
        private readonly EmailConfiguration _config;
        private readonly ILogger<SmtpEmailProvider> _logger;
        public NotificationChannelType SupportedChannel => NotificationChannelType.Email;

        public SmtpEmailProvider(IOptions<EmailConfiguration> config, ILogger<SmtpEmailProvider> logger)
        {
            _config = config.Value;
            _logger = logger;
        }

        public async Task<bool> SendAsync(Recipient recipient, string subject, string body, Dictionary<string, object>? metadata = null)
        {
            try
            {
                using var client = new SmtpClient(_config.Host, _config.Port)
                {
                    Credentials = new NetworkCredential(_config.Username, _config.Password),
                    EnableSsl = _config.EnableSsl
                };

                var mail = new MailMessage
                {
                    From = new MailAddress(_config.Sender, _config.SenderDisplayName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };
                mail.To.Add(recipient.Value);

                await client.SendMailAsync(mail);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SMTP Send Failed to {Email}", recipient.Value);
                return false;
            }
        }
    }
}