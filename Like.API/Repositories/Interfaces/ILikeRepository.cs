using Like.API.Entities;

namespace Like.API.Repositories.Interfaces
{
    public interface ILikeRepository
    {
        Task<LikeEntity?> FindByLikeId(int id);
        Task<LikeEntity?> FindByUserAndTarget(int userId, int targetId, string targetType);
        Task<IList<LikeEntity>> FindByTargetId(int targetId, string targetType);
        Task<IList<LikeEntity>> FindByUserId(int userId);
        Task<int> CountByTargetId(int targetId, string targetType);
        Task<bool> HasLiked(int userId, int targetId, string targetType);
        Task<LikeEntity> Create(LikeEntity like);
        Task DeleteByLikeId(int id);
        Task<IList<int>> FindLikerIdsByTarget(int targetId, string targetType);
        Task<IList<int>> FindLikedPostIdsByUser(int userId);
    }
}