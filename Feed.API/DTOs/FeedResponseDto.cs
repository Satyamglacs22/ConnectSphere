namespace Feed.API.DTOs
{
    public class FeedResponseDto
    {
        public int FeedItemId { get; set; }
        public int UserId { get; set; }
        public int PostId { get; set; }
        public int AuthorId { get; set; }
        public bool IsSeen { get; set; }
        public DateTime CreatedAt { get; set; }

        // Post details (from Post API)
        public string? Content { get; set; }
        public string? MediaUrl { get; set; }
        public string? Hashtags { get; set; }
        public int LikeCount { get; set; }
        public int CommentCount { get; set; }
    }
}