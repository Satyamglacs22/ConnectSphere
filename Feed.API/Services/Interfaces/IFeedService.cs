using Feed.API.DTOs;
using Feed.API.Entities;

namespace Feed.API.Services.Interfaces
{
    public interface IFeedService
    {
        Task<IList<FeedResponseDto>> GetFeed(
            int userId, int page, int pageSize);
        Task<IList<FeedItemEntity>> GetUnseenFeed(int userId);
        Task<int> GetUnseenCount(int userId);
        Task MarkFeedAsSeen(int userId);
    }
}