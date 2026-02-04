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

    public class SendGridConfiguration
    {
        public string ApiKey { get; set; } = string.Empty;
        public string SenderEmail { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;
    }

    public class MailgunConfiguration
    {
        public string ApiKey { get; set; } = string.Empty;
        public string Domain { get; set; } = string.Empty;
        public string SenderEmail { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;
    }

    public class TwilioConfiguration
    {
        public string AccountSid { get; set; } = string.Empty;
        public string AuthToken { get; set; } = string.Empty;
        public string FromNumber { get; set; } = string.Empty;
    }

    public class VonageConfiguration
    {
        public string ApiKey { get; set; } = string.Empty;
        public string ApiSecret { get; set; } = string.Empty;
        public string Sender { get; set; } = string.Empty;
        public string FromNumber { get; set; } = string.Empty;
    }

    public class TelegramConfiguration
    {
        public string BotToken { get; set; } = string.Empty;
    }
}