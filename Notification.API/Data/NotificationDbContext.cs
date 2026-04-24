using Microsoft.EntityFrameworkCore;
using Notification.API.Entities;

namespace Notification.API.Data
{
    public class NotificationDbContext : DbContext
    {
        public NotificationDbContext(
            DbContextOptions<NotificationDbContext> options)
            : base(options) { }

        public DbSet<NotificationEntity> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Index on RecipientId + IsRead
            // For efficient unread badge count queries
            modelBuilder.Entity<NotificationEntity>()
                .HasIndex(n => new { n.RecipientId, n.IsRead });

            // Default values
            modelBuilder.Entity<NotificationEntity>()
                .Property(n => n.IsRead)
                .HasDefaultValue(false);
        }
    }
}