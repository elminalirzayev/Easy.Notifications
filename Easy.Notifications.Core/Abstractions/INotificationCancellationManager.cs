using System.Threading.Tasks;
using System;

namespace Easy.Notifications.Core.Abstractions
{
    /// <summary>
    /// Manages cancellation requests for notification groups using a hybrid approach (Memory + Persistence).
    /// </summary>
    public interface INotificationCancellationManager
    {
        /// <summary>
        /// Cancels a group. Updates the in-memory cache for fast lookups and the database for consistency.
        /// </summary>
        /// <param name="groupId">The group identifier to cancel.</param>
        /// <param name="duration">How long the cancellation should remain active in memory.</param>
        Task CancelGroupAsync(string groupId, TimeSpan duration);

        /// <summary>
        /// Checks if a group is cancelled by looking up the in-memory cache.
        /// </summary>
        /// <param name="groupId">The group identifier.</param>
        /// <returns>True if the group is cancelled; otherwise false.</returns>
        bool IsGroupCancelled(string? groupId);
    }
}