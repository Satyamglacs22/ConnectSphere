using Microsoft.EntityFrameworkCore;
using Post.API.Data;
using Post.API.Entities;
using Post.API.Repositories.Interfaces;

namespace Post.API.Repositories
{
    public class PostRepository : IPostRepository
    {
        private readonly PostDbContext _context;

        public PostRepository(PostDbContext context)
        {
            _context = context;
        }

        public async Task<PostEntity?> FindByPostId(int id)
        {
            return await _context.Posts
                .FirstOrDefaultAsync(p => p.PostId == id && !p.IsDeleted);
        }

        public async Task<IList<PostEntity>> FindByUserId(int userId)
        {
            return await _context.Posts
                .Where(p => p.UserId == userId && !p.IsDeleted)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IList<PostEntity>> FindFeedForUser(int userId, IList<int> followingIds)
        {
            return await _context.Posts
                .Where(p => followingIds.Contains(p.UserId)
                            && !p.IsDeleted
                            && p.Visibility != "PRIVATE")
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IList<PostEntity>> FindByHashtag(string hashtag)
        {
            var tag = hashtag.StartsWith("#") ? hashtag : $"#{hashtag}";

            return await _context.Posts
                .Where(p => !p.IsDeleted
                            && p.Hashtags != null
                            && EF.Functions.Like(p.Hashtags, $"%{tag}%"))
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IList<PostEntity>> FindPublic()
        {
            return await _context.Posts
                .Where(p => p.Visibility == "PUBLIC" && !p.IsDeleted)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IList<PostEntity>> SearchPosts(string q)
        {
            return await _context.Posts
                .Where(p => !p.IsDeleted
                            && EF.Functions.Like(p.Content, $"%{q}%"))
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task IncrementCount(int postId, string field, int delta)
        {
            if (field == "LikeCount")
                await _context.Posts
                    .Where(p => p.PostId == postId)
                    .ExecuteUpdateAsync(s => s.SetProperty(
                        p => p.LikeCount, p => p.LikeCount + delta));

            else if (field == "CommentCount")
                await _context.Posts
                    .Where(p => p.PostId == postId)
                    .ExecuteUpdateAsync(s => s.SetProperty(
                        p => p.CommentCount, p => p.CommentCount + delta));

            else if (field == "ShareCount")
                await _context.Posts
                    .Where(p => p.PostId == postId)
                    .ExecuteUpdateAsync(s => s.SetProperty(
                        p => p.ShareCount, p => p.ShareCount + delta));
        }

        public async Task<IList<PostEntity>> FindTrending(int topN)
        {
            var since = DateTime.UtcNow.AddHours(-24);

            return await _context.Posts
                .Where(p => !p.IsDeleted
                            && p.Visibility == "PUBLIC"
                            && p.CreatedAt >= since)
                .OrderByDescending(p => (p.LikeCount * 3) + (p.CommentCount * 2) + p.ShareCount)
                .Take(topN)
                .ToListAsync();
        }

        public async Task DeleteByPostId(int id)
        {
            await _context.Posts
                .Where(p => p.PostId == id)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.IsDeleted, true));
        }

        public async Task<PostEntity> Create(PostEntity post)
        {
            _context.Posts.Add(post);
            await _context.SaveChangesAsync();
            return post;
        }

        public async Task<PostEntity> Update(PostEntity post)
        {
            _context.Posts.Update(post);
            await _context.SaveChangesAsync();
            return post;
        }
    }
}