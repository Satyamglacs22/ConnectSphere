using System.ComponentModel.DataAnnotations;

namespace Comment.API.DTOs
{
    public class IncrementCountDto
    {
        [Required]
        public string Field { get; set; } = string.Empty;

        [Required]
        public int Delta { get; set; }
    }
}