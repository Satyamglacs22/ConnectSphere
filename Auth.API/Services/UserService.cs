using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Auth.API.Entities;
using Auth.API.Repositories.Interfaces;
using Auth.API.Services.Interfaces;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Auth.API.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repo;
        private readonly IConfiguration _config;
        private readonly PasswordHasher<User> _hasher;

        public UserService(IUserRepository repo, IConfiguration config)
        {
            _repo = repo;
            _config = config;
            _hasher = new PasswordHasher<User>();
        }

        public async Task<User> Register(string userName, string fullName, string email, string password)
        {
            // Check duplicate username
            if (await _repo.ExistsByUserName(userName))
                throw new Exception("Username already taken.");

            // Check duplicate email
            var existing = await _repo.FindByEmail(email);
            if (existing != null)
                throw new Exception("Email already registered.");

            var user = new User
            {
                UserName = userName,
                FullName = fullName,
                Email = email,
                CreatedAt = DateTime.UtcNow
            };

            // Hash the password
            user.PasswordHash = _hasher.HashPassword(user, password);

            return await _repo.Create(user);
        }

        public async Task<string> Login(string email, string password)
        {
            var user = await _repo.FindByEmail(email);
            if (user == null || !user.IsActive)
                throw new Exception("Invalid credentials.");

            // Verify password
            var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, password);
            if (result == PasswordVerificationResult.Failed)
                throw new Exception("Invalid credentials.");

            return GenerateJwt(user);
        }

        public async Task<User?> GetUserById(int id)
        {
            return await _repo.FindByUserId(id);
        }

        public async Task<User?> GetUserByUserName(string name)
        {
            return await _repo.FindByUserName(name);
        }

        public async Task<User> UpdateProfile(int id, string? fullName, string? bio)
        {
            var user = await _repo.FindByUserId(id);
            if (user == null) throw new Exception("User not found.");

            if (fullName != null) user.FullName = fullName;
            if (bio != null) user.Bio = bio;

            return await _repo.Update(user);
        }

        public async Task ChangePassword(int id, string currentPassword, string newPassword)
        {
            var user = await _repo.FindByUserId(id);
            if (user == null) throw new Exception("User not found.");

            var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, currentPassword);
            if (result == PasswordVerificationResult.Failed)
                throw new Exception("Current password is incorrect.");

            user.PasswordHash = _hasher.HashPassword(user, newPassword);
            await _repo.Update(user);
        }

        public async Task<IList<User>> SearchUsers(string q)
        {
            return await _repo.SearchUsers(q);
        }

        public async Task TogglePrivacy(int id)
        {
            var user = await _repo.FindByUserId(id);
            if (user == null) throw new Exception("User not found.");

            user.IsPrivate = !user.IsPrivate;
            await _repo.Update(user);
        }

        public async Task DeactivateAccount(int id)
        {
            var user = await _repo.FindByUserId(id);
            if (user == null) throw new Exception("User not found.");

            user.IsActive = false;
            await _repo.Update(user);
        }

        public async Task UpdateCounters(int id, string field, int delta)
        {
            await _repo.UpdateCounters(id, field, delta);
        }

        public async Task<string?> UploadAvatar(int id, IFormFile file)
        {
            var user = await _repo.FindByUserId(id);
            if (user == null) throw new Exception("User not found.");

            var connStr = _config["Azure:BlobConnectionString"];
            var containerName = _config["Azure:AvatarContainer"] ?? "avatars";

            var blobClient = new BlobContainerClient(connStr, containerName);
            await blobClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

            var fileName = $"{id}-{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var blob = blobClient.GetBlobClient(fileName);

            using var stream = file.OpenReadStream();
            await blob.UploadAsync(stream, overwrite: true);

            user.AvatarUrl = blob.Uri.ToString();
            await _repo.Update(user);

            return user.AvatarUrl;
        }

        // ── Private: JWT Generator ──────────────────────────────────────────
        private string GenerateJwt(User user)
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("userName", user.UserName),
                new Claim(ClaimTypes.Role, "User")
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}