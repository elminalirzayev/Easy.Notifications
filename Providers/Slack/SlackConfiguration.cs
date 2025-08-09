namespace Easy.Notifications.Providers.Slack
{
    public class SlackConfiguration
    {
        public string WebhookUrl { get; set; } = string.Empty;
        public string Channel { get; set; } = string.Empty; // Optional
    }

}
