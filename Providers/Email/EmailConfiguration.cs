namespace Easy.Notifications.Providers.Email
{
    public class EmailConfiguration
    {
        public string ApiKey { get; set; } = string.Empty;
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 0;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Sender { get; set; } = string.Empty;
        public string SenderDisplayName { get; set; } = string.Empty;
        public string Domain { get; set; } = string.Empty;
        public bool? EnableSsl { get; set; } = true;
    }
}
