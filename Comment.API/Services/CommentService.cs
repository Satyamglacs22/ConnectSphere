using System.Text.RegularExpressions;
using Comment.API.DTOs;
using Comment.API.Entities;
using Comment.API.HttpClients;
using Comment.API.Repositories.Interfaces;
using Comment.API.Services.Interfaces;

namespace Comment.API.Services
{
    public class CommentService : ICommentService
    {
        private readonly ICommentRepository _repo;
        private readonly PostServiceClient _postClient;
        private readonly NotificationServiceClient _notifClient;
        private readonly AuthServiceClient _authClient;
        private readonly ILogger<CommentService> _logger;

        public CommentService(
            ICommentRepository repo,
            PostServiceClient postClient,
            NotificationServiceClient notifClient,
            AuthServiceClient authClient,
            ILogger<CommentService> logger)
        {
            _repo = repo;
            _postClient = postClient;
            _notifClient = notifClient;
            _authClient = authClient;
            _logger = logger;
        }

        public async Task<CommentResponseDto> AddComment(AddCommentDto dto)
        {
            var comment = new CommentEntity
            {
                PostId = dto.PostId,
                UserId = dto.UserId,
                Content = dto.Content,
                ParentCommentId = dto.ParentCommentId,
                CreatedAt = DateTime.UtcNow
            };

            var created = await _repo.Create(comment);

            // Increment CommentCount on Post API
            await _postClient.IncrementCommentCount(dto.PostId, +1);

            // If it's a reply, increment ReplyCount on parent comment
            if (dto.ParentCommentId.HasValue)
                await _repo.IncrementReplyCount(dto.ParentCommentId.Value, +1);

            // Send comment notification to post author
            await _notifClient.SendCommentNotification(
                postAuthorId: dto.PostId,
                actorId: dto.UserId,
                postId: dto.PostId);

            // Parse @mentions and send mention notifications
            await ParseAndSendMentions(dto.Content, dto.UserId, dto.PostId);

            return MapToDto(created);
        }

        public async Task<CommentResponseDto?> GetCommentById(int id)
        {
            var comment = await _repo.FindByCommentId(id);
            if (comment == null) return null;
            return MapToDto(comment);
        }

        public async Task<IList<CommentResponseDto>> GetCommentsByPost(int postId)
        {
            var comments = await _repo.FindByPostId(postId);
            return comments.Select(MapToDto).ToList();
        }

        public async Task<IList<CommentResponseDto>> GetTopLevelComments(int postId)
        {
            var comments = await _repo.FindTopLevelByPostId(postId);
            return comments.Select(MapToDto).ToList();
        }

        public async Task<IList<CommentResponseDto>> GetReplies(int commentId)
        {
            var comments = await _repo.FindReplies(commentId);
            return comments.Select(MapToDto).ToList();
        }

        public async Task<IList<CommentResponseDto>> GetCommentsByUser(int userId)
        {
            var comments = await _repo.FindByUserId(userId);
            return comments.Select(MapToDto).ToList();
        }

        public async Task<CommentResponseDto> EditComment(
            int id, int requestingUserId, string content)
        {
            var comment = await _repo.FindByCommentId(id);
            if (comment == null) throw new Exception("Comment not found.");

            if (comment.UserId != requestingUserId)
                throw new UnauthorizedAccessException("You can only edit your own comments.");

            comment.Content = content;
            comment.IsEdited = true;
            comment.EditedAt = DateTime.UtcNow;

            var updated = await _repo.Update(comment);
            return MapToDto(updated);
        }

        public async Task DeleteComment(int id, int requestingUserId)
        {
            var comment = await _repo.FindByCommentId(id);
            if (comment == null) throw new Exception("Comment not found.");

            if (comment.UserId != requestingUserId)
                throw new UnauthorizedAccessException("You can only delete your own comments.");

            // Soft delete — content masked, thread structure preserved
            await _repo.DeleteByCommentId(id);

            // Decrement CommentCount on Post API
            await _postClient.IncrementCommentCount(comment.PostId, -1);
        }

        public async Task<int> GetCommentCount(int postId)
        {
            return await _repo.CountByPostId(postId);
        }

        public async Task IncrementLikeCount(int commentId, int delta)
        {
            await _repo.IncrementLikeCount(commentId, delta);
        }

        // ── Private Helpers ────────────────────────────────────────────────

        // Parse @mentions from content and send notifications
        private async Task ParseAndSendMentions(
            string content, int actorId, int postId)
        {
            var mentions = Regex.Matches(content, @"@(\w+)")
                .Select(m => m.Groups[1].Value)
                .Distinct();

            foreach (var username in mentions)
            {
                try
                {
                    var mentionedUserId = await _authClient.GetUserIdByUsername(username);
                    if (mentionedUserId.HasValue)
                    {
                        await _notifClient.SendMentionNotification(
                            mentionedUserId.Value, actorId, postId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(
                        "Failed to send mention notification for @{username}: {msg}",
                        username, ex.Message);
                }
            }
        }

        // Map entity to DTO — applies soft delete masking
        private CommentResponseDto MapToDto(CommentEntity comment)
        {
            return new CommentResponseDto
            {
                CommentId = comment.CommentId,
                PostId = comment.PostId,
                UserId = comment.UserId,
                ParentCommentId = comment.ParentCommentId,
                // Mask content if soft deleted
                Content = comment.IsDeleted
                    ? "This comment was deleted."
                    : comment.Content,
                LikeCount = comment.LikeCount,
                ReplyCount = comment.ReplyCount,
                IsEdited = comment.IsEdited,
                CreatedAt = comment.CreatedAt,
                EditedAt = comment.EditedAt
            };
        }
    }
}