using Like.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace Like.API.Data
{
    public class LikeDbContext : DbContext
    {
        public LikeDbContext(DbContextOptions<LikeDbContext> options) : base(options) { }

        public DbSet<LikeEntity> Likes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Composite unique index — prevents duplicate likes at DB level
            modelBuilder.Entity<LikeEntity>()
                .HasIndex(l => new { l.UserId, l.TargetId, l.TargetType })
                .IsUnique();
        }
    }
}