[![Build & Test](https://github.com/elminalirzayev/Easy.Notifications.Persistence/actions/workflows/build.yml/badge.svg)](https://github.com/elminalirzayev/Easy.Notifications.Persistence/actions/workflows/build.yml)
[![Build & Release](https://github.com/elminalirzayev/Easy.Notifications.Persistence/actions/workflows/release.yml/badge.svg)](https://github.com/elminalirzayev/Easy.Notifications.Persistence/actions/workflows/release.yml)
[![Build & Nuget Publish](https://github.com/elminalirzayev/Easy.Notifications.Persistence/actions/workflows/nuget.yml/badge.svg)](https://github.com/elminalirzayev/Easy.Notifications.Persistence/actions/workflows/nuget.yml)
[![Release](https://img.shields.io/github/v/release/elminalirzayev/Easy.Notifications.Persistence)](https://github.com/elminalirzayev/Easy.Notifications.Persistence/releases)
[![License](https://img.shields.io/github/license/elminalirzayev/Easy.Notifications.Persistence)](https://github.com/elminalirzayev/Easy.Notifications.Persistence/blob/master/LICENSE.txt)
[![NuGet](https://img.shields.io/nuget/v/Easy.Notifications.Persistence.svg)](https://www.nuget.org/packages/Easy.Notifications.Persistence)

# Easy.Notifications.Persistence.EntityFramework

**The universal persistence layer for the Easy Notification System.**

This package implements the storage logic for `Easy.Notifications`. It is designed with **Multi-Targeting** support, making it capable of running on both modern **.NET 8** applications (using EF Core) and legacy **.NET Framework** projects (using EF 6).

By installing this package, you enable **audit logging**, **status tracking**, and **retry mechanisms** for your notifications.

## Installation

```bash
Install-Package Easy.Notifications.Persistence.EntityFramework
```

Or via .NET CLI:

```bash
dotnet add package Easy.Notifications.Persistence.EntityFramework
```

## Compatibility Matrix

This package automatically adapts its internal implementation based on your project's target framework:

| Your Project Target | Uses Internally | Dependency |
| --- | --- | --- |
| **.NET 6.0+** (Core) | Entity Framework Core | `Microsoft.EntityFrameworkCore.SqlServer` |
| **.NET Framework 4.7+** | Entity Framework 6 | `EntityFramework` (Classic) |


## Configuration

### 1. For .NET 6+ / .NET Core (EF Core)

In your `Program.cs`, use the extension method to register the storage.

```csharp
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Add the Main Engine
builder.Services.AddEasyNotifications();

// 2. Add Persistence (EF Core)
var connectionString = builder.Configuration.GetConnectionString("NotificationDb");

builder.Services.AddNotificationPersistence(options =>
{
    // Use SQL Server (or any other EF Core provider)
    options.UseSqlServer(connectionString);
});
```

### 2. For .NET Framework 4.7+ (EF 6)

In legacy applications (e.g., ASP.NET MVC 5), you typically register dependencies using a container like Unity, Autofac, or Ninject.


```csharp
// Example using a generic DI container
var connectionString = ConfigurationManager.ConnectionStrings["NotificationDb"].ConnectionString;

// Register the Store Implementation
container.RegisterType<INotificationStore, EfNotificationStore>(
    new ConstructorArgument("connectionString", connectionString)
);
```

## Database Setup

The package contains the `NotificationLogs` entity. You need to create this table in your database.

### Option A: Auto-Generate (Development)

For .NET Core apps, you can ensure the database is created at startup:

```csharp
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
    context.Database.EnsureCreated();
}
```

### Option B: Migrations (Production)

Since the `DbContext` is embedded within this library, you can generate migrations from your host application:

```csharp
dotnet ef migrations add InitialCreate --context NotificationDbContext
dotnet ef database update --context NotificationDbContext
```

_(For EF6 .NET Framework projects, use the standard `Enable-Migrations` and `Update-Database` commands in Package Manager Console)._

## Features

-   **Unified History:** Logs every email, SMS, and chat message sent via the system.
    
-   **Retry Support:** Allows the background worker to fetch failed messages from the database and retry them.
    
-   **Group Cancellation:** Persists cancellation requests to ensure stopped campaigns remain stopped even after a server restart.
    
-   **Dashboard Ready:** Provides the data source for the `Easy.Notifications` dashboard analytics.
    

## The Ecosystem

| Package | Description |
| --- | --- |
| **`Easy.Notifications.Core`** | Abstractions, Interfaces, and Models. |
| **`Easy.Notifications`** | The main engine (Worker, Dispatcher, Providers). |
| **`Easy.Notifications.Persistence.EntityFramework`** | **(You are here)** Storage implementation for EF Core & EF 6. |


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
