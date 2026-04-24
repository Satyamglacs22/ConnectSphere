namespace Feed.API.HttpClients
{
    public class FollowServiceClient
    {
        private readonly HttpClient _httpClient;

        public FollowServiceClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // Get all follower IDs of a user
        public async Task<List<int>> GetFollowerIds(int userId)
        {
            try
            {
                var response = await _httpClient
                    .GetFromJsonAsync<FollowerIdsResponse>(
                        $"/api/follows/{userId}/followerIds");

                return response?.FollowerIds ?? new List<int>();
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Warning: Failed to get follower IDs: {ex.Message}");
                return new List<int>();
            }
        }

        // Get all following IDs of a user
        public async Task<List<int>> GetFollowingIds(int userId)
        {
            try
            {
                var response = await _httpClient
                    .GetFromJsonAsync<FollowingIdsResponse>(
                        $"/api/follows/{userId}/followingIds");

                return response?.FollowingIds ?? new List<int>();
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Warning: Failed to get following IDs: {ex.Message}");
                return new List<int>();
            }
        }

        // Local response models
        private class FollowerIdsResponse
        {
            public int UserId { get; set; }
            public List<int> FollowerIds { get; set; } = new();
        }

        private class FollowingIdsResponse
        {
            public int UserId { get; set; }
            public List<int> FollowingIds { get; set; } = new();
        }
    }
}