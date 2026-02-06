using Easy.Notifications.Core.Abstractions;
using Easy.Notifications.Core.Models.Reporting;

namespace Easy.Notifications.Infrastructure.Services
{
    public class NoOpLiveMonitor : INotificationLiveMonitor
    {
        public Task PublishUpdateAsync(LiveNotificationDto log)
        {
            // Do nothing. Monitoring is disabled.
            return Task.CompletedTask;
        }
    }
}