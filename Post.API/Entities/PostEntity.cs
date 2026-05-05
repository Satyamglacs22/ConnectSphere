using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace Post.API.Entities
{
    [Index(nameof(UserId), nameof(CreatedAt))]
    public class PostEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PostId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public string Content { get; set; } = string.Empty;

        public string? MediaUrl { get; set; }
        public string? AdditionalMediaUrls { get; set; }

        // IMAGE, VIDEO, GIF, or null
        [MaxLength(10)]
        public string? MediaType { get; set; }

        // PUBLIC, FOLLOWERS, PRIVATE
        [Required]
        [MaxLength(20)]
        public string Visibility { get; set; } = "PUBLIC";

        // Comma-separated: '#travel,#food'
        public string? Hashtags { get; set; }

        private int _likes = 0;
        public int LikeCount { get => _likes; set => _likes = Math.Max(0, value); }

        private int _comments = 0;
        public int CommentCount { get => _comments; set => _comments = Math.Max(0, value); }

        private int _shares = 0;
        public int ShareCount { get => _shares; set => _shares = Math.Max(0, value); }

        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        [NotMapped]
        [JsonPropertyName("mediaList")]
        public List<string> MediaList
        {
            get
            {
                var list = new List<string>();
                if (!string.IsNullOrEmpty(MediaUrl)) list.Add(MediaUrl);
                if (!string.IsNullOrEmpty(AdditionalMediaUrls))
                    list.AddRange(AdditionalMediaUrls.Split('|', StringSplitOptions.RemoveEmptyEntries));
                return list;
            }
            private set { }
        }
    }
}