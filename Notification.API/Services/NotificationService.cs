using Notification.API.Entities;
using Notification.API.Repositories.Interfaces;
using Notification.API.Services.Interfaces;

namespace Notification.API.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _repo;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            INotificationRepository repo,
            ILogger<NotificationService> logger)
        {
            _repo = repo;
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
            var type = targetType == "POST"
                ? "LIKE_POST" : "LIKE_COMMENT";

            var message = targetType == "POST"
                ? $"User {actorId} liked your post."
                : $"User {actorId} liked your comment.";

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
            await Send(new NotificationEntity
            {
                RecipientId = postAuthorId,
                ActorId = actorId,
                Type = "NEW_COMMENT",
                Message = $"User {actorId} commented on your post.",
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
            await Send(new NotificationEntity
            {
                RecipientId = commentAuthorId,
                ActorId = actorId,
                Type = "NEW_REPLY",
                Message = $"User {actorId} replied to your comment.",
                TargetId = commentId,
                TargetType = "COMMENT"
            });

            _logger.LogInformation(
                "Reply notification sent to user {commentAuthorId}",
                commentAuthorId);
        }

        // ── Type 4: NEW_FOLLOWER / FOLLOW_REQUEST / FOLLOW_ACCEPTED ────────
        public async Task SendFollowNotif(
            int targetId, int followerId, string type)
        {
            var message = type switch
            {
                "NEW_FOLLOWER"
                    => $"User {followerId} started following you.",
                "FOLLOW_REQUEST"
                    => $"User {followerId} sent you a follow request.",
                "FOLLOW_ACCEPTED"
                    => $"User {targetId} accepted your follow request.",
                _ => $"Follow notification from user {followerId}."
            };

            await Send(new NotificationEntity
            {
                RecipientId = targetId,
                ActorId = followerId,
                Type = type,
                Message = message,
                TargetId = followerId,
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
            await Send(new NotificationEntity
            {
                RecipientId = mentionedId,
                ActorId = actorId,
                Type = "MENTION",
                Message = $"User {actorId} mentioned you in a post.",
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
        public async Task<IList<NotificationEntity>> GetByRecipient(int userId)
        {
            return await _repo.FindByRecipientId(userId);
        }

        public async Task<IList<NotificationEntity>> GetUnread(int userId)
        {
            return await _repo.FindUnreadByRecipientId(userId);
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
    }
}