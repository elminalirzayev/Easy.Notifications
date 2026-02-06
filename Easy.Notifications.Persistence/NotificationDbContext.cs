using Easy.Notifications.Persistence.EntityFramework.Entities;

#if !NETFRAMEWORK
using Microsoft.EntityFrameworkCore;
#else
using System.Data.Entity;
#endif

namespace Easy.Notifications.Persistence.EntityFramework
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
        public NotificationDbContext(DbContextOptions<NotificationDbContext> options) : base(options)
        {
        }
#else 
        /// <summary>
        /// Constructor for EF6.
        /// </summary>
        /// <param name="nameOrConnectionString">The connection string or name.</param>
        public NotificationDbContext(string nameOrConnectionString) : base(nameOrConnectionString) 
        { 
            // Optional: Disable initializer if you manage migrations manually
            // Database.SetInitializer<NotificationDbContext>(null);
        }
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
            // --- EF CORE CONFIGURATION ---
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<NotificationLog>(entity =>
            {
                entity.ToTable("NotificationLogs");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Recipient)
                      .IsRequired()
                      .HasMaxLength(255);

                entity.Property(e => e.Channel).IsRequired();
                entity.Property(e => e.Priority).IsRequired();

                entity.HasIndex(e => e.CreatedAt);
                entity.HasIndex(e => e.GroupId);
            });
#else 
            // --- EF6 CONFIGURATION ---
            base.OnModelCreating(modelBuilder);

            var entity = modelBuilder.Entity<NotificationLog>();
            
            entity.ToTable("NotificationLogs");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Recipient)
                  .IsRequired()
                  .HasMaxLength(255);

            entity.Property(e => e.Channel).IsRequired();
            entity.Property(e => e.Priority).IsRequired();
            
#endif
        }
    }
}