using System.ComponentModel.DataAnnotations;

namespace Notification.API.DTOs
{
    public class SendNotificationDto
    {
        [Required]
        public int RecipientId { get; set; }

        [Required]
        public int ActorId { get; set; }

        [Required]
        public string Type { get; set; } = string.Empty;

        [Required]
        public string Message { get; set; } = string.Empty;

        public int TargetId { get; set; }

        public string TargetType { get; set; } = string.Empty;
    }
}