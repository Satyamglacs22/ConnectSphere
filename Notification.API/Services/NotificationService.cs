using Notification.API.DTOs;
using Notification.API.Entities;
using Notification.API.HttpClients;
using Notification.API.Repositories.Interfaces;
using Notification.API.Services.Interfaces;

namespace Notification.API.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _repo;
        private readonly AuthServiceClient _authClient;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            INotificationRepository repo,
            AuthServiceClient authClient,
            ILogger<NotificationService> logger)
        {
            _repo = repo;
            _authClient = authClient;
            _logger = logger;
        }

        // ── Core Send ──────────────────────────────────────────────────────
        public async Task<NotificationEntity> Send(NotificationEntity notif)
        {
            notif.CreatedAt = DateTime.UtcNow;
            notif.IsRead = false;
            return await _repo.Create(notif);
        }

        // ── Type 1: LIKE_POST or LIKE_COMMENT ─────────────────────────────
        public async Task SendLikeNotif(
            int recipientId, int actorId,
            int targetId, string targetType)
        {
            var actorName = await _authClient.GetUserName(actorId);
            var type = targetType == "POST"
                ? "LIKE_POST" : "LIKE_COMMENT";

            var message = targetType == "POST"
                ? $"{actorName} liked your post."
                : $"{actorName} liked your comment.";

            // Deduplicate: Remove any existing notification of the same type from the same actor for the same target
            await _repo.DeleteDuplicates(recipientId, actorId, targetId, type);

            await Send(new NotificationEntity
            {
                RecipientId = recipientId,
                ActorId = actorId,
                Type = type,
                Message = message,
                TargetId = targetId,
                TargetType = targetType
            });

            _logger.LogInformation(
                "Like notification sent to user {recipientId}", recipientId);
        }

        // ── Type 2: NEW_COMMENT ────────────────────────────────────────────
        public async Task SendCommentNotif(
            int postAuthorId, int actorId, int postId)
        {
            var actorName = await _authClient.GetUserName(actorId);
            await Send(new NotificationEntity
            {
                RecipientId = postAuthorId,
                ActorId = actorId,
                Type = "NEW_COMMENT",
                Message = "[ACTOR_NAME] commented on your post.",
                TargetId = postId,
                TargetType = "POST"
            });

            _logger.LogInformation(
                "Comment notification sent to user {postAuthorId}",
                postAuthorId);
        }

        // ── Type 3: NEW_REPLY ──────────────────────────────────────────────
        public async Task SendReplyNotif(
            int commentAuthorId, int actorId, int commentId)
        {
            var actorName = await _authClient.GetUserName(actorId);
            await Send(new NotificationEntity
            {
                RecipientId = commentAuthorId,
                ActorId = actorId,
                Type = "NEW_REPLY",
                Message = $"{actorName} replied to your comment.",
                TargetId = commentId,
                TargetType = "COMMENT"
            });

            _logger.LogInformation(
                "Reply notification sent to user {commentAuthorId}",
                commentAuthorId);
        }

        // ── Type 4: NEW_FOLLOWER / FOLLOW_REQUEST / FOLLOW_ACCEPTED ────────
        public async Task SendFollowNotif(
            int targetId, int followerId, string type, int? followId = null)
        {
            var actorName = await _authClient.GetUserName(followerId);
            var targetName = await _authClient.GetUserName(targetId);

            var message = type switch
            {
                "NEW_FOLLOWER"
                    => $"{actorName} started following you.",
                "FOLLOW_REQUEST"
                    => $"{actorName} sent you a follow request.",
                "FOLLOW_ACCEPTED"
                    => $"{targetName} accepted your follow request.",
                _ => $"Follow notification from {actorName}."
            };

            // Deduplicate: Remove any existing follow notification from the same actor
            await _repo.DeleteDuplicates(targetId, followerId, followerId, type);

            await Send(new NotificationEntity
            {
                RecipientId = targetId,
                ActorId = followerId,
                Type = type,
                Message = message,
                // If it's a follow request, TargetId should be the followId (to allow accept/reject)
                // Otherwise, it's the followerId
                TargetId = (type == "FOLLOW_REQUEST" && followId.HasValue) ? followId.Value : followerId,
                TargetType = "USER"
            });

            _logger.LogInformation(
                "Follow notification ({type}) sent to user {targetId}",
                type, targetId);
        }

        // ── Type 5: MENTION ────────────────────────────────────────────────
        public async Task SendMentionNotif(
            int mentionedId, int actorId, int postId)
        {
            var actorName = await _authClient.GetUserName(actorId);
            await Send(new NotificationEntity
            {
                RecipientId = mentionedId,
                ActorId = actorId,
                Type = "MENTION",
                Message = $"{actorName} mentioned you in a post.",
                TargetId = postId,
                TargetType = "POST"
            });

            _logger.LogInformation(
                "Mention notification sent to user {mentionedId}",
                mentionedId);
        }

        // ── Type 6: PLATFORM (Admin Broadcast) ────────────────────────────
        public async Task SendBulk(
            IList<int> userIds, string title, string message)
        {
            // Create notification for each user
            var notifications = userIds.Select(userId =>
                new NotificationEntity
                {
                    RecipientId = userId,
                    ActorId = 0, // 0 = system/admin
                    Type = "PLATFORM",
                    Message = $"{title}: {message}",
                    TargetId = 0,
                    TargetType = "USER",
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false
                }).ToList();

            // Batch insert — all at once
            await _repo.CreateBatch(notifications);

            _logger.LogInformation(
                "Platform broadcast sent to {count} users",
                userIds.Count);
        }

        // ── Read Operations ────────────────────────────────────────────────
        public async Task<IList<NotificationResponseDto>> GetByRecipient(int userId)
        {
            var notifications = await _repo.FindByRecipientId(userId);
            return await EnrichList(notifications);
        }

        public async Task<IList<NotificationResponseDto>> GetUnread(int userId)
        {
            var notifications = await _repo.FindUnreadByRecipientId(userId);
            return await EnrichList(notifications);
        }

        public async Task<int> GetUnreadCount(int userId)
        {
            return await _repo.CountUnreadByRecipientId(userId);
        }

        // ── Mark Read ──────────────────────────────────────────────────────
        public async Task MarkAsRead(int notifId)
        {
            await _repo.MarkSingleRead(notifId);
        }

        public async Task MarkAllRead(int userId)
        {
            await _repo.MarkAllRead(userId);
        }

        // ── Delete ─────────────────────────────────────────────────────────
        public async Task DeleteNotif(int id)
        {
            await _repo.DeleteByNotifId(id);
        }

        public async Task ResolveFollowRequestNotification(int followId)
        {
            await _repo.DeleteByTargetAndType(followId, "FOLLOW_REQUEST");
            _logger.LogInformation(
                "Follow request notification for followId {followId} resolved (deleted).",
                followId);
        }

        // ── Private Helpers ────────────────────────────────────────────────
        private async Task<NotificationResponseDto> Enrich(NotificationEntity entity)
        {
            var dto = new NotificationResponseDto
            {
                NotificationId = entity.NotificationId,
                RecipientId = entity.RecipientId,
                ActorId = entity.ActorId,
                Type = entity.Type,
                Message = entity.Message,
                TargetId = entity.TargetId,
                TargetType = entity.TargetType,
                IsRead = entity.IsRead,
                CreatedAt = entity.CreatedAt,
                ActorName = $"User {entity.ActorId}", // Default
                ActorAvatarUrl = ""
            };

            // Resolve real actor name/avatar from Auth API
            if (entity.ActorId > 0)
            {
                var user = await _authClient.GetUserDetails(entity.ActorId);
                if (user != null)
                {
                    var realName = !string.IsNullOrWhiteSpace(user.FullName) ? user.FullName : user.UserName;
                    if (!string.IsNullOrWhiteSpace(realName))
                    {
                        dto.ActorName = realName;
                        
                        // Replace dynamic placeholder
                        dto.Message = dto.Message.Replace("[ACTOR_NAME]", realName, StringComparison.OrdinalIgnoreCase);
                        
                        // Also handle legacy "User X" fallback
                        dto.Message = dto.Message.Replace($"User {entity.ActorId}", realName, StringComparison.OrdinalIgnoreCase);
                    }
                    dto.ActorAvatarUrl = user.AvatarUrl;
                }
            }

            return dto;
        }

        private async Task<IList<NotificationResponseDto>> EnrichList(IList<NotificationEntity> entities)
        {
            var results = new List<NotificationResponseDto>();
            foreach (var entity in entities)
            {
                results.Add(await Enrich(entity));
            }
            return results;
        }
    }
}