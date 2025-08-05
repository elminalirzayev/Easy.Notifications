using Easy.Notifications.Core.Enums;
using Easy.Notifications.Core.Interfaces;
using Easy.Notifications.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Easy.Notifications.Providers.Email
{
    public class SendgridEmailNotificationProvider : INotificationProvider
    {
        private readonly EmailConfiguration _settings;
        private readonly ILogger<SendgridEmailNotificationProvider> _logger;
        private readonly ISendGridClient _sendGridClient;

        public ChannelType Channel => ChannelType.Email;
        public string Name => "Sendgrid";


        public SendgridEmailNotificationProvider(IOptions<EmailConfiguration> options, ISendGridClient sendGridClient, ILogger<SendgridEmailNotificationProvider> logger)
        {
            _settings = options.Value;
            _logger = logger;
            _sendGridClient = sendGridClient;
        }

        public async Task SendAsync(NotificationMessage message)
        {
            var from = new EmailAddress(_settings.Sender, _settings.SenderDisplayName);
            var subject = message.Title;
            var htmlContent = message.Body;

            foreach (var toEmail in message.Recipients)
            {
                var to = new EmailAddress(toEmail);
                var sendGridMessage = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent: null, htmlContent);

                try
                {
                    var response = await _sendGridClient.SendEmailAsync(sendGridMessage);

                    if ((int)response.StatusCode >= 200 && (int)response.StatusCode < 300)
                    {
                        _logger.LogInformation("SendGrid: E-posta gönderildi -> {To}", toEmail);
                    }
                    else
                    {
                        _logger.LogWarning("SendGrid: Gönderim başarısız ({StatusCode}) -> {To}", response.StatusCode, toEmail);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "SendGrid: E-posta gönderilemedi -> {To}", toEmail);
                }
            }
        }
    }
}