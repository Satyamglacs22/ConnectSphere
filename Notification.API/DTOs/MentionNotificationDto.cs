using System.ComponentModel.DataAnnotations;

namespace Notification.API.DTOs
{
    public class MentionNotificationDto
    {
        [Required]
        public int MentionedUserId { get; set; }

        [Required]
        public int ActorId { get; set; }

        [Required]
        public int PostId { get; set; }
    }
}