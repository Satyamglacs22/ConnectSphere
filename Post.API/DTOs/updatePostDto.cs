using System.ComponentModel.DataAnnotations;

namespace Post.API.DTOs
{
    public class UpdatePostDto
    {
        [Required]
        public string Content { get; set; } = string.Empty;

        public string? Hashtags { get; set; }

        public string Visibility { get; set; } = "PUBLIC";
    }
}