using Easy.Notifications.Core.Enums;
using Easy.Notifications.Core.Interfaces;
using Easy.Notifications.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace Easy.Notifications.Providers.Email
{
    public class SmtpEmailNotificationProvider : INotificationProvider
    {
        private readonly EmailConfiguration _settings;
        private readonly ILogger<SmtpEmailNotificationProvider> _logger;

        public ChannelType Channel => ChannelType.Email;
        public string Name => "Smtp";


        public SmtpEmailNotificationProvider(IOptions<EmailConfiguration> options, ILogger<SmtpEmailNotificationProvider> logger)
        {
            _settings = options.Value;
            _logger = logger;
        }

        public async Task SendAsync(NotificationMessage message)
        {
            using var smtp = new SmtpClient(_settings.Host, _settings.Port)
            {
                EnableSsl = _settings.EnableSsl,
                Credentials = new NetworkCredential(_settings.Username, _settings.Password)
            };

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
                    await smtp.SendMailAsync(mail);
                    _logger.LogInformation("Email gönderildi: {To}", to);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Email gönderilemedi: {To}", to);
                }
            }
        }
    }
}
