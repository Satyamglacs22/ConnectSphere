using Follow.API.Data;
using Follow.API.Entities;
using Follow.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Follow.API.Repositories
{
    public class FollowRepository : IFollowRepository
    {
        private readonly FollowDbContext _context;

        public FollowRepository(FollowDbContext context)
        {
            _context = context;
        }

        public async Task<FollowEntity?> FindByFollowerAndFollowee(
            int followerId, int followeeId)
        {
            return await _context.Follows
                .FirstOrDefaultAsync(f =>
                    f.FollowerId == followerId &&
                    f.FolloweeId == followeeId);
        }

        public async Task<FollowEntity?> FindByFollowId(int followId)
        {
            return await _context.Follows.FindAsync(followId);
        }

        public async Task<IList<FollowEntity>> FindFollowersByUserId(int userId)
        {
            return await _context.Follows
                .Where(f => f.FolloweeId == userId && f.Status == "ACCEPTED")
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();
        }

        public async Task<IList<FollowEntity>> FindFollowingByUserId(int userId)
        {
            return await _context.Follows
                .Where(f => f.FollowerId == userId && f.Status == "ACCEPTED")
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();
        }

        public async Task<IList<FollowEntity>> FindPendingRequests(int userId)
        {
            return await _context.Follows
                .Where(f => f.FolloweeId == userId && f.Status == "PENDING")
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> IsFollowing(int followerId, int followeeId)
        {
            return await _context.Follows
                .AnyAsync(f =>
                    f.FollowerId == followerId &&
                    f.FolloweeId == followeeId &&
                    f.Status == "ACCEPTED");
        }

        public async Task<int> CountFollowers(int userId)
        {
            return await _context.Follows
                .CountAsync(f => f.FolloweeId == userId && f.Status == "ACCEPTED");
        }

        public async Task<int> CountFollowing(int userId)
        {
            return await _context.Follows
                .CountAsync(f => f.FollowerId == userId && f.Status == "ACCEPTED");
        }

        public async Task<FollowEntity> Create(FollowEntity follow)
        {
            _context.Follows.Add(follow);
            await _context.SaveChangesAsync();
            return follow;
        }

        public async Task<FollowEntity> Update(FollowEntity follow)
        {
            _context.Follows.Update(follow);
            await _context.SaveChangesAsync();
            return follow;
        }

        public async Task DeleteByFollowId(int id)
        {
            await _context.Follows
                .Where(f => f.FollowId == id)
                .ExecuteDeleteAsync();
        }

        public async Task<IList<int>> FindFollowerIds(int userId)
        {
            return await _context.Follows
                .Where(f => f.FolloweeId == userId && f.Status == "ACCEPTED")
                .Select(f => f.FollowerId)
                .ToListAsync();
        }

        public async Task<IList<int>> FindFollowingIds(int userId)
        {
            return await _context.Follows
                .Where(f => f.FollowerId == userId && f.Status == "ACCEPTED")
                .Select(f => f.FolloweeId)
                .ToListAsync();
        }
    }
}