using Follow.API.DTOs;
using Follow.API.Entities;

namespace Follow.API.Services.Interfaces
{
    public interface IFollowService
    {
        Task<FollowResponseDto> FollowUser(int followerId, int followeeId);
        Task UnfollowUser(int followerId, int followeeId);
        Task<FollowResponseDto> AcceptFollowRequest(int followId);
        Task RejectFollowRequest(int followId);
        Task<IList<FollowEntity>> GetFollowers(int userId);
        Task<IList<FollowEntity>> GetFollowing(int userId);
        Task<IList<FollowEntity>> GetPendingRequests(int userId);
        Task<bool> IsFollowing(int followerId, int followeeId);
        Task<int> GetFollowerCount(int userId);
        Task<int> GetFollowingCount(int userId);
        Task<IList<int>> GetFollowingIds(int userId);
        Task<IList<int>> GetFollowerIds(int userId);
        Task<MutualFollowersDto> GetMutualFollowers(int userAId, int userBId);
    }
}