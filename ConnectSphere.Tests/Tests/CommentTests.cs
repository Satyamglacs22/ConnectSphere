using ConnectSphere.Tests.Helpers;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace ConnectSphere.Tests.Tests
{
    [TestFixture]
    public class CommentTests
    {
        private ApiClient _client = null!;
        private int _userId;
        private int _postId;

        [SetUp]
        public async Task SetUp()
        {
            _client = new ApiClient();

            var email = $"commenttest_{Guid.NewGuid():N}@test.com";
            var user = new
            {
                UserName = $"commenttestuser_{Guid.NewGuid():N}",
                FullName = "Comment Test User",
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
                new { UserId = _userId, Content = "Post for comment testing", Visibility = "PUBLIC" });
            var postJson = JObject.Parse(await postResponse.Content.ReadAsStringAsync());
            _postId = int.Parse(postJson["postId"]!.ToString());
        }

        // ── Test 1: Add Comment ────────────────────────────────────────────
        [Test]
        public async Task AddComment_ValidData_ReturnsSuccess()
        {
            var response = await _client.PostAsync("/api/comments", TestData.CreateComment(_postId, _userId));

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

            var json = JObject.Parse(await response.Content.ReadAsStringAsync());
            json["commentId"].Should().NotBeNull();
            json["content"].Should().NotBeNull();
        }

        // ── Test 2: Add Reply ──────────────────────────────────────────────
        [Test]
        public async Task AddReply_ValidData_ReturnsSuccess()
        {
            var commentResponse = await _client.PostAsync("/api/comments", TestData.CreateComment(_postId, _userId));
            var commentJson = JObject.Parse(await commentResponse.Content.ReadAsStringAsync());
            var commentId = int.Parse(commentJson["commentId"]!.ToString());

            var response = await _client.PostAsync(
                "/api/comments",
                new { PostId = _postId, UserId = _userId, Content = "This is a reply!", ParentCommentId = commentId });

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

            var json = JObject.Parse(await response.Content.ReadAsStringAsync());
            json["parentCommentId"]!.Value<int>().Should().Be(commentId);
        }

        // ── Test 3: Get Comments By Post ───────────────────────────────────
        [Test]
        public async Task GetCommentsByPost_ReturnsSuccess()
        {
            var response = await _client.GetAsync($"/api/comments/post/{_postId}");

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }

        // ── Test 4: Edit Comment ───────────────────────────────────────────
        [Test]
        public async Task EditComment_ValidData_ReturnsUpdated()
        {
            var commentResponse = await _client.PostAsync("/api/comments", TestData.CreateComment(_postId, _userId));
            var commentJson = JObject.Parse(await commentResponse.Content.ReadAsStringAsync());
            var commentId = commentJson["commentId"]!.ToString();

            var response = await _client.PutAsync($"/api/comments/{commentId}", new { Content = "Edited comment!" });

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

            var json = JObject.Parse(await response.Content.ReadAsStringAsync());
            json["isEdited"]!.Value<bool>().Should().BeTrue();
        }

        // ── Test 5: Soft Delete Comment ────────────────────────────────────
        [Test]
        public async Task DeleteComment_ValidId_ReturnsSuccess()
        {
            var commentResponse = await _client.PostAsync("/api/comments", TestData.CreateComment(_postId, _userId));
            var commentJson = JObject.Parse(await commentResponse.Content.ReadAsStringAsync());
            var commentId = commentJson["commentId"]!.ToString();

            var response = await _client.DeleteAsync($"/api/comments/{commentId}");

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

            // Verify via the post's comment list that the endpoint still succeeds
            var getResponse = await _client.GetAsync($"/api/comments/post/{_postId}");
            getResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
            var responseBody = await getResponse.Content.ReadAsStringAsync();
            responseBody.Should().NotBeNull();
        }
    }
}
