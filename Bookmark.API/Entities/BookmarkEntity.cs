using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Bookmark.API.Entities
{
    [Index(nameof(UserId), nameof(PostId), IsUnique = true)]
    public class BookmarkEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BookmarkId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int PostId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
