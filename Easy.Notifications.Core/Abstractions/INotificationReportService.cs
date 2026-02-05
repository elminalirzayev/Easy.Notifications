using Easy.Notifications.Core.Models.Reporting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Easy.Notifications.Core.Abstractions
{
    public interface INotificationReportService
    {
        /// <summary>
        /// Gets general statistics for a specific date range.
        /// </summary>
        Task<DashboardSummaryDto> GetSummaryAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Gets the last N days of sending trends for charts.
        /// </summary>
        Task<List<DailyTrendDto>> GetDailyTrendsAsync(int lastDays = 7);

        /// <summary>
        /// Gets detailed status of a specific group (Campaign).
        /// </summary>
        Task<DashboardSummaryDto> GetGroupStatsAsync(string groupId);
    }
}