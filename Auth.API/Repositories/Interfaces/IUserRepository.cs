using Auth.API.Entities;

namespace Auth.API.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> FindByUserId(int id);
        Task<User?> FindByUserName(string name);
        Task<User?> FindByEmail(string email);
        Task<bool> ExistsByUserName(string name);
        Task<IList<User>> SearchUsers(string q);
        Task<IList<User>> FindAllActive();
        Task UpdateCounters(int id, string field, int delta);
        Task<User> Create(User user);
        Task<User> Update(User user);
        Task Save();
    }
}