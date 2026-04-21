using Follow.API.Entities;

namespace Follow.API.Repositories.Interfaces
{
    public interface IFollowRepository
    {
        Task<FollowEntity?> FindByFollowerAndFollowee(int followerId, int followeeId);
        Task<FollowEntity?> FindByFollowId(int followId);
        Task<IList<FollowEntity>> FindFollowersByUserId(int userId);
        Task<IList<FollowEntity>> FindFollowingByUserId(int userId);
        Task<IList<FollowEntity>> FindPendingRequests(int userId);
        Task<bool> IsFollowing(int followerId, int followeeId);
        Task<int> CountFollowers(int userId);
        Task<int> CountFollowing(int userId);
        Task<FollowEntity> Create(FollowEntity follow);
        Task<FollowEntity> Update(FollowEntity follow);
        Task DeleteByFollowId(int id);
        Task<IList<int>> FindFollowerIds(int userId);
        Task<IList<int>> FindFollowingIds(int userId);
    }
}