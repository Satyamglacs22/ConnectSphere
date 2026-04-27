using ConnectSphere.Tests.Helpers;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using Xunit;

namespace ConnectSphere.Tests.Tests
{
    public class FollowTests
    {
        private readonly ApiClient _client;
        private int _user1Id;
        private int _user2Id;

        public FollowTests()
        {
            _client = new ApiClient();
            SetupTwoUsers().Wait();
        }

        private async Task SetupTwoUsers()
        {
            // Register User 1
            var email1 = $"followtest1_{Guid.NewGuid():N}@test.com";
            var user1 = new
            {
                UserName = $"followtest1_{Guid.NewGuid():N}",
                FullName = "Follow Test User 1",
                Email = email1,
                Password = "Password123!"
            };

            var reg1 = await _client.PostAsync(
                "/api/users/register", user1);
            var reg1Json = JObject.Parse(
                await reg1.Content.ReadAsStringAsync());
            _user1Id = int.Parse(reg1Json["userId"]!.ToString());

            // Login as User 1
            var login1 = await _client.PostAsync(
                "/api/users/login",
                new { Email = email1, Password = "Password123!" });
            var login1Json = JObject.Parse(
                await login1.Content.ReadAsStringAsync());
            _client.SetToken(login1Json["token"]!.ToString());

            // Register User 2
            var email2 = $"followtest2_{Guid.NewGuid():N}@test.com";
            var user2 = new
            {
                UserName = $"followtest2_{Guid.NewGuid():N}",
                FullName = "Follow Test User 2",
                Email = email2,
                Password = "Password123!"
            };

            var reg2 = await _client.PostAsync(
                "/api/users/register", user2);
            var reg2Json = JObject.Parse(
                await reg2.Content.ReadAsStringAsync());
            _user2Id = int.Parse(reg2Json["userId"]!.ToString());
        }

        // ── Test 1: Follow User ────────────────────────────────────────────
        [Fact]
        public async Task FollowUser_ValidData_ReturnsAccepted()
        {
            // Act
            var response = await _client.PostAsync(
                "/api/follows",
                TestData.FollowRequest(_user1Id, _user2Id));

            // Assert
            response.StatusCode.Should().Be(
                System.Net.HttpStatusCode.OK);

            var json = JObject.Parse(
                await response.Content.ReadAsStringAsync());
            json["status"]!.ToString().Should().Be("ACCEPTED");
        }

        // ── Test 2: Follow Same User Twice ─────────────────────────────────
        [Fact]
        public async Task FollowUser_Twice_ReturnsAlreadyFollowing()
        {
            // Follow first time
            await _client.PostAsync(
                "/api/follows",
                TestData.FollowRequest(_user1Id, _user2Id));

            // Follow second time
            var response = await _client.PostAsync(
                "/api/follows",
                TestData.FollowRequest(_user1Id, _user2Id));

            // Assert
            var json = JObject.Parse(
                await response.Content.ReadAsStringAsync());
            json["message"]!.ToString()
                .Should().Contain("Already following");
        }

        // ── Test 3: Get Followers ──────────────────────────────────────────
        [Fact]
        public async Task GetFollowers_ReturnsFollowersList()
        {
            // Follow first
            await _client.PostAsync(
                "/api/follows",
                TestData.FollowRequest(_user1Id, _user2Id));

            // Act
            var response = await _client.GetAsync(
                $"/api/follows/{_user2Id}/followers");

            // Assert
            response.StatusCode.Should().Be(
                System.Net.HttpStatusCode.OK);
        }

        // ── Test 4: Is Following ───────────────────────────────────────────
        [Fact]
        public async Task IsFollowing_AfterFollow_ReturnsTrue()
        {
            // Follow first
            await _client.PostAsync(
                "/api/follows",
                TestData.FollowRequest(_user1Id, _user2Id));

            // Act
            var response = await _client.GetAsync(
                $"/api/follows/isFollowing/{_user1Id}/{_user2Id}");

            // Assert
            var json = JObject.Parse(
                await response.Content.ReadAsStringAsync());
            json["isFollowing"]!.Value<bool>().Should().BeTrue();
        }

        // ── Test 5: Unfollow ───────────────────────────────────────────────
        [Fact]
        public async Task Unfollow_AfterFollow_ReturnsSuccess()
        {
            // Follow first
            await _client.PostAsync(
                "/api/follows",
                TestData.FollowRequest(_user1Id, _user2Id));

            // Act
            var response = await _client.DeleteAsync(
                $"/api/follows/{_user2Id}");

            // Assert
            response.StatusCode.Should().Be(
                System.Net.HttpStatusCode.OK);
        }
    }
}
