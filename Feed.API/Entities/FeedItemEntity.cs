using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Feed.API.Entities
{
    // Index on UserId + CreatedAt for efficient feed queries
    [Index(nameof(UserId), nameof(CreatedAt))]
    public class FeedItemEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int FeedItemId { get; set; }

        // Owner of this feed item
        [Required]
        public int UserId { get; set; }

        // Post that appeared in feed
        [Required]
        public int PostId { get; set; }

        // Who created the post
        [Required]
        public int AuthorId { get; set; }

        public bool IsSeen { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}