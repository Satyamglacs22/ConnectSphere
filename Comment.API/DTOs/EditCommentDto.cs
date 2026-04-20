using System.ComponentModel.DataAnnotations;

namespace Comment.API.DTOs
{
    public class EditCommentDto
    {
        [Required]
        public string Content { get; set; } = string.Empty;
    }
}