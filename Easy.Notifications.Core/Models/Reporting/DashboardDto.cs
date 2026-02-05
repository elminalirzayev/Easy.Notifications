using System;
using System.Collections.Generic;

namespace Easy.Notifications.Core.Models.Reporting
{
    /// <summary>
    /// Aggregated summary for the dashboard.
    /// </summary>
    public class DashboardSummaryDto
    {
        public int TotalRequests { get; set; }
        public int Successful { get; set; }
        public int Failed { get; set; }
        public int Pending { get; set; }
        public int Cancelled { get; set; }
        public List<ChannelStatsDto> ChannelBreakdown { get; set; } = new();
    }

    /// <summary>
    /// Stats per channel (e.g., Email: 150 sent).
    /// </summary>
    public class ChannelStatsDto
    {
        public string Channel { get; set; } = string.Empty;
        public int Count { get; set; }
        public double SuccessRate { get; set; }
    }

    /// <summary>
    /// Daily trend data for charts.
    /// </summary>
    public class DailyTrendDto
    {
        public DateTime Date { get; set; }
        public int SuccessCount { get; set; }
        public int FailCount { get; set; }
    }
}