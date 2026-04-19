namespace Like.API.DTOs
{
    public class ToggleLikeResponseDto
    {
        // true = liked, false = unliked
        public bool Liked { get; set; }
        public int LikeCount { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}