using System.ComponentModel.DataAnnotations;

namespace Post.API.DTOs
{
    public class CreatePostDto
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public string Content { get; set; } = string.Empty;

        // Optional media
        public string? MediaUrl { get; set; }

        // IMAGE, VIDEO, GIF
        public string? MediaType { get; set; }

        // PUBLIC, FOLLOWERS, PRIVATE
        public string Visibility { get; set; } = "PUBLIC";

        // Comma-separated hashtags: '#travel,#food'
        public string? Hashtags { get; set; }
    }
}