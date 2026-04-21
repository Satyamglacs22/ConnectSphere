using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Follow.API.Entities
{
    // Composite unique index — one user can only follow another once
    [Index(nameof(FollowerId), nameof(FolloweeId), IsUnique = true)]
    // Index on FolloweeId for efficient follower list queries
    [Index(nameof(FolloweeId))]
    public class FollowEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int FollowId { get; set; }

        // The user who initiated the follow
        [Required]
        public int FollowerId { get; set; }

        // The user being followed
        [Required]
        public int FolloweeId { get; set; }

        // PENDING, ACCEPTED, REJECTED
        [Required]
        [MaxLength(10)]
        public string Status { get; set; } = "PENDING";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}