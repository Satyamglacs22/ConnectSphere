using Comment.API.Data;
using Comment.API.Entities;
using Comment.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Comment.API.Repositories
{
    public class CommentRepository : ICommentRepository
    {
        private readonly CommentDbContext _context;

        public CommentRepository(CommentDbContext context)
        {
            _context = context;
        }

        public async Task<CommentEntity?> FindByCommentId(int id)
        {
            return await _context.Comments.FindAsync(id);
        }

        public async Task<IList<CommentEntity>> FindByPostId(int postId)
        {
            return await _context.Comments
                .Where(c => c.PostId == postId)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<IList<CommentEntity>> FindTopLevelByPostId(int postId)
        {
            return await _context.Comments
                .Where(c => c.PostId == postId
                            && c.ParentCommentId == null)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<IList<CommentEntity>> FindReplies(int commentId)
        {
            return await _context.Comments
                .Where(c => c.ParentCommentId == commentId)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<IList<CommentEntity>> FindByUserId(int userId)
        {
            return await _context.Comments
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<int> CountByPostId(int postId)
        {
            return await _context.Comments
                .CountAsync(c => c.PostId == postId && !c.IsDeleted);
        }

        public async Task IncrementLikeCount(int commentId, int delta)
        {
            await _context.Comments
                .Where(c => c.CommentId == commentId)
                .ExecuteUpdateAsync(s => s.SetProperty(
                    c => c.LikeCount, c => c.LikeCount + delta));
        }

        public async Task IncrementReplyCount(int commentId, int delta)
        {
            await _context.Comments
                .Where(c => c.CommentId == commentId)
                .ExecuteUpdateAsync(s => s.SetProperty(
                    c => c.ReplyCount, c => c.ReplyCount + delta));
        }

        public async Task DeleteByCommentId(int id)
        {
            await _context.Comments
                .Where(c => c.CommentId == id)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(c => c.IsDeleted, true)
                    .SetProperty(c => c.Content, "This comment was deleted."));
        }

        public async Task<CommentEntity> Create(CommentEntity comment)
        {
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
            return comment;
        }

        public async Task<CommentEntity> Update(CommentEntity comment)
        {
            _context.Comments.Update(comment);
            await _context.SaveChangesAsync();
            return comment;
        }
    }
}