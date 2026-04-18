namespace Post.API.HttpClients
{
    // Typed HTTP client — calls Auth.API to update PostCount
    public class AuthServiceClient
    {
        private readonly HttpClient _httpClient;

        public AuthServiceClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task UpdatePostCount(int userId, int delta)
        {
            var payload = new
            {
                Field = "PostCount",
                Delta = delta
            };

            var response = await _httpClient.PutAsJsonAsync(
                $"/api/users/{userId}/counters", payload);

            response.EnsureSuccessStatusCode();
        }
    }
}