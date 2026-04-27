using ConnectSphere.Tests.Helpers;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using Xunit;

namespace ConnectSphere.Tests.Tests
{
    public class EndToEndTests
    {
        private readonly ApiClient _client;

        public EndToEndTests()
        {
            _client = new ApiClient();
        }

        // ── Full Social Flow Test ──────────────────────────────────────────
        [Fact]
        public async Task FullSocialFlow_RegisterFollowPostLikeComment()
        {
            // ── Step 1: Register User 1 ────────────────────────────────────
            var email1 = $"e2e1_{Guid.NewGuid():N}@test.com";
            var user1 = new
            {
                UserName = $"e2euser1_{Guid.NewGuid():N}",
                FullName = "E2E User One",
                Email = email1,
                Password = "Password123!"
            };

            var reg1 = await _client.PostAsync(
                "/api/users/register", user1);
            reg1.StatusCode.Should().Be(
                System.Net.HttpStatusCode.OK);

            var reg1Json = JObject.Parse(
                await reg1.Content.ReadAsStringAsync());
            var user1Id = int.Parse(reg1Json["userId"]!.ToString());

            // ── Step 2: Register User 2 ────────────────────────────────────
            var email2 = $"e2e2_{Guid.NewGuid():N}@test.com";
            var user2 = new
            {
                UserName = $"e2euser2_{Guid.NewGuid():N}",
                FullName = "E2E User Two",
                Email = email2,
                Password = "Password123!"
            };

            var reg2 = await _client.PostAsync(
                "/api/users/register", user2);
            var reg2Json = JObject.Parse(
                await reg2.Content.ReadAsStringAsync());
            var user2Id = int.Parse(reg2Json["userId"]!.ToString());

            // ── Step 3: Login as User 1 ────────────────────────────────────
            var login1 = await _client.PostAsync(
                "/api/users/login",
                new { Email = email1, Password = "Password123!" });
            var login1Json = JObject.Parse(
                await login1.Content.ReadAsStringAsync());
            var token1 = login1Json["token"]!.ToString();
            _client.SetToken(token1);

            // ── Step 4: User 1 follows User 2 ─────────────────────────────
            var followResponse = await _client.PostAsync(
                "/api/follows",
                new { FollowerId = user1Id, FolloweeId = user2Id });
            followResponse.StatusCode.Should().Be(
                System.Net.HttpStatusCode.OK);

            // ── Step 5: Login as User 2 ────────────────────────────────────
            var login2 = await _client.PostAsync(
                "/api/users/login",
                new { Email = email2, Password = "Password123!" });
            var login2Json = JObject.Parse(
                await login2.Content.ReadAsStringAsync());
            _client.SetToken(login2Json["token"]!.ToString());

            // ── Step 6: User 2 creates a Post ─────────────────────────────
            var postResponse = await _client.PostAsync(
                "/api/posts",
                new
                {
                    UserId = user2Id,
                    Content = "E2E test post!",
                    Visibility = "PUBLIC"
                });
            postResponse.StatusCode.Should().Be(
                System.Net.HttpStatusCode.OK);

            var postJson = JObject.Parse(
                await postResponse.Content.ReadAsStringAsync());
            var postId = int.Parse(postJson["postId"]!.ToString());

            // ── Step 7: Login as User 1 ────────────────────────────────────
            _client.SetToken(token1);

            // ── Step 8: User 1 likes User 2's post ────────────────────────
            var likeResponse = await _client.PostAsync(
                "/api/likes/toggle",
                new
                {
                    UserId = user1Id,
                    TargetId = postId,
                    TargetType = "POST"
                });
            likeResponse.StatusCode.Should().Be(
                System.Net.HttpStatusCode.OK);

            var likeJson = JObject.Parse(
                await likeResponse.Content.ReadAsStringAsync());
            likeJson["liked"]!.Value<bool>().Should().BeTrue();

            // ── Step 9: User 1 comments on User 2's post ──────────────────
            var commentResponse = await _client.PostAsync(
                "/api/comments",
                new
                {
                    PostId = postId,
                    UserId = user1Id,
                    Content = "Great post!",
                    ParentCommentId = (int?)null
                });
            commentResponse.StatusCode.Should().Be(
                System.Net.HttpStatusCode.OK);

            // ── Step 10: Verify post has likes and comments ────────────────
            var getPostResponse = await _client.GetAsync(
                $"/api/posts/{postId}");
            var getPostJson = JObject.Parse(
                await getPostResponse.Content.ReadAsStringAsync());

            getPostJson["likeCount"]!.Value<int>()
                .Should().BeGreaterThan(0);
            getPostJson["commentCount"]!.Value<int>()
                .Should().BeGreaterThan(0);
        }
    }
}
