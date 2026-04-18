using System.ComponentModel.DataAnnotations;

namespace Auth.API.DTOs
{
    public class UpdateCounterDto
    {
        [Required]
        public string Field { get; set; } = string.Empty;  // FollowerCount, FollowingCount, PostCount

        [Required]
        public int Delta { get; set; }  // +1 or -1
    }
}