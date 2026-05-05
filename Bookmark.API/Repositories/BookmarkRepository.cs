using Bookmark.API.Data;
using Bookmark.API.Entities;
using Bookmark.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Bookmark.API.Repositories
{
    public class BookmarkRepository : IBookmarkRepository
    {
        private readonly BookmarkDbContext _context;

        public BookmarkRepository(BookmarkDbContext context)
        {
            _context = context;
        }

        public async Task<BookmarkEntity?> FindBookmark(int userId, int postId)
        {
            return await _context.Bookmarks
                .FirstOrDefaultAsync(b => b.UserId == userId && b.PostId == postId);
        }

        public async Task<IList<BookmarkEntity>> FindByUserId(int userId)
        {
            return await _context.Bookmarks
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
        }

        public async Task<BookmarkEntity> Create(BookmarkEntity bookmark)
        {
            _context.Bookmarks.Add(bookmark);
            await _context.SaveChangesAsync();
            return bookmark;
        }

        public async Task Delete(int userId, int postId)
        {
            await _context.Bookmarks
                .Where(b => b.UserId == userId && b.PostId == postId)
                .ExecuteDeleteAsync();
        }

        public async Task<bool> IsBookmarked(int userId, int postId)
        {
            return await _context.Bookmarks
                .AnyAsync(b => b.UserId == userId && b.PostId == postId);
        }
    }
}
