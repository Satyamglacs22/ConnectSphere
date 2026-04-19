using Like.API.Data;
using Like.API.Entities;
using Like.API.HttpClients;
using Like.API.Repositories.Interfaces;
using Like.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Like.API.Services
{
    public class LikeService : ILikeService
    {
        private readonly ILikeRepository _repo;
        private readonly LikeDbContext _context;
        private readonly PostServiceClient _postClient;
        private readonly NotificationServiceClient _notifClient;
        private readonly ILogger<LikeService> _logger;

        public LikeService(
            ILikeRepository repo,
            LikeDbContext context,
            PostServiceClient postClient,
            NotificationServiceClient notifClient,
            ILogger<LikeService> logger)
        {
            _repo = repo;
            _context = context;
            _postClient = postClient;
            _notifClient = notifClient;
            _logger = logger;
        }

        public async Task<(bool Liked, int LikeCount)> ToggleLike(
            int userId, int targetId, string targetType)
        {
            // Begin EF Core transaction — atomic operation
            using var tx = await _context.Database.BeginTransactionAsync();

            try
            {
                bool alreadyLiked = await _repo.HasLiked(userId, targetId, targetType);

                if (!alreadyLiked)
                {
                    // ── ADD LIKE ──────────────────────────────────────────
                    var like = new LikeEntity
                    {
                        UserId = userId,
                        TargetId = targetId,
                        TargetType = targetType,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _repo.Create(like);

                    // Update like count on Post or Comment service
                    await UpdateExternalLikeCount(targetId, targetType, +1);

                    // Send notification (only on like, not unlike)
                    await _notifClient.SendLikeNotification(
                        recipientId: targetId,
                        actorId: userId,
                        targetId: targetId,
                        targetType: targetType);

                    await tx.CommitAsync();

                    int likeCount = await _repo.CountByTargetId(targetId, targetType);
                    return (true, likeCount);
                }
                else
                {
                    // ── REMOVE LIKE ───────────────────────────────────────
                    var existing = await _repo.FindByUserAndTarget(userId, targetId, targetType);
                    if (existing != null)
                        await _repo.DeleteByLikeId(existing.LikeId);

                    // Update like count on Post or Comment service
                    await UpdateExternalLikeCount(targetId, targetType, -1);

                    // No notification on unlike
                    await tx.CommitAsync();

                    int likeCount = await _repo.CountByTargetId(targetId, targetType);
                    return (false, likeCount);
                }
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        public async Task<IList<LikeEntity>> GetLikesByTarget(int targetId, string targetType)
        {
            return await _repo.FindByTargetId(targetId, targetType);
        }

        public async Task<IList<LikeEntity>> GetLikesByUser(int userId)
        {
            return await _repo.FindByUserId(userId);
        }

        public async Task<int> GetLikeCount(int targetId, string targetType)
        {
            return await _repo.CountByTargetId(targetId, targetType);
        }

        public async Task<bool> HasUserLiked(int userId, int targetId, string targetType)
        {
            return await _repo.HasLiked(userId, targetId, targetType);
        }

        public async Task<IList<int>> GetLikersForTarget(int targetId, string targetType)
        {
            return await _repo.FindLikerIdsByTarget(targetId, targetType);
        }

        public async Task<IList<int>> GetLikedPostsByUser(int userId)
        {
            return await _repo.FindLikedPostIdsByUser(userId);
        }

        // ── Private Helper ─────────────────────────────────────────────────
        private async Task UpdateExternalLikeCount(int targetId, string targetType, int delta)
        {
            try
            {
                if (targetType == "POST")
                    await _postClient.UpdateLikeCount(targetId, delta);
                // Comment like count update will be added in UC4 (Comment API)
            }
            catch (Exception ex)
            {
                // Log but don't fail the like operation
                _logger.LogWarning(
                    "Failed to update like count on external service: {msg}", ex.Message);
            }
        }
    }
}