using System.ComponentModel.DataAnnotations;

namespace Comment.API.DTOs
{
    public class AddCommentDto
    {
        [Required]
        public int PostId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public string Content { get; set; } = string.Empty;

        // null = top-level comment, set = reply
        public int? ParentCommentId { get; set; }
    }
}