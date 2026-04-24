using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Notification.API.Entities
{
    // Index on RecipientId + IsRead for efficient unread queries
    [Index(nameof(RecipientId), nameof(IsRead))]
    public class NotificationEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int NotificationId { get; set; }

        // Who receives the notification
        [Required]
        public int RecipientId { get; set; }

        // Who triggered the notification
        [Required]
        public int ActorId { get; set; }

        // LIKE_POST, LIKE_COMMENT, NEW_COMMENT, NEW_REPLY,
        // NEW_FOLLOWER, FOLLOW_REQUEST, FOLLOW_ACCEPTED,
        // MENTION, PLATFORM
        [Required]
        [MaxLength(20)]
        public string Type { get; set; } = string.Empty;

        [Required]
        public string Message { get; set; } = string.Empty;

        // PostId, CommentId, or UserId
        public int TargetId { get; set; }

        // POST, COMMENT, USER
        [MaxLength(10)]
        public string TargetType { get; set; } = string.Empty;

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}