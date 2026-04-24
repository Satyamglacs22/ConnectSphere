using System.ComponentModel.DataAnnotations;

namespace Notification.API.DTOs
{
    public class BroadcastNotificationDto
    {
        [Required]
        public IList<int> UserIds { get; set; } = new List<int>();

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Message { get; set; } = string.Empty;
    }
}