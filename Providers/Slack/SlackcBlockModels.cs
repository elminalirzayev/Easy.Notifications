namespace Easy.Notifications.Providers.Slack
{
    public class SlackBlock
    {
        public string type { get; set; } = string.Empty;
        public SlackText text { get; set; } = new SlackText();
        public List<SlackElement> elements { get; set; } = new List<SlackElement>();
    }

    public class SlackText
    {
        public string type { get; set; } = string.Empty;
        public string text { get; set; } = string.Empty;
    }

    public class SlackElement
    {
        public string type { get; set; } = string.Empty;
        public SlackText text { get; set; } = new SlackText();
        public string url { get; set; } = string.Empty;
    }
}
