using Notification.API.Entities;

namespace Notification.API.Repositories.Interfaces
{
    public interface INotificationRepository
    {
        Task<IList<NotificationEntity>> FindByRecipientId(int userId);
        Task<IList<NotificationEntity>> FindUnreadByRecipientId(int userId);
        Task<int> CountUnreadByRecipientId(int userId);
        Task<IList<NotificationEntity>> FindByType(string type);
        Task MarkAllRead(int userId);
        Task MarkSingleRead(int notifId);
        Task DeleteByNotifId(int id);
        Task<NotificationEntity> Create(NotificationEntity notif);
        Task<IList<NotificationEntity>> CreateBatch(
            IList<NotificationEntity> notifications);
    }
}