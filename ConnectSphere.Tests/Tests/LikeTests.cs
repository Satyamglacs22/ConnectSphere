using ConnectSphere.Tests.Helpers;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace ConnectSphere.Tests.Tests
{
    [TestFixture]
    public class LikeTests
    {
        private ApiClient _client = null!;
        private int _userId;
        private int _postId;

        [SetUp]
        public async Task SetUp()
        {
            _client = new ApiClient();

            var email = $"liketest_{Guid.NewGuid():N}@test.com";
            var user = new
            {
                UserName = $"liketestuser_{Guid.NewGuid():N}",
                FullName = "Like Test User",
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

            var postResponse = await _client.PostAsync(
                "/api/posts",
                new { UserId = _userId, Content = "Post for like testing", Visibility = "PUBLIC" });
            var postJson = JObject.Parse(await postResponse.Content.ReadAsStringAsync());
            _postId = int.Parse(postJson["postId"]!.ToString());
        }

        // ── Test 1: Toggle Like ────────────────────────────────────────────
        [Test]
        public async Task ToggleLike_FirstTime_ReturnsLiked()
        {
            var response = await _client.PostAsync("/api/likes/toggle", TestData.ToggleLike(_userId, _postId));

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

            var json = JObject.Parse(await response.Content.ReadAsStringAsync());
            json["liked"]!.Value<bool>().Should().BeTrue();
        }

        // ── Test 2: Toggle Like Twice = Unlike ─────────────────────────────
        [Test]
        public async Task ToggleLike_Twice_ReturnsUnliked()
        {
            await _client.PostAsync("/api/likes/toggle", TestData.ToggleLike(_userId, _postId));

            var response = await _client.PostAsync("/api/likes/toggle", TestData.ToggleLike(_userId, _postId));

            var json = JObject.Parse(await response.Content.ReadAsStringAsync());
            json["liked"]!.Value<bool>().Should().BeFalse();
        }

        // ── Test 3: Get Like Count ─────────────────────────────────────────
        [Test]
        public async Task GetLikeCount_ReturnsCount()
        {
            var response = await _client.GetAsync($"/api/likes/count/{_postId}/POST");

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

            var json = JObject.Parse(await response.Content.ReadAsStringAsync());
            json["likeCount"].Should().NotBeNull();
        }

        // ── Test 4: Has User Liked ─────────────────────────────────────────
        [Test]
        public async Task HasLiked_AfterLike_ReturnsTrue()
        {
            await _client.PostAsync("/api/likes/toggle", TestData.ToggleLike(_userId, _postId));

            var response = await _client.GetAsync($"/api/likes/hasLiked/{_userId}/{_postId}/POST");

            var json = JObject.Parse(await response.Content.ReadAsStringAsync());
            json["hasLiked"]!.Value<bool>().Should().BeTrue();
        }
    }
}
