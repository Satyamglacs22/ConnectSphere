using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Follow.API.Entities
{
    [Index(nameof(BlockerId), nameof(BlockedId), IsUnique = true)]
    public class BlockEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BlockId { get; set; }

        [Required]
        public int BlockerId { get; set; }

        [Required]
        public int BlockedId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
