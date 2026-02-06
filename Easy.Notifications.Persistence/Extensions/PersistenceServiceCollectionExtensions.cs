using Easy.Notifications.Core.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using Easy.Notifications.EntityFrameworkCore;
using Easy.Notifications.EntityFrameworkCore.Implementations;




#if !NETFRAMEWORK
using Microsoft.EntityFrameworkCore;
#else
using System.Data.Entity;
#endif

namespace Easy.Notifications.EntityFrameworkCore.Extensions
{
    /// <summary>
    /// Extension methods for registering persistence services for Easy.Notifications.
    /// Supports both Entity Framework Core and Entity Framework 6.
    /// </summary>
    public static class PersistenceServiceCollectionExtensions
    {

#if !NETFRAMEWORK
        /// <summary>
        /// Registers the Entity Framework based notification store and database context.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="options">The DbContext options builder for EF Core.</param>
        /// <returns>The updated service collection.</returns>
        public static IServiceCollection AddNotificationPersistence(
            this IServiceCollection services,
            Action<DbContextOptionsBuilder> options)
        {
            // 1. Register the DbContext with provided options (Scoped by default)
            services.AddDbContext<NotificationDbContext>(options);

            // 2. Register the EF Store implementation as INotificationStore
            // Transient or Scoped is preferred for DbContext-dependent services
            services.TryAddScoped<INotificationStore, EfNotificationStore>();

            // Raporlama servisini de ekliyoruz
            // 3. Register the Report Service implementation as INotificationReportService
            services.TryAddScoped<INotificationReportService, EfNotificationReportService>();

            return services;
        }
#else
        /// <summary>
        /// Registers the Entity Framework based notification store and database context.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="connectionString">The connection string or name for EF6.</param>
        /// <returns>The updated service collection.</returns>
        public static IServiceCollection AddNotificationPersistence(
            this IServiceCollection services, 
            string connectionString)
        {
            // 1. Register the DbContext for EF6
            // We register it as a factory-based scoped service to pass the connection string
            services.AddScoped(sp => new NotificationDbContext(connectionString));

            // 2. Register the EF Store implementation
            services.TryAddScoped<INotificationStore, EfNotificationStore>();

             // 3. Register the Report Service implementation as INotificationReportService
            services.TryAddScoped<INotificationReportService, EfNotificationReportService>();

            return services;
        }
#endif
    }
}