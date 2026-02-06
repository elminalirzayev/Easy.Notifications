[![Build & Test](https://github.com/elminalirzayev/Easy.Notifications/actions/workflows/build.yml/badge.svg)](https://github.com/elminalirzayev/Easy.Notifications/actions/workflows/build.yml)
[![Build & Release](https://github.com/elminalirzayev/Easy.Notifications/actions/workflows/release.yml/badge.svg)](https://github.com/elminalirzayev/Easy.Notifications/actions/workflows/release.yml)
[![Build & Nuget Publish](https://github.com/elminalirzayev/Easy.Notifications/actions/workflows/nuget.yml/badge.svg)](https://github.com/elminalirzayev/Easy.Notifications/actions/workflows/nuget.yml)
[![Release](https://img.shields.io/github/v/release/elminalirzayev/Easy.Notifications)](https://github.com/elminalirzayev/Easy.Notifications/releases)
[![License](https://img.shields.io/github/license/elminalirzayev/Easy.Notifications)](https://github.com/elminalirzayev/Easy.Notifications/blob/master/LICENSE.txt)
[![NuGet](https://img.shields.io/nuget/v/Easy.Notifications.svg)](https://www.nuget.org/packages/Easy.Notifications)


# Easy.Notifications

**Easy.Notifications** is a robust, high-performance, and channel-agnostic notification engine for .NET 6+.

It is designed for enterprise applications that require **Reliable Dispatching** without blocking the main execution thread. It combines **System.Threading.Channels**, **Priority Queues**, and **Hybrid Persistence** to ensure your messages (Email, SMS, Chat) are delivered safely, even under heavy load.

## 🚀 Features

-   ** Fire-and-Forget Architecture:** Uses in-memory channels to offload sending logic instantly.
-   ** Priority Queues:** Process `Urgent` messages (e.g., OTPs, Alerts) before `Normal` newsletters.
-   ** Resilience & Retries:** Automatic retry mechanism with exponential backoff for failed providers.
-   ** Hybrid Cancellation:** Cancel millions of pending campaign messages instantly using a Memory+DB hybrid lock.
-   ** Live Monitoring:** Real-time hooks for SignalR to watch notification traffic as it happens.
-   ** Audit Logging:** (Optional) Persist every attempt, success, and failure to SQL Server using the persistence package.
-   ** Built-in Templating:** Lightweight template engine for dynamic content (`Hello {{Name}}`).
-   ** Modular Architecture:** Add only the providers you need via Dependency Injection.
-   ** Multi-Channel Support:**
    -   **Email:** SMTP, SendGrid, Mailgun.
    -   **SMS/WhatsApp:** Twilio, Vonage.
    -   **Chat:** Slack (Block Kit), Teams (Adaptive Cards), Telegram.
    -   **Realtime:** SignalR (WebSockets).
-   ** Rich Content Support:**
    -   Automatically converts messages to **Slack Block Kit** structures.
    -   Renders **Microsoft Teams Message Cards** with custom colors and sections.   
      
##  Architecture

The library separates the **Dispatching** logic from the **Processing** logic to ensure maximum throughput.

1.  **Producer:** Your Controller calls `INotificationService.SendAsync()`.
    
2.  **Dispatcher:** The payload is instantly written to a memory channel. Control returns to your code in microseconds.
    
3.  **Worker:** A background service (`BackgroundNotificationWorker`) reads from the channel.
    
4.  **Provider:** The specific provider (e.g., `SlackProvider`, `SmtpEmailProvider`) executes the external API call safely.
  

## Installation

Install via NuGet Package Manager:

```bash
Install-Package Easy.Notifications
```

Or via .NET CLI:

```bash
dotnet add package Easy.Notifications
```

_(Optional) If you need database logging and retry persistence:_

```bash
dotnet add package Easy.Notifications.Persistence.EntityFramework
```

## Configuration

### 1. Register Services (Program.cs)

Use the fluent API to configure exactly what you need.


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

// 3. SMS & WhatsApp
builder.Services.AddTwilio(builder.Configuration);
// builder.Services.AddVonage(builder.Configuration);

// 4. Chat Apps (Slack, Teams, Telegram)
builder.Services.AddChatProviders(builder.Configuration);

// 5. (Optional) Add Persistence for Logs & Retries
var connString = builder.Configuration.GetConnectionString("NotificationDb");
builder.Services.AddNotificationPersistence(options => options.UseSqlServer(connString));

// 6. Realtime
builder.Services.AddSignalRNotifications();

// 7. (Optional) Add Real-Time Monitoring
builder.Services.AddSingleton<INotificationLiveMonitor, SignalRNotificationMonitor>();

var app = builder.Build();

// Map SignalR Hub (If using Realtime)
app.MapHub<NotificationHub>("/notifications");

app.Run();

```

### 2. Configure Settings (appsettings.json)

```json
{
  "NotificationConfiguration": {
    "RetryConfiguration": {
      "MaxRetryCount": 5,
      "IntervalInMinutes": 10
    },
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


## Usage

Inject `INotificationService` into your controllers.

### 1. Sending an Urgent Alert (Priority Queue)

Urgent messages jump to the front of the line.

```csharp
[HttpPost("send-otp")]
public async Task<IActionResult> SendOtp()
{
    var payload = new NotificationPayload
    {
        Subject = "Login Code",
        Body = "Your code is: **123456**",
        Priority = NotificationPriority.Urgent, // Processed immediately
        Recipients = new List<Recipient> 
        { 
            Recipient.Sms("+15550001234"),
            Recipient.WhatsApp("+15550001234")
        }
    };

    await _notifier.SendAsync(payload);
    return Ok();
}
```

### 2. Sending Rich Chat Messages (Context Aware)

Send styled messages to Slack and Teams using the same payload.

```csharp
var payload = new NotificationPayload
{
    Subject = "⚠️ Server High Load",
    Body = "Server **PROD-01** is at 99% CPU.",
    Metadata = new Dictionary<string, object>
    {
        { "ThemeColor", "FF0000" }, // Red card for Teams
        { "Server", "Prod-01" },    // Field for Slack
        { "Region", "US-East" }
    },
    Recipients = new List<Recipient>
    {
        Recipient.Teams("https://outlook.office.com/webhook/..."),
        Recipient.Slack("https://hooks.slack.com/services/...")
    }
};

await _notifier.SendAsync(payload);
```

### 3. Campaign Management (Grouping & Cancellation)

You can group notifications (e.g., a newsletter) and cancel them instantly if you realize there is a typo.

**Sending:**

```csharp
var payload = new NotificationPayload
{
    GroupId = "Newsletter-Feb-2026", // Link messages to a group
    Subject = "Monthly Update",
    Recipients = // list of 10,000 users...
};
await _notifier.SendAsync(payload);
```

**Cancelling:**

```csharp
public class AdminController : ControllerBase
{
    private readonly INotificationCancellationManager _cancellationManager;

    [HttpPost("cancel-campaign")]
    public async Task<IActionResult> Cancel(string campaignId)
    {
        // Instantly stops processing this group in memory and DB
        await _cancellationManager.CancelGroupAsync(campaignId, TimeSpan.FromHours(24));
        return Ok("Campaign stopped.");
    }
}
```

### 4. Multi-Channel Broadcast (Email + SMS)

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

### 5. Real-Time Web Notification (SignalR)

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

## Live Dashboard & Monitoring

The library supports real-time monitoring via SignalR. When enabled, the worker broadcasts every event (Success/Failure) to your frontend.

1.  Implement `INotificationLiveMonitor` in your Web API.
    
2.  Register it as a Singleton.
    
3.  Connect your React/Angular frontend to the SignalR Hub.
    

_(See the `samples` folder for a full dashboard implementation)_

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


## The Ecosystem

| Package | Description |
| --- | --- |
| **`Easy.Notifications.Core`** | Abstractions, Interfaces, and Models. |
| **`Easy.Notifications`** | **(You are here)** The main engine and default providers. |
| **`Easy.Notifications.Persistence.EntityFramework`** | Persistence layer for logging, status tracking, and retry mechanisms. |

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