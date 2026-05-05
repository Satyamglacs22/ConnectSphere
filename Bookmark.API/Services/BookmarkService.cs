using Bookmark.API.Entities;
using Bookmark.API.Repositories.Interfaces;
using Bookmark.API.Services.Interfaces;

namespace Bookmark.API.Services
{
    public class BookmarkService : IBookmarkService
    {
        private readonly IBookmarkRepository _repo;

        public BookmarkService(IBookmarkRepository repo)
        {
            _repo = repo;
        }

        public async Task<bool> ToggleBookmark(int userId, int postId)
        {
            var existing = await _repo.FindBookmark(userId, postId);
            if (existing != null)
            {
                await _repo.Delete(userId, postId);
                return false; // Removed
            }
            else
            {
                var bookmark = new BookmarkEntity
                {
                    UserId = userId,
                    PostId = postId
                };
                await _repo.Create(bookmark);
                return true; // Added
            }
        }

        public async Task<IList<BookmarkEntity>> GetUserBookmarks(int userId)
        {
            return await _repo.FindByUserId(userId);
        }

        public async Task<bool> IsBookmarked(int userId, int postId)
        {
            return await _repo.IsBookmarked(userId, postId);
        }
    }
}
