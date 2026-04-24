using System.ComponentModel.DataAnnotations;

namespace Notification.API.DTOs
{
    public class CommentNotificationDto
    {
        [Required]
        public int PostAuthorId { get; set; }

        [Required]
        public int ActorId { get; set; }

        [Required]
        public int PostId { get; set; }
    }
}