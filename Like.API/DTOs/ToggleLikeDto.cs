using System.ComponentModel.DataAnnotations;

namespace Like.API.DTOs
{
    public class ToggleLikeDto
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public int TargetId { get; set; }

        // POST or COMMENT
        [Required]
        public string TargetType { get; set; } = string.Empty;
    }
}