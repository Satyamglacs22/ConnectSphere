namespace Comment.API.HttpClients
{
    public class AuthServiceClient
    {
        private readonly HttpClient _httpClient;

        public AuthServiceClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<int?> GetUserIdByUsername(string username)
        {
            try
            {
                var response = await _httpClient
                    .GetFromJsonAsync<UserResponse>($"/api/users/byUsername/{username}");
                return response?.UserId;
            }
            catch
            {
                return null;
            }
        }

        // Local response model
        private class UserResponse
        {
            public int UserId { get; set; }
        }
    }
}