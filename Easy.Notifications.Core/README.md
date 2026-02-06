[![Build & Test](https://github.com/elminalirzayev/Easy.Notifications/actions/workflows/build.yml/badge.svg)](https://github.com/elminalirzayev/Easy.Notifications/actions/workflows/build.yml)
[![Build & Release](https://github.com/elminalirzayev/Easy.Notifications/actions/workflows/release.yml/badge.svg)](https://github.com/elminalirzayev/Easy.Notifications/actions/workflows/release.yml)
[![Build & Nuget Publish](https://github.com/elminalirzayev/Easy.Notifications/actions/workflows/nuget.yml/badge.svg)](https://github.com/elminalirzayev/Easy.Notifications/actions/workflows/nuget.yml)
[![Release](https://img.shields.io/github/v/release/elminalirzayev/Easy.Notifications)](https://github.com/elminalirzayev/Easy.Notifications/releases)
[![License](https://img.shields.io/github/license/elminalirzayev/Easy.Notifications)](https://github.com/elminalirzayev/Easy.Notifications/blob/master/LICENSE.txt)
[![NuGet](https://img.shields.io/nuget/v/Easy.Notifications.svg)](https://www.nuget.org/packages/Easy.Notifications)


# Easy.Notifications.Core

**The abstraction layer and shared models for the Easy Notification System.**

This package contains the core interfaces (`INotificationService`, `INotificationProvider`), domain models (`NotificationPayload`, `Recipient`), and enums required to build or consume the notification system.

> **Note:** This package **does not** contain the implementation logic (Background Worker) or database context. For the full functionality, please install **`Easy.Notifications`**.

## Installation

Install via NuGet Package Manager:

```bash
Install-Package Easy.Notifications.Core
```

Or via .NET CLI:

```bash
dotnet add package Easy.Notifications.Core
```

## Key Features

-   **Unified Payload:** A single `NotificationPayload` model to rule them all (Email, SMS, Chat). 
-   **Rich Recipient Model:** Helper methods for creating Email, SMS, Teams, Slack, and Telegram recipients. 
-   **Prioritization:** Built-in `NotificationPriority` enum (Urgent, High, Normal, Low).
-   **Templating Support:** Dictionary-based template data structure.
-   **Metadata Support:** Pass custom data (e.g., color codes for Teams cards) via Metadata.
-   **Clean Architecture:** Pure C# POCOs and Interfaces. Zero external dependencies.
    

### Supported Channels (Enum)
The `NotificationChannelType` enum currently supports:
* `Email` (SMTP, SendGrid, Mailgun)
* `Sms` (Twilio, Vonage)
* `Teams` (Webhook)
* `Slack` (Webhook)
* `Telegram` (Bot API)
* `WhatsApp` (Twilio/Vonage)


## Usage

This package is primarily used to construct the **Notification Payload** that will be sent to the dispatcher.

### 1. Creating a Notification Payload

```csharp
using Easy.Notifications.Core.Models;

var payload = new NotificationPayload
{
    // Basic Info
    Subject = "Welcome Onboard!",
    Body = "Hi {{Name}}, your account is ready.",
    Priority = NotificationPriority.High,
    
    // Grouping (for Batch Cancellation)
    GroupId = "Campaign-2024-Feb",

    // Dynamic Data for Templates
    TemplateData = new Dictionary<string, string>
    {
        { "Name", "John Doe" }
    },

    // Recipients (Multi-Channel)
    Recipients = new List<Recipient>
    {
        Recipient.Email("john.doe@example.com", "John Doe"),
        Recipient.Sms("+1234567890"),
        Recipient.Teams("https://outlook.office.com/webhook/...") // Webhook URL
    },

    // Channel-Specific Metadata
    Metadata = new Dictionary<string, object>
    {
        { "ThemeColor", "FF0000" } // Red card for Teams
    }
};
```

### 2. Implementing Custom Providers

If you want to build a custom provider (e.g., for Discord or Push Notifications), you only need to reference this Core package and implement the `INotificationProvider` interface.

```csharp
using Easy.Notifications.Core.Abstractions;
using Easy.Notifications.Core.Models;

public class MyCustomProvider : INotificationProvider
{
    public NotificationChannelType SupportedChannel => NotificationChannelType.MobilePush; // Assume you added this enum

    public Task<bool> SendAsync(Recipient recipient, string subject, string body, Dictionary<string, object>? metadata)
    {
        // Your custom sending logic here...
        return Task.FromResult(true);
    }
}
```

> **Developer Note:** In your application, you will inject `INotificationService` (provided by the main `Easy.Notifications` package) and use it to dispatch the payload you create here.


## The Ecosystem

This package is part of a modular notification system:

| Package | Description |
| --- | --- |
| **`Easy.Notifications.Core`** | **(You are here)** Abstractions, Interfaces, and Models. |
| **`Easy.Notifications`** | The main engine. Includes Background Worker, Retry logic, Dispatcher, and default providers (SMTP, Twilio, etc.). |
| **`Easy.Notifications.EntityFrameworkCore`** | Persistence layer for logging, status tracking, and retry mechanisms using EF Core. |
| **`Easy.Notifications.EntityFrameworkCore`** | Persistence layer for logging, status tracking, and retry mechanisms using EF Core. |

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
