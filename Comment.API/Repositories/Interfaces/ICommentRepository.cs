using Comment.API.Entities;

namespace Comment.API.Repositories.Interfaces
{
    public interface ICommentRepository
    {
        Task<CommentEntity?> FindByCommentId(int id);
        Task<IList<CommentEntity>> FindByPostId(int postId);
        Task<IList<CommentEntity>> FindTopLevelByPostId(int postId);
        Task<IList<CommentEntity>> FindReplies(int commentId);
        Task<IList<CommentEntity>> FindByUserId(int userId);
        Task<int> CountByPostId(int postId);
        Task IncrementLikeCount(int commentId, int delta);
        Task IncrementReplyCount(int commentId, int delta);
        Task DeleteByCommentId(int id);
        Task<CommentEntity> Create(CommentEntity comment);
        Task<CommentEntity> Update(CommentEntity comment);
    }
}