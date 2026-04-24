using Microsoft.EntityFrameworkCore;
using Notification.API.Data;
using Notification.API.Entities;
using Notification.API.Repositories.Interfaces;

namespace Notification.API.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly NotificationDbContext _context;

        public NotificationRepository(NotificationDbContext context)
        {
            _context = context;
        }

        public async Task<IList<NotificationEntity>> FindByRecipientId(int userId)
        {
            return await _context.Notifications
                .Where(n => n.RecipientId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<IList<NotificationEntity>> FindUnreadByRecipientId(int userId)
        {
            return await _context.Notifications
                .Where(n => n.RecipientId == userId && !n.IsRead)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<int> CountUnreadByRecipientId(int userId)
        {
            return await _context.Notifications
                .CountAsync(n => n.RecipientId == userId && !n.IsRead);
        }

        public async Task<IList<NotificationEntity>> FindByType(string type)
        {
            return await _context.Notifications
                .Where(n => n.Type == type)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task MarkAllRead(int userId)
        {
            // Batch update — efficient ExecuteUpdateAsync
            await _context.Notifications
                .Where(n => n.RecipientId == userId && !n.IsRead)
                .ExecuteUpdateAsync(s => s.SetProperty(
                    n => n.IsRead, true));
        }

        public async Task MarkSingleRead(int notifId)
        {
            await _context.Notifications
                .Where(n => n.NotificationId == notifId)
                .ExecuteUpdateAsync(s => s.SetProperty(
                    n => n.IsRead, true));
        }

        public async Task DeleteByNotifId(int id)
        {
            await _context.Notifications
                .Where(n => n.NotificationId == id)
                .ExecuteDeleteAsync();
        }

        public async Task<NotificationEntity> Create(NotificationEntity notif)
        {
            _context.Notifications.Add(notif);
            await _context.SaveChangesAsync();
            return notif;
        }

        public async Task<IList<NotificationEntity>> CreateBatch(
            IList<NotificationEntity> notifications)
        {
            // Add all at once — efficient bulk insert
            await _context.Notifications.AddRangeAsync(notifications);
            await _context.SaveChangesAsync();
            return notifications;
        }
    }
}