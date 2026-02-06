using Easy.Notifications.Persistence.Entities;
using System.Collections.Generic;
using System.Reflection.Emit;

<<<<<<< TODO: Unmerged change from project 'Easy.Notifications.EntityFrameworkCore (net6.0)', Before:
=======
using Easy;
using Easy.Notifications;
using Easy.Notifications.Persistence;
using Easy.Notifications.EntityFrameworkCore;
>>>>>>> After



#if !NETFRAMEWORK
using Microsoft.EntityFrameworkCore;
#else
using System.Data.Entity;
#endif
namespace Easy.Notifications.EntityFrameworkCore
{
    /// <summary>
    /// Multi-framework database context for storing notification history.
    /// Supports both EF Core (Modern .NET) and EF6 (.NET Framework).
    /// </summary>
    public class NotificationDbContext : DbContext
    {
#if !NETFRAMEWORK
        /// <summary>
        /// Constructor for EF Core.
        /// </summary>
        /// <param name="options">The options to be used by the DbContext.</param>
        public NotificationDbContext(DbContextOptions<NotificationDbContext> options) : base(options) { }
#else 
        /// <summary>
        /// Constructor for EF6.
        /// </summary>
        /// <param name="nameOrConnectionString">The connection string name.</param>
        public NotificationDbContext(string nameOrConnectionString) : base(nameOrConnectionString) { }
#endif

        /// <summary>
        /// Gets or sets the notification logs table.
        /// </summary>
        public DbSet<NotificationLog> NotificationLogs { get; set; } = null!;

        /// <summary>
        /// Configures the database schema and model mappings for both EF versions.
        /// </summary>
#if !NETFRAMEWORK
        protected override void OnModelCreating(ModelBuilder modelBuilder)
#else
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
#endif
        {
#if !NETFRAMEWORK
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<NotificationLog>(entity =>
            {
                entity.ToTable("NotificationLogs");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Recipient).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Channel).HasConversion<string>().HasMaxLength(50);
                entity.HasIndex(e => e.CreatedAt);
            });
#else 
            // EF6 Configuration
            modelBuilder.Entity<NotificationLog>()
                .ToTable("NotificationLogs")
                .HasKey(e => e.Id);

            modelBuilder.Entity<NotificationLog>()
                .Property(e => e.Recipient)
                .IsRequired()
                .HasMaxLength(255);

            // Note: EF6 doesn't have HasConversion<string> like EF Core. 
            // The enum will be stored as an INT by default in EF6.
            
            base.OnModelCreating(modelBuilder);
#endif
        }
    }
}