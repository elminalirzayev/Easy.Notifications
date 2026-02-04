[![Build & Test](https://github.com/elminalirzayev/Easy.Notifications/actions/workflows/build.yml/badge.svg)](https://github.com/elminalirzayev/Easy.Notifications/actions/workflows/build.yml)
[![Build & Release](https://github.com/elminalirzayev/Easy.Notifications/actions/workflows/release.yml/badge.svg)](https://github.com/elminalirzayev/Easy.Notifications/actions/workflows/release.yml)
[![Build & Nuget Publish](https://github.com/elminalirzayev/Easy.Notifications/actions/workflows/nuget.yml/badge.svg)](https://github.com/elminalirzayev/Easy.Notifications/actions/workflows/nuget.yml)
[![Release](https://img.shields.io/github/v/release/elminalirzayev/Easy.Notifications)](https://github.com/elminalirzayev/Easy.Notifications/releases)
[![License](https://img.shields.io/github/license/elminalirzayev/Easy.Notifications)](https://github.com/elminalirzayev/Easy.Notifications/blob/master/LICENSE.txt)
[![NuGet](https://img.shields.io/nuget/v/Easy.Notifications.svg)](https://www.nuget.org/packages/Easy.Notifications)



#  Easy.Notifications

**Easy.Notifications** is a high-performance, asynchronous, and channel-agnostic notification library for .NET 6+.

It is designed for **Enterprise** applications that need to send notifications (Email, SMS, Chat, Push) without blocking the main execution thread. It leverages **System.Threading.Channels** to implement a robust "Fire-and-Forget" architecture.


## 🚀 Features

-   ** Asynchronous & Non-Blocking:** Uses high-performance in-memory queues to dispatch messages instantly.
    
-   ** Multi-Channel Support:** Unified API for:
    
    -   **Email:** SMTP, SendGrid, Mailgun.
        
    -   **SMS / WhatsApp:** Twilio, Vonage (Nexmo).
        
    -   **Chat:** Slack (Block Kit), Microsoft Teams (Message Cards), Telegram.
        
    -   **Realtime:** SignalR (WebSockets).
        
-   ** Rich Content Support:** - Automatically converts messages to **Slack Block Kit** structures.
    
    -   Renders **Microsoft Teams Message Cards** with custom colors and sections.
        
-   ** Modular Architecture:** Add only the providers you need via Dependency Injection.
    
-   ** Built-in Templating:** Lightweight template engine for dynamic content (`Hello {{Name}}`).
    

##  Architecture

The library separates the **Dispatching** logic from the **Processing** logic to ensure maximum throughput.

1.  **Producer:** Your Controller calls `INotificationService.SendAsync()`.
    
2.  **Dispatcher:** The payload is instantly written to a memory channel. Control returns to your code in microseconds.
    
3.  **Worker:** A background service (`BackgroundNotificationWorker`) reads from the channel.
    
4.  **Provider:** The specific provider (e.g., `SlackProvider`, `SmtpEmailProvider`) executes the external API call safely.
    


## 📦 Installation

Install via NuGet Package Manager:

```bash
Install-Package Easy.Notifications
```

Or via .NET CLI:

```bash
dotnet add package Easy.Notifications
```


##  Configuration

### 1. appsettings.json

Configure only the providers you intend to use.

```json
{
  "NotificationConfiguration": {
    "EmailConfiguration": {
      "Host": "smtp.example.com",
      "Port": 587,
      "Username": "apikey",
      "Password": "secret-password",
      "Sender": "no-reply@example.com",
      "SenderDisplayName": "System Alerts",
      "EnableSsl": true
    },
    "TwilioConfiguration": {
      "AccountSid": "ACxxxxxxxx...",
      "AuthToken": "xxxxxxxx...",
      "FromNumber": "+15551234567"
    },
    "SendGridConfiguration": {
      "ApiKey": "SG.xxxxxxxx...",
      "SenderEmail": "alert@example.com",
      "SenderName": "Alert Bot"
    },
    "VonageConfiguration": {
      "ApiKey": "a1b2c3d4",
      "ApiSecret": "xxxxxxxxxxxx",
      "Sender": "MyBrand",
      "FromNumber": "MyBrand"
    },
    "MailgunConfiguration": {
      "ApiKey": "<MAILGUN_API_KEY>",
      "Domain": "<MAILGUN_DOMAIN>",
      "SenderEmail": "noreply@example.com",
      "SenderName": "Easy Notifications"
    },
    "TelegramConfiguration": {
      "BotToken": "123456:ABC-DEF..."
    }
  }
}
```

### 2. Service Registration (Program.cs)

Register the core services and the specific providers you need.

```csharp
using Easy.Notifications.Extensions;

var builder = WebApplication.CreateBuilder(args);

// 1. Add Core Services (Dispatcher, Worker)
builder.Services.AddEasyNotifications();

// 2. Add Desired Providers (Modular Registration)
// Email
builder.Services.AddSmtpEmail(builder.Configuration);
// builder.Services.AddSendGrid(builder.Configuration);
// builder.Services.AddMailgun(builder.Configuration);

// SMS & WhatsApp
builder.Services.AddTwilio(builder.Configuration);
// builder.Services.AddVonage(builder.Configuration);

// Chat Apps (Slack, Teams, Telegram)
builder.Services.AddChatProviders(builder.Configuration);

// Realtime
builder.Services.AddSignalRNotifications();


var app = builder.Build();

// 3. Map SignalR Hub (If using Realtime)
app.MapHub<NotificationHub>("/notifications");

app.Run();
```

## Usage

Inject `INotificationService` into your controllers or business services.

### Example 1: Sending Rich Slack & Teams Messages

Send structured messages with colors, headers, and metadata without dealing with complex JSON payloads.

```csharp
public class AlertController : ControllerBase
{
    private readonly INotificationService _notifier;

    public AlertController(INotificationService notifier)
    {
        _notifier = notifier;
    }

    [HttpPost("alert")]
    public async Task<IActionResult> SendAlert()
    {
        var slackUrl = "https://hooks.slack.com/services/T000/B000/XXXX";
        var teamsUrl = "https://your-teams-webhook-url";

        var payload = new NotificationPayload
        {
            Subject = "High CPU Usage Warning 🚨",
            Body = "Server **Prod-01** CPU usage is at *98%*. Please investigate immediately.",
            
            Recipients = new List<Recipient>
            {
                Recipient.Chat(slackUrl, NotificationChannelType.Slack),
                Recipient.Chat(teamsUrl, NotificationChannelType.Teams)
            },

            // Metadata creates "Context" in Slack and "Fact Section" in Teams
            Metadata = new Dictionary<string, object>
            {
                { "ThemeColor", "FF0000" }, // Red card for Teams
                { "Server", "Linux-Prod-01" },
                { "Region", "US-East-1" },
                { "Time", DateTime.Now.ToString("HH:mm") }
            }
        };

        // Fire-and-Forget
        await _notifier.SendAsync(payload);

        return Ok("Alert queued.");
    }
}
```

### Example 2: Multi-Channel Broadcast (Email + SMS)

```csharp
var payload = new NotificationPayload
{
    Subject = "Welcome {{Name}}",
    Body = "Hi {{Name}}, your account is activated.",
    TemplateData = new Dictionary<string, string> { { "Name", "John Doe" } },
    
    Recipients = new List<Recipient>
    {
        Recipient.Email("john@example.com", "John Doe"),
        Recipient.Sms("+15550001234"),
        Recipient.WhatsApp("+15550001234")
    }
};

await _notifier.SendAsync(payload);
```

### Example 3: Real-Time Web Notification (SignalR)

```csharp
var payload = new NotificationPayload
{
    Subject = "Export Complete",
    Body = "Your data export is ready to download.",
    Recipients = new List<Recipient>
    {
        Recipient.Client("user-123-id") // Targets a specific user on frontend
    }
};

await _notifier.SendAsync(payload);
```

##  Supported Providers

| Channel      | Provider Class           | NuGet Dependency | Notes |
| ---          | ---                      | ---      | --- |
| **Email**    | `SmtpEmailProvider`      | Built-in | Standard .NET `SmtpClient` |
| **Email**    | `SendGridProvider`       | `SendGrid` | Uses SendGrid Web API |
| **Email**    | `MailgunProvider`        | `Microsoft.Extensions.Http` | Uses Mailgun API v3 |
| **SMS**      | `TwilioSmsProvider`      | `Twilio` | Standard SMS |
| **SMS**      | `VonageSmsProvider`      | Built-in | Uses Nexmo/Vonage REST API |
| **WhatsApp** | `TwilioWhatsAppProvider` | `Twilio` | Requires WhatsApp Sandbox/Number |
| **WhatsApp** | `VonageWhatsAppProvider` | Built-in | Requires WhatsApp Sandbox/Number |
| **Slack**    | `SlackProvider`          | Built-in | Uses Incoming Webhooks (Block Kit) |
| **Teams**    | `TeamsProvider`          | Built-in | Uses Incoming Webhooks (MessageCard) |
| **Telegram** | `TelegramProvider`       | Built-in | Uses Telegram Bot API |
| **Realtime** | `SignalRProvider`        | `Microsoft.AspNetCore.SignalR.Core` | Pushes to connected clients |


##  Extending

You can easily add your own provider (e.g., Discord, Firebase FCM) by implementing `INotificationProvider`.

1.  Create a class implementing `INotificationProvider`.
    
2.  Define the `SupportedChannel`.
    
3.  Implement `SendAsync`.
    
4.  Register it in DI: `services.TryAddEnumerable(ServiceDescriptor.Singleton<INotificationProvider, MyCustomProvider>());`
    

___

## Contributing

Contributions and suggestions are welcome. Please open an issue or submit a pull request.

---

## Contact

For questions, contact us via elmin.alirzayev@gmail.com or GitHub.

---

## License

This project is licensed under the MIT License.

---

© 2025 Elmin Alirzayev / Easy Code Tools



