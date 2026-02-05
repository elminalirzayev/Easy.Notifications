using Easy.Notifications.Core.Abstractions;
using Easy.Notifications.Core.Models.Reporting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#if NETFRAMEWORK
using System.Data.Entity;
#else
using Microsoft.EntityFrameworkCore;
#endif
namespace Easy.Notifications.Persistence.Implementations
{
    public class EfNotificationReportService : INotificationReportService
    {
        private readonly NotificationDbContext _context;

        public EfNotificationReportService(NotificationDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardSummaryDto> GetSummaryAsync(DateTime startDate, DateTime endDate)
        {
            // Performans için sadece ilgili tarih aralığını ve tracking olmadan çekiyoruz
            var query = _context.NotificationLogs
                .AsNoTracking()
                .Where(x => x.CreatedAt >= startDate && x.CreatedAt <= endDate);

            var total = await query.CountAsync();
            var success = await query.CountAsync(x => x.IsSuccess);
            var cancelled = await query.CountAsync(x => x.IsCancelled);
            // Başarısız ama iptal edilmemiş olanlar
            var failed = await query.CountAsync(x => !x.IsSuccess && !x.IsCancelled && x.RetryCount >= 3);
            // Hala denenenler veya kuyrukta olanlar (Basit mantık)
            var pending = total - (success + failed + cancelled);

            // Kanal Bazlı Kırılım
            var channels = await query
                .GroupBy(x => x.Channel)
                .Select(g => new ChannelStatsDto
                {
                    Channel = g.Key.ToString(),
                    Count = g.Count(),
                    SuccessRate = g.Count() > 0 ? (double)g.Count(x => x.IsSuccess) / g.Count() * 100 : 0
                })
                .ToListAsync();

            return new DashboardSummaryDto
            {
                TotalRequests = total,
                Successful = success,
                Failed = failed,
                Cancelled = cancelled,
                Pending = pending > 0 ? pending : 0,
                ChannelBreakdown = channels
            };
        }

        public async Task<List<DailyTrendDto>> GetDailyTrendsAsync(int lastDays = 7)
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

        public async Task<DashboardSummaryDto> GetGroupStatsAsync(string groupId)
        {
            if (string.IsNullOrWhiteSpace(groupId)) return new DashboardSummaryDto();

            // Aynı mantığı sadece GroupId filtresiyle çalıştırıyoruz
            var query = _context.NotificationLogs.AsNoTracking().Where(x => x.GroupId == groupId);

            // (Yukarıdaki GetSummaryAsync mantığının aynısı buraya uygulanabilir veya private bir helper metoda çekilebilir)
            // Örnek kısaltma:
            var total = await query.CountAsync();
            var success = await query.CountAsync(x => x.IsSuccess);

            return new DashboardSummaryDto
            {
                TotalRequests = total,
                Successful = success,
                Failed = await query.CountAsync(x => !x.IsSuccess && !x.IsCancelled),
                Cancelled = await query.CountAsync(x => x.IsCancelled)
            };
        }
    }
}