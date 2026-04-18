using Auth.API.Entities;

namespace Auth.API.Services.Interfaces
{
    public interface IUserService
    {
        Task<User> Register(string userName, string fullName, string email, string password);
        Task<string> Login(string email, string password);
        Task<User?> GetUserById(int id);
        Task<User?> GetUserByUserName(string name);
        Task<User> UpdateProfile(int id, string? fullName, string? bio);
        Task ChangePassword(int id, string currentPassword, string newPassword);
        Task<IList<User>> SearchUsers(string q);
        Task TogglePrivacy(int id);
        Task DeactivateAccount(int id);
        Task UpdateCounters(int id, string field, int delta);
        Task<string?> UploadAvatar(int id, IFormFile file);
    }
}