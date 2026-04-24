using Feed.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace Feed.API.Data
{
    public class FeedDbContext : DbContext
    {
        public FeedDbContext(DbContextOptions<FeedDbContext> options)
            : base(options) { }

        public DbSet<FeedItemEntity> FeedItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Composite index on UserId + CreatedAt
            // For efficient feed queries ordered by time
            modelBuilder.Entity<FeedItemEntity>()
                .HasIndex(f => new { f.UserId, f.CreatedAt });

            // Default values
            modelBuilder.Entity<FeedItemEntity>()
                .Property(f => f.IsSeen)
                .HasDefaultValue(false);
        }
    }
}