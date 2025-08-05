using Easy.Notifications.Core.Enums;
using Easy.Notifications.Core.Models;

namespace Easy.Notifications.Core.Interfaces
{
    public interface INotificationProvider
    {
        Task SendAsync(NotificationMessage message);
        ChannelType Channel { get; }
    }
}
