using Like.API.Entities;

namespace Like.API.Services.Interfaces
{
    public interface ILikeService
    {
        Task<(bool Liked, int LikeCount)> ToggleLike(int userId, int targetId, string targetType);
        Task<IList<LikeEntity>> GetLikesByTarget(int targetId, string targetType);
        Task<IList<LikeEntity>> GetLikesByUser(int userId);
        Task<int> GetLikeCount(int targetId, string targetType);
        Task<bool> HasUserLiked(int userId, int targetId, string targetType);
        Task<IList<int>> GetLikersForTarget(int targetId, string targetType);
        Task<IList<int>> GetLikedPostsByUser(int userId);
    }
}