using System.ComponentModel.DataAnnotations;

namespace Auth.API.DTOs
{
    public class UpdateProfileDto
    {
        [MaxLength(100)]
        public string? FullName { get; set; }

        [MaxLength(300)]
        public string? Bio { get; set; }
    }
}