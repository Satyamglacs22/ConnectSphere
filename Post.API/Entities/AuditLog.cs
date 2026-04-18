using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Post.API.Entities
{
    public class AuditLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AuditLogId { get; set; }

        public int ActorId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Action { get; set; } = string.Empty;

        public int TargetId { get; set; }

        [Required]
        [MaxLength(50)]
        public string TargetType { get; set; } = string.Empty;

        public string? Before { get; set; }

        public string? After { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}