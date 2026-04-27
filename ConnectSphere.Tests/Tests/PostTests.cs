using ConnectSphere.Tests.Helpers;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using Xunit;

namespace ConnectSphere.Tests.Tests
{
    public class PostTests
    {
        private readonly ApiClient _client;
        private string _token = string.Empty;
        private int _userId;

        public PostTests()
        {
            _client = new ApiClient();
            SetupUser().Wait();
        }

        private async Task SetupUser()
        {
            // Register
            var email = $"posttest_{Guid.NewGuid():N}@test.com";
            var user = new
            {
                UserName = $"posttestuser_{Guid.NewGuid():N}",
                FullName = "Post Test User",
                Email = email,
                Password = "Password123!"
            };

            var registerResponse = await _client.PostAsync(
                "/api/users/register", user);
            var registerJson = JObject.Parse(
                await registerResponse.Content.ReadAsStringAsync());
            _userId = int.Parse(registerJson["userId"]!.ToString());

            // Login
            var loginResponse = await _client.PostAsync(
                "/api/users/login",
                new { Email = email, Password = "Password123!" });
            var loginJson = JObject.Parse(
                await loginResponse.Content.ReadAsStringAsync());
            _token = loginJson["token"]!.ToString();
            _client.SetToken(_token);
        }

        // ── Test 1: Create Post ────────────────────────────────────────────
        [Fact]
        public async Task CreatePost_ValidData_ReturnsSuccess()
        {
            // Act
            var response = await _client.PostAsync(
                "/api/posts",
                TestData.CreatePost(_userId));

            // Assert
            response.StatusCode.Should().Be(
                System.Net.HttpStatusCode.OK);

            var content = JObject.Parse(
                await response.Content.ReadAsStringAsync());
            content["postId"].Should().NotBeNull();
            content["content"].Should().NotBeNull();
        }

        // ── Test 2: Get Post By Id ─────────────────────────────────────────
        [Fact]
        public async Task GetPost_ValidId_ReturnsPost()
        {
            // Arrange — create post first
            var createResponse = await _client.PostAsync(
                "/api/posts", TestData.CreatePost(_userId));
            var createJson = JObject.Parse(
                await createResponse.Content.ReadAsStringAsync());
            var postId = createJson["postId"]!.ToString();

            // Act
            var response = await _client.GetAsync(
                $"/api/posts/{postId}");

            // Assert
            response.StatusCode.Should().Be(
                System.Net.HttpStatusCode.OK);
        }

        // ── Test 3: Get Public Posts ───────────────────────────────────────
        [Fact]
        public async Task GetPublicPosts_ReturnsSuccess()
        {
            // Act
            var response = await _client.GetAsync("/api/posts/public");

            // Assert
            response.StatusCode.Should().Be(
                System.Net.HttpStatusCode.OK);
        }

        // ── Test 4: Update Post ────────────────────────────────────────────
        [Fact]
        public async Task UpdatePost_ValidData_ReturnsUpdated()
        {
            // Arrange — create post
            var createResponse = await _client.PostAsync(
                "/api/posts", TestData.CreatePost(_userId));
            var createJson = JObject.Parse(
                await createResponse.Content.ReadAsStringAsync());
            var postId = createJson["postId"]!.ToString();

            // Act
            var response = await _client.PutAsync(
                $"/api/posts/{postId}",
                new
                {
                    Content = "Updated content!",
                    Visibility = "PUBLIC",
                    Hashtags = "#updated"
                });

            // Assert
            response.StatusCode.Should().Be(
                System.Net.HttpStatusCode.OK);
        }

        // ── Test 5: Delete Post ────────────────────────────────────────────
        [Fact]
        public async Task DeletePost_ValidId_ReturnsSuccess()
        {
            // Arrange — create post
            var createResponse = await _client.PostAsync(
                "/api/posts", TestData.CreatePost(_userId));
            var createJson = JObject.Parse(
                await createResponse.Content.ReadAsStringAsync());
            var postId = createJson["postId"]!.ToString();

            // Act
            var response = await _client.DeleteAsync(
                $"/api/posts/{postId}");

            // Assert
            response.StatusCode.Should().Be(
                System.Net.HttpStatusCode.OK);
        }

        // ── Test 6: Get Trending Posts ─────────────────────────────────────
        [Fact]
        public async Task GetTrendingPosts_ReturnsSuccess()
        {
            // Act
            var response = await _client.GetAsync(
                "/api/posts/trending?topN=10");

            // Assert
            response.StatusCode.Should().Be(
                System.Net.HttpStatusCode.OK);
        }
    }
}
