using System.ComponentModel.DataAnnotations;

namespace Follow.API.DTOs
{
    public class FollowRequestDto
    {
        [Required]
        public int FollowerId { get; set; }

        [Required]
        public int FolloweeId { get; set; }
    }
}