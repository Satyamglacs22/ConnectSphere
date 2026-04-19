namespace Like.API.HttpClients
{
    // Calls Post.API to increment/decrement LikeCount on a post
    public class PostServiceClient
    {
        private readonly HttpClient _httpClient;

        public PostServiceClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task UpdateLikeCount(int postId, int delta)
        {
            var payload = new
            {
                Field = "LikeCount",
                Delta = delta
            };

            var response = await _httpClient.PutAsJsonAsync(
                $"/api/posts/{postId}/counts", payload);

            response.EnsureSuccessStatusCode();
        }
    }
}