using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Post.API.Entities
{
    [Index(nameof(UserId), nameof(CreatedAt))]
    public class PostEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PostId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public string Content { get; set; } = string.Empty;

        public string? MediaUrl { get; set; }

        // IMAGE, VIDEO, GIF, or null
        [MaxLength(10)]
        public string? MediaType { get; set; }

        // PUBLIC, FOLLOWERS, PRIVATE
        [Required]
        [MaxLength(20)]
        public string Visibility { get; set; } = "PUBLIC";

        // Comma-separated: '#travel,#food'
        public string? Hashtags { get; set; }

        public int LikeCount { get; set; } = 0;

        public int CommentCount { get; set; } = 0;

        public int ShareCount { get; set; } = 0;

        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}