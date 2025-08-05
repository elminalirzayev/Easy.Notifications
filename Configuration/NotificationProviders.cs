using Easy.Notifications.Core.Enums;

namespace Easy.Notifications.Configuration
{
    public class NotificationProviders
    {
        public EmailProviderType EmailProvider { get; set; }
        public SmsProviderType SmsProvider { get; set; }
    }
}
