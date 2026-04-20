using Comment.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace Comment.API.Data
{
    public class CommentDbContext : DbContext
    {
        public CommentDbContext(DbContextOptions<CommentDbContext> options) : base(options) { }

        public DbSet<CommentEntity> Comments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Composite index on PostId + ParentCommentId
            modelBuilder.Entity<CommentEntity>()
                .HasIndex(c => new { c.PostId, c.ParentCommentId });

            // Default values
            modelBuilder.Entity<CommentEntity>()
                .Property(c => c.IsDeleted)
                .HasDefaultValue(false);

            modelBuilder.Entity<CommentEntity>()
                .Property(c => c.IsEdited)
                .HasDefaultValue(false);

            modelBuilder.Entity<CommentEntity>()
                .Property(c => c.LikeCount)
                .HasDefaultValue(0);

            modelBuilder.Entity<CommentEntity>()
                .Property(c => c.ReplyCount)
                .HasDefaultValue(0);
        }
    }
}