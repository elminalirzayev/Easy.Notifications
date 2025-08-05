using Easy.Notifications.Core.Enums;
using Easy.Notifications.Core.Models;

namespace Easy.Notifications.Core.Interfaces
{
    public interface INotificationService
    {
        Task SendToAsync(NotificationMessage message, params ChannelType[] channels);
        Task SendToAllAsync(NotificationMessage message);
    }
}
