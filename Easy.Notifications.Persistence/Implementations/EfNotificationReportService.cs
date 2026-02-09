using Easy.Notifications.Core.Abstractions;
using Easy.Notifications.Core.Models.Reporting;

#if NETFRAMEWORK
using System.Data.Entity;
#else
using Microsoft.EntityFrameworkCore;
#endif

namespace Easy.Notifications.Persistence.EntityFramework.Implementations
{
    /// <summary>
    /// Provides reporting and statistics data from the notification database.
    /// </summary>
    public class EfNotificationReportService : INotificationReportService
    {
        private readonly NotificationDbContext _context;
        private const int MaxRetryThreshold = 3; // "Failed"

        public EfNotificationReportService(NotificationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<DashboardSummaryDto> GetSummaryAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
        {
            var query = _context.NotificationLogs
                .AsNoTracking()
                .Where(x => x.CreatedAt >= startDate && x.CreatedAt <= endDate);

            var aggregateStats = await query
                .GroupBy(x => 1)
                .Select(g => new
                {
                    Total = g.Count(),
                    Success = g.Count(x => x.IsSuccess),
                    Cancelled = g.Count(x => x.IsCancelled),
                    Failed = g.Count(x => !x.IsSuccess && !x.IsCancelled && x.RetryCount >= MaxRetryThreshold)
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (aggregateStats == null)
            {
                return new DashboardSummaryDto();
            }

            var pending = aggregateStats.Total - (aggregateStats.Success + aggregateStats.Failed + aggregateStats.Cancelled);

            var channelData = await query
                .Where(x => x.ChannelLookup != null)
                .GroupBy(x => x.ChannelLookup!.Name)
                .Select(g => new
                {
                    Channel = g.Key ?? "Unknown",
                    Count = g.Count(),
                    SuccessCount = g.Count(x => x.IsSuccess)
                })
                .ToListAsync(cancellationToken);

            var channelStats = channelData.Select(x => new ChannelStatsDto
            {
                Channel = x.Channel,
                Count = x.Count,
                SuccessRate = x.Count > 0 ? (double)x.SuccessCount / x.Count * 100 : 0
            }).ToList();

            return new DashboardSummaryDto
            {
                TotalRequests = aggregateStats.Total,
                Successful = aggregateStats.Success,
                Failed = aggregateStats.Failed,
                Cancelled = aggregateStats.Cancelled,
                Pending = pending > 0 ? pending : 0,
                ChannelBreakdown = channelStats
            };
        }

        public async Task<IEnumerable<DailyTrendDto>> GetDailyTrendsAsync(int lastDays = 7, CancellationToken cancellationToken = default)
        {
            var startDate = DateTime.UtcNow.Date.AddDays(-lastDays);

            var data = await _context.NotificationLogs
                .AsNoTracking()
                .Where(x => x.CreatedAt >= startDate)
                .GroupBy(x => x.CreatedAt.Date)
                .Select(g => new DailyTrendDto
                {
                    Date = g.Key,
                    SuccessCount = g.Count(x => x.IsSuccess),
                    FailCount = g.Count(x => !x.IsSuccess && !x.IsCancelled)
                })
                .OrderBy(x => x.Date)
                .ToListAsync();

            return data;
        }

        public async Task<DashboardSummaryDto> GetGroupStatsAsync(string groupId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(groupId))
                return new DashboardSummaryDto();

            var query = _context.NotificationLogs
                .AsNoTracking()
                .Where(x => x.GroupId == groupId);

            var stats = await query
                .GroupBy(x => 1)
                .Select(g => new
                {
                    Total = g.Count(),
                    Success = g.Count(x => x.IsSuccess),
                    Failed = g.Count(x => !x.IsSuccess && !x.IsCancelled),
                    Cancelled = g.Count(x => x.IsCancelled)
                })
                .FirstOrDefaultAsync();

            if (stats == null) return new DashboardSummaryDto();

            return new DashboardSummaryDto
            {
                TotalRequests = stats.Total,
                Successful = stats.Success,
                Failed = stats.Failed,
                Cancelled = stats.Cancelled,
                Pending = stats.Total - (stats.Success + stats.Failed + stats.Cancelled)
            };
        }
    }
}