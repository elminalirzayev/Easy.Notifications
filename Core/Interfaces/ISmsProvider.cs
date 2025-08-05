namespace Easy.Notifications.Core.Interfaces
{
    public interface ISmsProvider : INotificationProvider
    {
        string Name { get; }
    }
}
