using Bookmark.API.Entities;

namespace Bookmark.API.Repositories.Interfaces
{
    public interface IBookmarkRepository
    {
        Task<BookmarkEntity?> FindBookmark(int userId, int postId);
        Task<IList<BookmarkEntity>> FindByUserId(int userId);
        Task<BookmarkEntity> Create(BookmarkEntity bookmark);
        Task Delete(int userId, int postId);
        Task<bool> IsBookmarked(int userId, int postId);
    }
}
