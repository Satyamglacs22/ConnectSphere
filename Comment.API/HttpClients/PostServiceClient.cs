namespace Comment.API.HttpClients
{
    public class PostServiceClient
    {
        private readonly HttpClient _httpClient;

        public PostServiceClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<PostResponse?> GetPostById(int postId)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<PostResponse>($"/api/posts/{postId}");
            }
            catch
            {
                return null;
            }
        }

        public async Task IncrementCommentCount(int postId, int delta)
        {
            var payload = new
            {
                Field = "CommentCount",
                Delta = delta
            };

            try
            {
                var response = await _httpClient.PutAsJsonAsync(
                    $"/api/posts/{postId}/counts", payload);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Failed to update CommentCount: {ex.Message}");
            }
        }

        public class PostResponse
        {
            public int PostId { get; set; }
            public int UserId { get; set; }
        }
    }
}