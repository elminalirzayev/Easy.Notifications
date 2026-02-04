using Easy.Notifications.Core.Abstractions;
using Easy.Notifications.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Easy.Notifications.Providers.Email
{
    public class SendGridProvider : INotificationProvider
    {
        private readonly SendGridConfiguration _config;
        private readonly ISendGridClient _client;
        private readonly ILogger<SendGridProvider> _logger;
        public NotificationChannelType SupportedChannel => NotificationChannelType.Email;

        public SendGridProvider(IOptions<SendGridConfiguration> config, ISendGridClient client, ILogger<SendGridProvider> logger)
        {
            _config = config.Value;
            _client = client;
            _logger = logger;
        }

        public async Task<bool> SendAsync(Recipient recipient, string subject, string body, Dictionary<string, object>? metadata = null)
        {
            try
            {
                var from = new EmailAddress(_config.SenderEmail, _config.SenderName);
                var to = new EmailAddress(recipient.Value, recipient.DisplayName);
                var msg = MailHelper.CreateSingleEmail(from, to, subject, null, body);
                var response = await _client.SendEmailAsync(msg);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SendGrid Failed to {Email}", recipient.Value);
                return false;
            }
        }
    }
}