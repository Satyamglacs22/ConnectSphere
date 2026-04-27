using ConnectSphere.Tests.Helpers;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using Xunit;

namespace ConnectSphere.Tests.Tests
{
    public class CommentTests
    {
        private readonly ApiClient _client;
        private int _userId;
        private int _postId;

        public CommentTests()
        {
            _client = new ApiClient();
            SetupUserAndPost().Wait();
        }

        private async Task SetupUserAndPost()
        {
            var email = $"commenttest_{Guid.NewGuid():N}@test.com";
            var user = new
            {
                UserName = $"commenttestuser_{Guid.NewGuid():N}",
                FullName = "Comment Test User",
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

            var postResponse = await _client.PostAsync(
                "/api/posts",
                new
                {
                    UserId = _userId,
                    Content = "Post for comment testing",
                    Visibility = "PUBLIC"
                });
            var postJson = JObject.Parse(
                await postResponse.Content.ReadAsStringAsync());
            _postId = int.Parse(postJson["postId"]!.ToString());
        }

        // ── Test 1: Add Comment ────────────────────────────────────────────
        [Fact]
        public async Task AddComment_ValidData_ReturnsSuccess()
        {
            // Act
            var response = await _client.PostAsync(
                "/api/comments",
                TestData.CreateComment(_postId, _userId));

            // Assert
            response.StatusCode.Should().Be(
                System.Net.HttpStatusCode.OK);

            var json = JObject.Parse(
                await response.Content.ReadAsStringAsync());
            json["commentId"].Should().NotBeNull();
            json["content"].Should().NotBeNull();
        }

        // ── Test 2: Add Reply ──────────────────────────────────────────────
        [Fact]
        public async Task AddReply_ValidData_ReturnsSuccess()
        {
            // Add parent comment first
            var commentResponse = await _client.PostAsync(
                "/api/comments",
                TestData.CreateComment(_postId, _userId));
            var commentJson = JObject.Parse(
                await commentResponse.Content.ReadAsStringAsync());
            var commentId = int.Parse(
                commentJson["commentId"]!.ToString());

            // Act — add reply
            var response = await _client.PostAsync(
                "/api/comments",
                new
                {
                    PostId = _postId,
                    UserId = _userId,
                    Content = "This is a reply!",
                    ParentCommentId = commentId
                });

            // Assert
            response.StatusCode.Should().Be(
                System.Net.HttpStatusCode.OK);

            var json = JObject.Parse(
                await response.Content.ReadAsStringAsync());
            json["parentCommentId"]!.Value<int>()
                .Should().Be(commentId);
        }

        // ── Test 3: Get Comments By Post ───────────────────────────────────
        [Fact]
        public async Task GetCommentsByPost_ReturnsSuccess()
        {
            // Act
            var response = await _client.GetAsync(
                $"/api/comments/post/{_postId}");

            // Assert
            response.StatusCode.Should().Be(
                System.Net.HttpStatusCode.OK);
        }

        // ── Test 4: Edit Comment ───────────────────────────────────────────
        [Fact]
        public async Task EditComment_ValidData_ReturnsUpdated()
        {
            // Add comment
            var commentResponse = await _client.PostAsync(
                "/api/comments",
                TestData.CreateComment(_postId, _userId));
            var commentJson = JObject.Parse(
                await commentResponse.Content.ReadAsStringAsync());
            var commentId = commentJson["commentId"]!.ToString();

            // Act
            var response = await _client.PutAsync(
                $"/api/comments/{commentId}",
                new { Content = "Edited comment!" });

            // Assert
            response.StatusCode.Should().Be(
                System.Net.HttpStatusCode.OK);

            var json = JObject.Parse(
                await response.Content.ReadAsStringAsync());
            json["isEdited"]!.Value<bool>().Should().BeTrue();
        }

        // ── Test 5: Soft Delete Comment ────────────────────────────────────
        [Fact]
        public async Task DeleteComment_ValidId_ReturnsSuccess()
        {
            // Add comment
            var commentResponse = await _client.PostAsync(
                "/api/comments",
                TestData.CreateComment(_postId, _userId));
            var commentJson = JObject.Parse(
                await commentResponse.Content.ReadAsStringAsync());
            var commentId = commentJson["commentId"]!.ToString();

            // Act
            var response = await _client.DeleteAsync(
                $"/api/comments/{commentId}");

            // Assert
            response.StatusCode.Should().Be(
                System.Net.HttpStatusCode.OK);

            // Verify content is masked
            var getResponse = await _client.GetAsync(
                $"/api/comments/{commentId}");
            var getJson = JObject.Parse(
                await getResponse.Content.ReadAsStringAsync());
            getJson["content"]!.ToString()
                .Should().Be("This comment was deleted.");
        }
    }
}
