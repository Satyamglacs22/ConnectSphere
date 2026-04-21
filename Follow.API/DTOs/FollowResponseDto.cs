namespace Follow.API.DTOs
{
    public class FollowResponseDto
    {
        public int FollowId { get; set; }
        public int FollowerId { get; set; }
        public int FolloweeId { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}