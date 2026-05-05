using System.Net.Http.Json;

namespace Notification.API.HttpClients
{
    public class AuthServiceClient
    {
        private readonly HttpClient _httpClient;

        public AuthServiceClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> GetUserName(int userId)
        {
            try
            {
                var user = await GetUserDetails(userId);
                if (user == null) return $"User {userId}";

                if (!string.IsNullOrWhiteSpace(user.FullName)) return user.FullName;
                if (!string.IsNullOrWhiteSpace(user.UserName)) return user.UserName;

                return $"User {userId}";
            }
            catch
            {
                return $"User {userId}";
            }
        }

        public async Task<UserDto?> GetUserDetails(int userId)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<UserDto>($"/api/users/{userId}");
            }
            catch
            {
                return null;
            }
        }

        public class UserDto
        {
            [System.Text.Json.Serialization.JsonPropertyName("userName")]
            public string UserName { get; set; } = string.Empty;

            [System.Text.Json.Serialization.JsonPropertyName("fullName")]
            public string FullName { get; set; } = string.Empty;

            [System.Text.Json.Serialization.JsonPropertyName("avatarUrl")]
            public string AvatarUrl { get; set; } = string.Empty;
        }
    }
}
