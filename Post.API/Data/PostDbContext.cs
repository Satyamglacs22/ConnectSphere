using Microsoft.EntityFrameworkCore;
using Post.API.Entities;

namespace Post.API.Data
{
    public class PostDbContext : DbContext
    {
        public PostDbContext(DbContextOptions<PostDbContext> options) : base(options) { }

        public DbSet<PostEntity> Posts { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Composite index on UserId + CreatedAt for feed queries
            modelBuilder.Entity<PostEntity>()
                .HasIndex(p => new { p.UserId, p.CreatedAt });

            // Default values
            modelBuilder.Entity<PostEntity>()
                .Property(p => p.Visibility)
                .HasDefaultValue("PUBLIC");

            modelBuilder.Entity<PostEntity>()
                .Property(p => p.IsDeleted)
                .HasDefaultValue(false);

            modelBuilder.Entity<PostEntity>()
                .Property(p => p.LikeCount)
                .HasDefaultValue(0);

            modelBuilder.Entity<PostEntity>()
                .Property(p => p.CommentCount)
                .HasDefaultValue(0);

            modelBuilder.Entity<PostEntity>()
                .Property(p => p.ShareCount)
                .HasDefaultValue(0);
        }
    }
}