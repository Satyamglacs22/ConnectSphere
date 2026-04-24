using System.ComponentModel.DataAnnotations;

namespace Notification.API.DTOs
{
    public class FollowNotificationDto
    {
        [Required]
        public int TargetId { get; set; }

        [Required]
        public int FollowerId { get; set; }

        [Required]
        public string Type { get; set; } = string.Empty;
        // NEW_FOLLOWER, FOLLOW_REQUEST, FOLLOW_ACCEPTED
    }
}