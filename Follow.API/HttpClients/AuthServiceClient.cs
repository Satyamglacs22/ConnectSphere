namespace Follow.API.HttpClients
{
    public class AuthServiceClient
    {
        private readonly HttpClient _httpClient;

        public AuthServiceClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // Check if target user is private
        public async Task<bool> IsUserPrivate(int userId)
        {
            try
            {
                var response = await _httpClient
                    .GetFromJsonAsync<UserResponse>($"/api/users/{userId}");
                return response?.IsPrivate ?? false;
            }
            catch
            {
                return false;
            }
        }

        // Update FollowerCount or FollowingCount
        public async Task UpdateCounters(int userId, string field, int delta)
        {
            var payload = new
            {
                Field = field,
                Delta = delta
            };

            try
            {
                var response = await _httpClient.PutAsJsonAsync(
                    $"/api/users/{userId}/counters", payload);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Warning: Failed to update {field} for user {userId}: {ex.Message}");
            }
        }

        private class UserResponse
        {
            public int UserId { get; set; }
            public bool IsPrivate { get; set; }
        }
    }
}