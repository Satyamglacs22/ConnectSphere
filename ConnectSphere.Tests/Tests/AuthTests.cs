using ConnectSphere.Tests.Helpers;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using Xunit;

namespace ConnectSphere.Tests.Tests
{
    public class AuthTests
    {
        private readonly ApiClient _client;

        public AuthTests()
        {
            _client = new ApiClient();
        }

        // ── Test 1: Register ───────────────────────────────────────────────
        [Fact]
        public async Task Register_ValidUser_ReturnsSuccess()
        {
            // Arrange
            var user = TestData.User1;

            // Act
            var response = await _client.PostAsync(
                "/api/users/register", user);

            // Assert
            response.StatusCode.Should().Be(
                System.Net.HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            var json = JObject.Parse(content);

            json["userId"].Should().NotBeNull();
            json["userName"].Should().NotBeNull();
            json["email"].Should().NotBeNull();
        }

        // ── Test 2: Register Duplicate ─────────────────────────────────────
        [Fact]
        public async Task Register_DuplicateEmail_ReturnsBadRequest()
        {
            // Arrange — register first time
            var user = new
            {
                UserName = $"dupuser_{Guid.NewGuid():N}",
                FullName = "Duplicate User",
                Email = "duplicate@test.com",
                Password = "Password123!"
            };

            await _client.PostAsync("/api/users/register", user);

            // Act — register again with same email
            var user2 = new
            {
                UserName = $"dupuser2_{Guid.NewGuid():N}",
                FullName = "Duplicate User 2",
                Email = "duplicate@test.com",
                Password = "Password123!"
            };

            var response = await _client.PostAsync(
                "/api/users/register", user2);

            // Assert
            response.StatusCode.Should().Be(
                System.Net.HttpStatusCode.BadRequest);
        }

        // ── Test 3: Login ──────────────────────────────────────────────────
        [Fact]
        public async Task Login_ValidCredentials_ReturnsToken()
        {
            // Arrange — register first
            var email = $"login_{Guid.NewGuid():N}@test.com";
            var user = new
            {
                UserName = $"loginuser_{Guid.NewGuid():N}",
                FullName = "Login User",
                Email = email,
                Password = "Password123!"
            };

            await _client.PostAsync("/api/users/register", user);

            // Act
            var loginResponse = await _client.PostAsync(
                "/api/users/login",
                new { Email = email, Password = "Password123!" });

            // Assert
            loginResponse.StatusCode.Should().Be(
                System.Net.HttpStatusCode.OK);

            var content = await loginResponse.Content.ReadAsStringAsync();
            var json = JObject.Parse(content);

            json["token"].Should().NotBeNull();
            json["token"]!.ToString().Should().NotBeEmpty();
        }

        // ── Test 4: Login Wrong Password ───────────────────────────────────
        [Fact]
        public async Task Login_WrongPassword_ReturnsUnauthorized()
        {
            // Arrange
            var email = $"wrongpass_{Guid.NewGuid():N}@test.com";
            var user = new
            {
                UserName = $"wrongpassuser_{Guid.NewGuid():N}",
                FullName = "Wrong Pass User",
                Email = email,
                Password = "Password123!"
            };

            await _client.PostAsync("/api/users/register", user);

            // Act
            var response = await _client.PostAsync(
                "/api/users/login",
                new { Email = email, Password = "WrongPassword!" });

            // Assert
            response.StatusCode.Should().Be(
                System.Net.HttpStatusCode.Unauthorized);
        }

        // ── Test 5: Get User By Id ─────────────────────────────────────────
        [Fact]
        public async Task GetUser_ValidId_ReturnsUser()
        {
            // Arrange — register and login
            var email = $"getuser_{Guid.NewGuid():N}@test.com";
            var user = new
            {
                UserName = $"getusertest_{Guid.NewGuid():N}",
                FullName = "Get User Test",
                Email = email,
                Password = "Password123!"
            };

            var registerResponse = await _client.PostAsync(
                "/api/users/register", user);
            var registerJson = JObject.Parse(
                await registerResponse.Content.ReadAsStringAsync());
            var userId = registerJson["userId"]!.ToString();

            // Act
            var response = await _client.GetAsync(
                $"/api/users/{userId}");

            // Assert
            response.StatusCode.Should().Be(
                System.Net.HttpStatusCode.OK);

            var content = JObject.Parse(
                await response.Content.ReadAsStringAsync());
            content["userId"].Should().NotBeNull();
        }
    }
}
