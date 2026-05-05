using System.ComponentModel.DataAnnotations;

namespace Auth.API.DTOs
{
    public class UpdateProfileDto
    {
        [MaxLength(100)]
        public string? FullName { get; set; }

        [MaxLength(300)]
        public string? Bio { get; set; }

        // Accepts a data URL (base64) for local dev OR a CDN URL for production
        public string? AvatarUrl { get; set; }
    }
}