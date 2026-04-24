using Feed.API.Data;
using Feed.API.Entities;
using Feed.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Feed.API.Repositories
{
    public class FeedRepository : IFeedRepository
    {
        private readonly FeedDbContext _context;

        public FeedRepository(FeedDbContext context)
        {
            _context = context;
        }

        public async Task<IList<FeedItemEntity>> GetFeedByUserId(
            int userId, int page, int pageSize)
        {
            return await _context.FeedItems
                .Where(f => f.UserId == userId)
                .OrderByDescending(f => f.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IList<FeedItemEntity>> GetUnseenFeed(int userId)
        {
            return await _context.FeedItems
                .Where(f => f.UserId == userId && !f.IsSeen)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();
        }

        public async Task<int> GetUnseenCount(int userId)
        {
            return await _context.FeedItems
                .CountAsync(f => f.UserId == userId && !f.IsSeen);
        }

        public async Task MarkAsSeen(int userId)
        {
            await _context.FeedItems
                .Where(f => f.UserId == userId && !f.IsSeen)
                .ExecuteUpdateAsync(s => s.SetProperty(
                    f => f.IsSeen, true));
        }

        public async Task<bool> FeedItemExists(int userId, int postId)
        {
            return await _context.FeedItems
                .AnyAsync(f => f.UserId == userId && f.PostId == postId);
        }

        public async Task<FeedItemEntity> Create(FeedItemEntity item)
        {
            _context.FeedItems.Add(item);
            await _context.SaveChangesAsync();
            return item;
        }

        public async Task CreateBatch(IList<FeedItemEntity> items)
        {
            await _context.FeedItems.AddRangeAsync(items);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteByPostId(int postId)
        {
            await _context.FeedItems
                .Where(f => f.PostId == postId)
                .ExecuteDeleteAsync();
        }
    }
}