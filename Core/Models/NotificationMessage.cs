namespace Easy.Notifications.Core.Models
{
    public class NotificationMessage
    {
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public List<string> Recipients { get; set; } = new();
        public Dictionary<string, object>? Metadata { get; set; }
    }
}
