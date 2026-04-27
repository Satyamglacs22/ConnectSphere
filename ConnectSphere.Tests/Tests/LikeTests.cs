using ConnectSphere.Tests.Helpers;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using Xunit;

namespace ConnectSphere.Tests.Tests
{
    public class LikeTests
    {
        private readonly ApiClient _client;
        private int _userId;
        private int _postId;

        public LikeTests()
        {
            _client = new ApiClient();
            SetupUserAndPost().Wait();
        }

        private async Task SetupUserAndPost()
        {
            // Register and login
            var email = $"liketest_{Guid.NewGuid():N}@test.com";
            var user = new
            {
                UserName = $"liketestuser_{Guid.NewGuid():N}",
                FullName = "Like Test User",
                Email = email,
                Password = "Password123!"
            };

            var registerResponse = await _client.PostAsync(
                "/api/users/register", user);
            var registerJson = JObject.Parse(
                await registerResponse.Content.ReadAsStringAsync());
            _userId = int.Parse(registerJson["userId"]!.ToString());

            var loginResponse = await _client.PostAsync(
                "/api/users/login",
                new { Email = email, Password = "Password123!" });
            var loginJson = JObject.Parse(
                await loginResponse.Content.ReadAsStringAsync());
            _client.SetToken(loginJson["token"]!.ToString());

            // Create post
            var postResponse = await _client.PostAsync(
                "/api/posts",
                new
                {
                    UserId = _userId,
                    Content = "Post for like testing",
                    Visibility = "PUBLIC"
                });
            var postJson = JObject.Parse(
                await postResponse.Content.ReadAsStringAsync());
            _postId = int.Parse(postJson["postId"]!.ToString());
        }

        // ── Test 1: Toggle Like ────────────────────────────────────────────
        [Fact]
        public async Task ToggleLike_FirstTime_ReturnsLiked()
        {
            // Act
            var response = await _client.PostAsync(
                "/api/likes/toggle",
                TestData.ToggleLike(_userId, _postId));

            // Assert
            response.StatusCode.Should().Be(
                System.Net.HttpStatusCode.OK);

            var json = JObject.Parse(
                await response.Content.ReadAsStringAsync());
            json["liked"]!.Value<bool>().Should().BeTrue();
        }

        // ── Test 2: Toggle Like Twice = Unlike ─────────────────────────────
        [Fact]
        public async Task ToggleLike_Twice_ReturnsUnliked()
        {
            // Like first
            await _client.PostAsync(
                "/api/likes/toggle",
                TestData.ToggleLike(_userId, _postId));

            // Unlike
            var response = await _client.PostAsync(
                "/api/likes/toggle",
                TestData.ToggleLike(_userId, _postId));

            // Assert
            var json = JObject.Parse(
                await response.Content.ReadAsStringAsync());
            json["liked"]!.Value<bool>().Should().BeFalse();
        }

        // ── Test 3: Get Like Count ─────────────────────────────────────────
        [Fact]
        public async Task GetLikeCount_ReturnsCount()
        {
            // Act
            var response = await _client.GetAsync(
                $"/api/likes/count/{_postId}/POST");

            // Assert
            response.StatusCode.Should().Be(
                System.Net.HttpStatusCode.OK);

            var json = JObject.Parse(
                await response.Content.ReadAsStringAsync());
            json["likeCount"].Should().NotBeNull();
        }

        // ── Test 4: Has User Liked ─────────────────────────────────────────
        [Fact]
        public async Task HasLiked_AfterLike_ReturnsTrue()
        {
            // Like first
            await _client.PostAsync(
                "/api/likes/toggle",
                TestData.ToggleLike(_userId, _postId));

            // Act
            var response = await _client.GetAsync(
                $"/api/likes/hasLiked/{_userId}/{_postId}/POST");

            // Assert
            var json = JObject.Parse(
                await response.Content.ReadAsStringAsync());
            json["hasLiked"]!.Value<bool>().Should().BeTrue();
        }
    }
}
