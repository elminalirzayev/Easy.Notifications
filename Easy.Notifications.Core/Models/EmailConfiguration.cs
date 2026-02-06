namespace Easy.Notifications.Core.Models
{
    public class EmailConfiguration
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 587;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Sender { get; set; } = string.Empty;
        public string SenderDisplayName { get; set; } = string.Empty;
        public bool EnableSsl { get; set; } = true;
    }
}
