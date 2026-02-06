using Easy.Notifications.Core.Models.Reporting;

namespace Easy.Notifications.Core.Abstractions
{
    /// <summary>
    /// Defines the contract for retrieving analytics and reporting data 
    /// from the persistence store.
    /// </summary>
    public interface INotificationReportService
    {
        /// <summary>
        /// Retrieves general statistics (Total, Success, Failed, Pending) for a specific date range.
        /// </summary>
        /// <param name="startDate">The start date of the reporting period (UTC).</param>
        /// <param name="endDate">The end date of the reporting period (UTC).</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task containing the <see cref="DashboardSummaryDto"/> with aggregated counts.</returns>
        Task<DashboardSummaryDto> GetSummaryAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves the daily sending trends (Success vs Failure) for the charts.
        /// </summary>
        /// <param name="lastDays">The number of days to look back. Default is 7.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task containing a list of daily trend data points.</returns>
        Task<IEnumerable<DailyTrendDto>> GetDailyTrendsAsync(int lastDays = 7, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves detailed statistics for a specific notification group (e.g., a Marketing Campaign).
        /// </summary>
        /// <param name="groupId">The unique identifier of the group/campaign.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task containing the summary statistics for the specified group.</returns>
        Task<DashboardSummaryDto> GetGroupStatsAsync(string groupId, CancellationToken cancellationToken = default);
    }
}