using ConnectSphere.Tests.Helpers;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace ConnectSphere.Tests.Tests
{
    [TestFixture]
    public class AuthTests
    {
        private ApiClient _client = null!;

        [SetUp]
        public void SetUp()
        {
            _client = new ApiClient();
        }

        // ── Test 1: Register ───────────────────────────────────────────────
        [Test]
        public async Task Register_ValidUser_ReturnsSuccess()
        {
            var user = TestData.User1;

            var response = await _client.PostAsync("/api/users/register", user);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

            var json = JObject.Parse(await response.Content.ReadAsStringAsync());
            json["userId"].Should().NotBeNull();
            json["userName"].Should().NotBeNull();
            json["email"].Should().NotBeNull();
        }

        // ── Test 2: Register Duplicate ─────────────────────────────────────
        [Test]
        public async Task Register_DuplicateEmail_ReturnsBadRequest()
        {
            var user = new
            {
                UserName = $"dupuser_{Guid.NewGuid():N}",
                FullName = "Duplicate User",
                Email = "duplicate@test.com",
                Password = "Password123!"
            };

            await _client.PostAsync("/api/users/register", user);

            var user2 = new
            {
                UserName = $"dupuser2_{Guid.NewGuid():N}",
                FullName = "Duplicate User 2",
                Email = "duplicate@test.com",
                Password = "Password123!"
            };

            var response = await _client.PostAsync("/api/users/register", user2);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        }

        // ── Test 3: Login ──────────────────────────────────────────────────
        [Test]
        public async Task Login_ValidCredentials_ReturnsToken()
        {
            var email = $"login_{Guid.NewGuid():N}@test.com";
            var user = new
            {
                UserName = $"loginuser_{Guid.NewGuid():N}",
                FullName = "Login User",
                Email = email,
                Password = "Password123!"
            };

            await _client.PostAsync("/api/users/register", user);

            var loginResponse = await _client.PostAsync(
                "/api/users/login",
                new { Email = email, Password = "Password123!" });

            loginResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

            var json = JObject.Parse(await loginResponse.Content.ReadAsStringAsync());
            json["token"].Should().NotBeNull();
            json["token"]!.ToString().Should().NotBeEmpty();
        }

        // ── Test 4: Login Wrong Password ───────────────────────────────────
        [Test]
        public async Task Login_WrongPassword_ReturnsUnauthorized()
        {
            var email = $"wrongpass_{Guid.NewGuid():N}@test.com";
            var user = new
            {
                UserName = $"wrongpassuser_{Guid.NewGuid():N}",
                FullName = "Wrong Pass User",
                Email = email,
                Password = "Password123!"
            };

            await _client.PostAsync("/api/users/register", user);

            var response = await _client.PostAsync(
                "/api/users/login",
                new { Email = email, Password = "WrongPassword!" });

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
        }

        // ── Test 5: Get User By Id ─────────────────────────────────────────
        [Test]
        public async Task GetUser_ValidId_ReturnsUser()
        {
            var email = $"getuser_{Guid.NewGuid():N}@test.com";
            var user = new
            {
                UserName = $"getusertest_{Guid.NewGuid():N}",
                FullName = "Get User Test",
                Email = email,
                Password = "Password123!"
            };

            var registerResponse = await _client.PostAsync("/api/users/register", user);
            var registerJson = JObject.Parse(await registerResponse.Content.ReadAsStringAsync());
            var userId = registerJson["userId"]!.ToString();

            var response = await _client.GetAsync($"/api/users/{userId}");

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

            var content = JObject.Parse(await response.Content.ReadAsStringAsync());
            content["userId"].Should().NotBeNull();
        }
    }
}
