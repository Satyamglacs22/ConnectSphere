using Follow.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace Follow.API.Data
{
    public class FollowDbContext : DbContext
    {
        public FollowDbContext(DbContextOptions<FollowDbContext> options) : base(options) { }

        public DbSet<FollowEntity> Follows { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Composite unique index — prevent duplicate follows
            modelBuilder.Entity<FollowEntity>()
                .HasIndex(f => new { f.FollowerId, f.FolloweeId })
                .IsUnique();

            // Index on FolloweeId for follower list queries
            modelBuilder.Entity<FollowEntity>()
                .HasIndex(f => f.FolloweeId);

            // Default status
            modelBuilder.Entity<FollowEntity>()
                .Property(f => f.Status)
                .HasDefaultValue("PENDING");
        }
    }
}