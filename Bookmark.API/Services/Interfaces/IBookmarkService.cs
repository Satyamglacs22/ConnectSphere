using Bookmark.API.Entities;

namespace Bookmark.API.Services.Interfaces
{
    public interface IBookmarkService
    {
        Task<bool> ToggleBookmark(int userId, int postId);
        Task<IList<BookmarkEntity>> GetUserBookmarks(int userId);
        Task<bool> IsBookmarked(int userId, int postId);
    }
}
