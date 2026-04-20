namespace Comment.API.DTOs
{
    public class CommentResponseDto
    {
        public int CommentId { get; set; }
        public int PostId { get; set; }
        public int UserId { get; set; }
        public int? ParentCommentId { get; set; }

        // Masked if deleted
        public string Content { get; set; } = string.Empty;

        public int LikeCount { get; set; }
        public int ReplyCount { get; set; }
        public bool IsEdited { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? EditedAt { get; set; }
    }
}