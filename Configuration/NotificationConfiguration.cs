using Easy.Notifications.Providers.Email;
using Easy.Notifications.Providers.Slack;
using Easy.Notifications.Providers.Sms;
using Easy.Notifications.Providers.Teams;
using Easy.Notifications.Providers.Telegram;

namespace Easy.Notifications.Configuration
{
    public class NotificationConfiguration
    {
        public NotificationOptions? NotificationOptions { get; set; }
        public NotificationProviders? NotificationProviders { get; set; }
        public EmailConfiguration? EmailConfiguration { get; set; }
        public SmsConfiguration? SmsConfiguration { get; set; }
        public SlackConfiguration? SlackConfiguration { get; set; }
        public TeamsConfiguration? TeamsConfiguration { get; set; }
        public TelegramConfiguration? TelegramConfiguration { get; set; }
    }
}
