[![Build & Test](https://github.com/elminalirzayev/Easy.Notifications/actions/workflows/build.yml/badge.svg)](https://github.com/elminalirzayev/Easy.Notifications/actions/workflows/build.yml)
[![Build & Release](https://github.com/elminalirzayev/Easy.Notifications/actions/workflows/release.yml/badge.svg)](https://github.com/elminalirzayev/Easy.Notifications/actions/workflows/release.yml)
[![Build & Nuget Publish](https://github.com/elminalirzayev/Easy.Notifications/actions/workflows/nuget.yml/badge.svg)](https://github.com/elminalirzayev/Easy.Notifications/actions/workflows/nuget.yml)
[![Release](https://img.shields.io/github/v/release/elminalirzayev/Easy.Notifications)](https://github.com/elminalirzayev/Easy.Notifications/releases)
[![License](https://img.shields.io/github/license/elminalirzayev/Easy.Notifications)](https://github.com/elminalirzayev/Easy.Notifications/blob/master/LICENSE.txt)
[![NuGet](https://img.shields.io/nuget/v/Easy.Notifications.svg)](https://www.nuget.org/packages/Easy.Notifications)

# Easy.Notifications

**Easy.Notifications** is a modular and extensible .NET library for sending notifications through multiple channels. It provides a unified interface to dispatch messages via Email, SMS, WhatsApp, Telegram, Slack, Microsoft Teams, and more.

## Features

- 🔌 Plug-and-play support for:
  - SMTP, SendGrid, Mailgun
  - Twilio, Vonage (SMS & WhatsApp)
  - Telegram Bots
  - Slack (Block Kit)
  - Microsoft Teams (MessageCard)
- 📦 Simple service registration via `AddEasyNotifications()`
- 🧩 Extensible provider model
- 🔒 Configuration-based setup
- ☁️ SignalR support (optional)

## Installation

1. Install the NuGet package (TBD):

```
dotnet add package Easy.Notifications
```

2. Add configuration section in `appsettings.json`:

```json
{
  "NotificationConfiguration": {
    "NotificationOptions": {
      "UseEmail": true,
      "UseSms": true,
      "UseWhatsApp": true,
      "UseTelegram": true,
      "UseSlack": true,
      "UseTeams": true
    },
    "NotificationProviders": {
      "EmailProvider": "Sendgrid", // Options: "Smtp", "Sendgrid", "Mailgun"
      "SmsProvider": "Twilio" // Options: "Twilio", "Vonage" (ex nexmo)
    },
    "EmailConfiguration": {
      // SMTP
      "Host": "smtp.yourdomain.com",
      "Port": 587,
      "Username": "your-smtp-username",
      "Password": "your-smtp-password",
      "Sender": "noreply@yourdomain.com",
      "SenderDisplayName": "Your App Name"
      "EnableSsl": true,
    }
    /*
    // SendGrid
    "EmailConfiguration": {
      "ApiKey": "<SENDGRID_API_KEY>",
      "Sender": "noreply@example.com",
      "SenderDisplayName": "Easy Notifications"
    },
    */
    // Mailgun
    "EmailConfiguration": {
      "ApiKey": "<MAILGUN_API_KEY>",
      "Domain": "<MAILGUN_DOMAIN>",
      "Sender": "noreply@example.com",
      "SenderDisplayName": "Easy Notifications"
    },
    */
    "SmsConfiguration": {
    // Twilio
      "Username": "<TWILIO_SID>",
      "Password": "<TWILIO_TOKEN>",
      "Sender": "+1234567890"
    },
    /*
    // Vonage (Nexmo)
    "SmsConfiguration": {
      "Username": "<ACCOUNT_SID>",
      "Password": "<AUTH_TOKEN>",
      "Sender": "+1234567890"
    */
    "WhatsAppConfiguration": {
      "AccountSid": "<ACCOUNT_SID>",
      "AuthToken": "<AUTH_TOKEN>",
      "Sender": "whatsapp:+1234567890"
    },
    "TelegramConfiguration": {
      "BotToken": "<BOT_TOKEN>",
      "ChatId":   "<CHAT_ID>" 
    },
    "SlackConfiguration": {
      "WebhookUrl": "<SLACK_WEBHOOK_URL>"
    },
    "TeamsConfiguration": {
      "WebhookUrl": "<TEAMS_WEBHOOK_URL>"
    }
  }
}
```

3. Register the service in `Startup.cs` or `Program.cs`:

```csharp
services.AddEasyNotifications(Configuration);
```

4. Inject and use `INotificationService`:

```csharp
public class NotificationController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpPost("notify")]
    public async Task<IActionResult> Send(NotificationMessage message)
    {
        await _notificationService.SendAsync(message);
        return Ok();
    }
}
```

## SignalR Integration

If using SignalR:

```csharp
app.UseEndpoints(endpoints =>
{
    endpoints.MapHub<NotificationHub>("/notificationHub");
});
```

## License

MIT License.

---

© Elmin Alirzayev 2025 / Easy Code Tools
