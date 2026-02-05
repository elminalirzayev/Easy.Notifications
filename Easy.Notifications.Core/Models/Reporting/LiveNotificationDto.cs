namespace Easy.Notifications.Core.Models.Reporting
{
    /// <summary>
    /// Data sent to the UI via SignalR for real-time monitoring.
    /// </summary>
    public class LiveNotificationDto
    {
        public Guid Id { get; set; }
        public string Recipient { get; set; } = string.Empty;
        public string Channel { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string? GroupId { get; set; } = string.Empty;
    }
}