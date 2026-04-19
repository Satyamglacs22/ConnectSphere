using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Like.API.Entities
{
    // Composite unique index — one user can only like a target once
    [Index(nameof(UserId), nameof(TargetId), nameof(TargetType), IsUnique = true)]
    public class LikeEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int LikeId { get; set; }

        // The user who liked
        [Required]
        public int UserId { get; set; }

        // PostId or CommentId — depends on TargetType
        [Required]
        public int TargetId { get; set; }

        // POST or COMMENT
        [Required]
        [MaxLength(10)]
        public string TargetType { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}