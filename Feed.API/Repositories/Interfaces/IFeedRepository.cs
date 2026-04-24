using Feed.API.Entities;

namespace Feed.API.Repositories.Interfaces
{
    public interface IFeedRepository
    {
        Task<IList<FeedItemEntity>> GetFeedByUserId(
            int userId, int page, int pageSize);
        Task<IList<FeedItemEntity>> GetUnseenFeed(int userId);
        Task<int> GetUnseenCount(int userId);
        Task MarkAsSeen(int userId);
        Task<bool> FeedItemExists(int userId, int postId);
        Task<FeedItemEntity> Create(FeedItemEntity item);
        Task CreateBatch(IList<FeedItemEntity> items);
        Task DeleteByPostId(int postId);
    }
}