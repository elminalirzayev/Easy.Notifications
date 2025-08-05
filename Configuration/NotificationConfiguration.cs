using Easy.Notifications.Providers.Email;
using Easy.Notifications.Providers.Sms;

namespace Easy.Notifications.Configuration
{
    public class NotificationConfiguration
    {
        public NotificationOptions NotificationOptions { get; set; }
        public NotificationProviders NotificationProviders { get; set; }
        public EmailConfiguration EmailConfiguration { get; set; }
        public SmsConfiguration SmsConfiguration { get; set; }
    }
}
