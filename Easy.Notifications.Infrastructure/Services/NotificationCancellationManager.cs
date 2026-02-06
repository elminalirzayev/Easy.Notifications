using Easy.Notifications.Core.Abstractions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace Easy.Notifications.Infrastructure.Services
{
    /// <summary>
    /// Implements hybrid cancellation logic using IMemoryCache for reads and INotificationStore for writes.
    /// </summary>
    public class NotificationCancellationManager : INotificationCancellationManager
    {
        private readonly IMemoryCache _cache;
        private readonly IServiceProvider _serviceProvider;

        public NotificationCancellationManager(IMemoryCache cache, IServiceProvider serviceProvider)
        {
            _cache = cache;
            _serviceProvider = serviceProvider;
        }

        public async Task CancelGroupAsync(string groupId, TimeSpan duration)
        {
            if (string.IsNullOrWhiteSpace(groupId)) return;

            // 1. Write to RAM (Fast Block)
            _cache.Set($"Cancel_Group_{groupId}", true, duration);

            // 2. Write to DB (Consistency) - Create a scope because Manager is Singleton but Store is Scoped
            using var scope = _serviceProvider.CreateScope();
            var store = scope.ServiceProvider.GetService<INotificationStore>();

            if (store != null)
            {
                await store.CancelGroupAsync(groupId);
            }
        }

        public bool IsGroupCancelled(string? groupId)
        {
            if (string.IsNullOrWhiteSpace(groupId)) return false;

            // Only check RAM for performance
            return _cache.TryGetValue($"Cancel_Group_{groupId}", out _);
        }
    }
}