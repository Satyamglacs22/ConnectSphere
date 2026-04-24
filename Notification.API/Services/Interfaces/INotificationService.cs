using Notification.API.Entities;

namespace Notification.API.Services.Interfaces
{
    public interface INotificationService
    {
        // Core send method
        Task<NotificationEntity> Send(NotificationEntity notif);

        // Typed helper methods — 9 notification types
        Task SendLikeNotif(
            int recipientId, int actorId,
            int targetId, string targetType);

        Task SendCommentNotif(
            int postAuthorId, int actorId, int postId);

        Task SendReplyNotif(
            int commentAuthorId, int actorId, int commentId);

        Task SendFollowNotif(
            int targetId, int followerId, string type);

        Task SendMentionNotif(
            int mentionedId, int actorId, int postId);

        Task SendBulk(
            IList<int> userIds, string title, string message);

        // Read operations
        Task<IList<NotificationEntity>> GetByRecipient(int userId);
        Task<IList<NotificationEntity>> GetUnread(int userId);
        Task<int> GetUnreadCount(int userId);

        // Mark read operations
        Task MarkAsRead(int notifId);
        Task MarkAllRead(int userId);

        // Delete
        Task DeleteNotif(int id);
    }
}