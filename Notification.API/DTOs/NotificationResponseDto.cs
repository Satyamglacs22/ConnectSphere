namespace Notification.API.DTOs
{
    public class NotificationResponseDto
    {
        public int NotificationId { get; set; }
        public int RecipientId { get; set; }
        public int ActorId { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;

        // Dynamic fields for UI
        public string ActorName { get; set; } = string.Empty;
        public string ActorAvatarUrl { get; set; } = string.Empty;

        public int TargetId { get; set; }
        public string TargetType { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}