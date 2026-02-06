[![Build & Test](https://github.com/elminalirzayev/Easy.Notifications/actions/workflows/build.yml/badge.svg)](https://github.com/elminalirzayev/Easy.Notifications/actions/workflows/build.yml)
[![Build & Release](https://github.com/elminalirzayev/Easy.Notifications/actions/workflows/release.yml/badge.svg)](https://github.com/elminalirzayev/Easy.Notifications/actions/workflows/release.yml)
[![Build & Nuget Publish](https://github.com/elminalirzayev/Easy.Notifications/actions/workflows/nuget.yml/badge.svg)](https://github.com/elminalirzayev/Easy.Notifications/actions/workflows/nuget.yml)
[![Release](https://img.shields.io/github/v/release/elminalirzayev/Easy.Notifications)](https://github.com/elminalirzayev/Easy.Notifications/releases)
[![License](https://img.shields.io/github/license/elminalirzayev/Easy.Notifications)](https://github.com/elminalirzayev/Easy.Notifications/blob/master/LICENSE.txt)
[![NuGet](https://img.shields.io/nuget/v/Easy.Notifications.svg)](https://www.nuget.org/packages/Easy.Notifications)


# Easy.Notifications.Infrastructure

**The core execution engine and provider implementations for the Easy Notification System.**

This package contains the "Heavy Lifting" logic of the system. It houses the background processing workers, the priority-based dispatching logic, and the concrete implementations for various notification channels (Email, SMS, Chat).

## Key Components

-   **`BackgroundNotificationWorker`**: A hosted service that manages the lifecycle of a notification from the internal queue to the final provider.
    
-   **`NotificationDispatcher`**: The traffic controller that routes incoming payloads into the correct `System.Threading.Channels` based on priority.
    
-   **`ProviderFactory`**: Dynamically resolves the correct `INotificationProvider` based on the recipient's channel type.
    
-   **`CancellationManager`**: Manages the hybrid (In-memory + Store) cancellation logic to stop pending messages.
    

## Installation

Install via NuGet Package Manager:

```bash
Install-Package Easy.Notifications.Infrastructure
```

Or via .NET CLI:

```bash
dotnet add package Easy.Notifications.Infrastructure
```

_Note: This package requires `Easy.Notifications.Core` for abstractions and models._

## Internal Workflow

1.  **Ingestion:** The infrastructure layer receives a `NotificationPayload` through the `INotificationService`.
    
2.  **Queueing:** Messages are pushed into one of three internal channels: `Urgent`, `High`, or `Normal`.
    
3.  **Consumption:** The `BackgroundNotificationWorker` monitors these channels with a "Priority First" strategy.
    
4.  **Resilience:** If a provider fails, the infrastructure layer interacts with the `INotificationStore` (if registered) to schedule a retry.
    

## Provider Support

This package includes built-in infrastructure for:

-   **Email**: `SmtpEmailProvider`
    
-   **SMS**: `TwilioSmsProvider`, `VonageSmsProvider`
    
-   **Chat**: `TeamsProvider` (MessageCards), `SlackProvider` (Block Kit), `TelegramProvider` (Bot API)
    
-   **Real-Time**: `SignalRNotificationMonitor` hooks
    

## Advanced Configuration

You can customize the worker's behavior via DI:

```csharp
builder.Services.AddEasyNotifications(options => 
{
    // Configure internal channel capacities to prevent memory overflow
    options.ChannelCapacity = 10000;
    
    // Set global retry limits
    options.MaxRetryAttempts = 3;
});
```

## The Ecosystem

| Package | Role |
| --- | --- |
| **`Easy.Notifications.Core`** | Interfaces and POCO Models (Dependency of Infrastructure). |
| **`Easy.Notifications.Infrastructure`** | **(This package)** Background processing and Provider logic. |
| **`Easy.Notifications.Persistence.EntityFramework`** | Database storage for logs and retry state. |

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