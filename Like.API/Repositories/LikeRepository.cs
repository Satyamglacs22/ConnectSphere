using Like.API.Data;
using Like.API.Entities;
using Like.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Like.API.Repositories
{
    public class LikeRepository : ILikeRepository
    {
        private readonly LikeDbContext _context;

        public LikeRepository(LikeDbContext context)
        {
            _context = context;
        }

        public async Task<LikeEntity?> FindByLikeId(int id)
        {
            return await _context.Likes.FindAsync(id);
        }

        public async Task<LikeEntity?> FindByUserAndTarget(int userId, int targetId, string targetType)
        {
            return await _context.Likes
                .FirstOrDefaultAsync(l =>
                    l.UserId == userId &&
                    l.TargetId == targetId &&
                    l.TargetType == targetType);
        }

        public async Task<IList<LikeEntity>> FindByTargetId(int targetId, string targetType)
        {
            return await _context.Likes
                .Where(l => l.TargetId == targetId && l.TargetType == targetType)
                .ToListAsync();
        }

        public async Task<IList<LikeEntity>> FindByUserId(int userId)
        {
            return await _context.Likes
                .Where(l => l.UserId == userId)
                .ToListAsync();
        }

        public async Task<int> CountByTargetId(int targetId, string targetType)
        {
            return await _context.Likes
                .CountAsync(l => l.TargetId == targetId && l.TargetType == targetType);
        }

        public async Task<bool> HasLiked(int userId, int targetId, string targetType)
        {
            return await _context.Likes
                .AnyAsync(l =>
                    l.UserId == userId &&
                    l.TargetId == targetId &&
                    l.TargetType == targetType);
        }

        public async Task<LikeEntity> Create(LikeEntity like)
        {
            _context.Likes.Add(like);
            await _context.SaveChangesAsync();
            return like;
        }

        public async Task DeleteByLikeId(int id)
        {
            await _context.Likes
                .Where(l => l.LikeId == id)
                .ExecuteDeleteAsync();
        }

        public async Task<IList<int>> FindLikerIdsByTarget(int targetId, string targetType)
        {
            return await _context.Likes
                .Where(l => l.TargetId == targetId && l.TargetType == targetType)
                .Select(l => l.UserId)
                .ToListAsync();
        }

        public async Task<IList<int>> FindLikedPostIdsByUser(int userId)
        {
            return await _context.Likes
                .Where(l => l.UserId == userId && l.TargetType == "POST")
                .Select(l => l.TargetId)
                .ToListAsync();
        }
    }
}