namespace Feed.API.HttpClients
{
    public class PostServiceClient
    {
        private readonly HttpClient _httpClient;

        public PostServiceClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // Get post details by postId
        public async Task<PostResponse?> GetPostById(int postId)
        {
            try
            {
                return await _httpClient
                    .GetFromJsonAsync<PostResponse>(
                        $"/api/posts/{postId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Warning: Failed to get post: {ex.Message}");
                return null;
            }
        }

        // Get all posts by a user
        public async Task<List<PostResponse>> GetPostsByUser(int userId)
        {
            try
            {
                var posts = await _httpClient
                    .GetFromJsonAsync<List<PostResponse>>(
                        $"/api/posts/user/{userId}");

                return posts ?? new List<PostResponse>();
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Warning: Failed to get posts: {ex.Message}");
                return new List<PostResponse>();
            }
        }

        // Response model
        public class PostResponse
        {
            public int PostId { get; set; }
            public int UserId { get; set; }
            public string Content { get; set; } = string.Empty;
            public string? MediaUrl { get; set; }
            public string Visibility { get; set; } = string.Empty;
            public string? Hashtags { get; set; }
            public int LikeCount { get; set; }
            public int CommentCount { get; set; }
            public DateTime CreatedAt { get; set; }
        }
    }
}