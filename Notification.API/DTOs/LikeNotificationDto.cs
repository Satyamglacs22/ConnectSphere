using System.ComponentModel.DataAnnotations;

namespace Notification.API.DTOs
{
    public class LikeNotificationDto
    {
        [Required]
        public int RecipientId { get; set; }

        [Required]
        public int ActorId { get; set; }

        [Required]
        public int TargetId { get; set; }

        [Required]
        public string TargetType { get; set; } = string.Empty;
        // POST or COMMENT
    }
}