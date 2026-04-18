using System.ComponentModel.DataAnnotations;

namespace Post.API.DTOs
{
    public class IncrementCountDto
    {
        // LikeCount, CommentCount, ShareCount
        [Required]
        public string Field { get; set; } = string.Empty;

        // +1 or -1
        [Required]
        public int Delta { get; set; }
    }
}