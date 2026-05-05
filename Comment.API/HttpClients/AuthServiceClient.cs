namespace Comment.API.HttpClients
{
    public class AuthServiceClient
    {
        private readonly HttpClient _httpClient;

        public AuthServiceClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<UserResponse?> GetUserDetails(int userId)
        {
            try
            {
                return await _httpClient
                    .GetFromJsonAsync<UserResponse>($"/api/users/{userId}");
            }
            catch
            {
                return null;
            }
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
        public class UserResponse
        {
            [System.Text.Json.Serialization.JsonPropertyName("userId")]
            public int UserId { get; set; }

            [System.Text.Json.Serialization.JsonPropertyName("userName")]
            public string UserName { get; set; } = string.Empty;

            [System.Text.Json.Serialization.JsonPropertyName("fullName")]
            public string FullName { get; set; } = string.Empty;

            [System.Text.Json.Serialization.JsonPropertyName("avatarUrl")]
            public string AvatarUrl { get; set; } = string.Empty;
        }
    }
}