using Comment.API.DTOs;
using Comment.API.Entities;

namespace Comment.API.Services.Interfaces
{
    public interface ICommentService
    {
        Task<CommentResponseDto> AddComment(AddCommentDto dto);
        Task<CommentResponseDto?> GetCommentById(int id);
        Task<IList<CommentResponseDto>> GetCommentsByPost(int postId);
        Task<IList<CommentResponseDto>> GetTopLevelComments(int postId);
        Task<IList<CommentResponseDto>> GetReplies(int commentId);
        Task<IList<CommentResponseDto>> GetCommentsByUser(int userId);
        Task<CommentResponseDto> EditComment(int id, int requestingUserId, string content);
        Task DeleteComment(int id, int requestingUserId);
        Task<int> GetCommentCount(int postId);
        Task IncrementLikeCount(int commentId, int delta);
    }
}