using Bookmark.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace Bookmark.API.Data
{
    public class BookmarkDbContext : DbContext
    {
        public BookmarkDbContext(DbContextOptions<BookmarkDbContext> options) : base(options) { }

        public DbSet<BookmarkEntity> Bookmarks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<BookmarkEntity>()
                .HasIndex(b => new { b.UserId, b.PostId })
                .IsUnique();
        }
    }
}
