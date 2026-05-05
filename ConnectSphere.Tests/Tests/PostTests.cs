using ConnectSphere.Tests.Helpers;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace ConnectSphere.Tests.Tests
{
    [TestFixture]
    public class PostTests
    {
        private ApiClient _client = null!;
        private int _userId;

        [SetUp]
        public async Task SetUp()
        {
            _client = new ApiClient();

            var email = $"posttest_{Guid.NewGuid():N}@test.com";
            var user = new
            {
                UserName = $"posttestuser_{Guid.NewGuid():N}",
                FullName = "Post Test User",
                Email = email,
                Password = "Password123!"
            };

            var registerResponse = await _client.PostAsync("/api/users/register", user);
            var registerJson = JObject.Parse(await registerResponse.Content.ReadAsStringAsync());
            _userId = int.Parse(registerJson["userId"]!.ToString());

            var loginResponse = await _client.PostAsync(
                "/api/users/login",
                new { Email = email, Password = "Password123!" });
            var loginJson = JObject.Parse(await loginResponse.Content.ReadAsStringAsync());
            _client.SetToken(loginJson["token"]!.ToString());
        }

        // ── Test 1: Create Post ────────────────────────────────────────────
        [Test]
        public async Task CreatePost_ValidData_ReturnsSuccess()
        {
            var response = await _client.PostAsync("/api/posts", TestData.CreatePost(_userId));

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

            var content = JObject.Parse(await response.Content.ReadAsStringAsync());
            content["postId"].Should().NotBeNull();
            content["content"].Should().NotBeNull();
        }

        // ── Test 2: Get Post By Id ─────────────────────────────────────────
        [Test]
        public async Task GetPost_ValidId_ReturnsPost()
        {
            var createResponse = await _client.PostAsync("/api/posts", TestData.CreatePost(_userId));
            var createJson = JObject.Parse(await createResponse.Content.ReadAsStringAsync());
            var postId = createJson["postId"]!.ToString();

            var response = await _client.GetAsync($"/api/posts/{postId}");

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }

        // ── Test 3: Get Public Posts ───────────────────────────────────────
        [Test]
        public async Task GetPublicPosts_ReturnsSuccess()
        {
            var response = await _client.GetAsync("/api/posts/public");

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }

        // ── Test 4: Update Post ────────────────────────────────────────────
        [Test]
        public async Task UpdatePost_ValidData_ReturnsUpdated()
        {
            var createResponse = await _client.PostAsync("/api/posts", TestData.CreatePost(_userId));
            var createJson = JObject.Parse(await createResponse.Content.ReadAsStringAsync());
            var postId = createJson["postId"]!.ToString();

            var response = await _client.PutAsync(
                $"/api/posts/{postId}",
                new { Content = "Updated content!", Visibility = "PUBLIC", Hashtags = "#updated" });

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }

        // ── Test 5: Delete Post ────────────────────────────────────────────
        [Test]
        public async Task DeletePost_ValidId_ReturnsSuccess()
        {
            var createResponse = await _client.PostAsync("/api/posts", TestData.CreatePost(_userId));
            var createJson = JObject.Parse(await createResponse.Content.ReadAsStringAsync());
            var postId = createJson["postId"]!.ToString();

            var response = await _client.DeleteAsync($"/api/posts/{postId}");

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }

        // ── Test 6: Get Trending Posts ─────────────────────────────────────
        [Test]
        public async Task GetTrendingPosts_ReturnsSuccess()
        {
            var response = await _client.GetAsync("/api/posts/trending?topN=10");

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }
    }
}
