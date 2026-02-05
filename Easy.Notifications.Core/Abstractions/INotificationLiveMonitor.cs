using Easy.Notifications.Core.Models.Reporting;
using System.Threading.Tasks;

namespace Easy.Notifications.Core.Abstractions
{
    /// <summary>
    /// Interface for publishing real-time updates.
    /// The consumer application will implement this using SignalR.
    /// </summary>
    public interface INotificationLiveMonitor
    {
        Task PublishUpdateAsync(LiveNotificationDto log);
    }
}