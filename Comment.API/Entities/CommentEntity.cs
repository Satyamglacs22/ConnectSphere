using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Comment.API.Entities
{
    // Composite index for efficient GetTopLevelComments and GetReplies queries
    [Index(nameof(PostId), nameof(ParentCommentId))]
    public class CommentEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CommentId { get; set; }

        [Required]
        public int PostId { get; set; }

        [Required]
        public int UserId { get; set; }

        // null = top-level comment, set = reply to parent
        public int? ParentCommentId { get; set; }

        [Required]
        public string Content { get; set; } = string.Empty;

        public int LikeCount { get; set; } = 0;

        public int ReplyCount { get; set; } = 0;

        public bool IsDeleted { get; set; } = false;

        public bool IsEdited { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? EditedAt { get; set; }
    }
}