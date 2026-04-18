using Auth.API.Data;
using Auth.API.Entities;
using Auth.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Auth.API.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AuthDbContext _context;

        public UserRepository(AuthDbContext context)
        {
            _context = context;
        }

        public async Task<User?> FindByUserId(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<User?> FindByUserName(string name)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.UserName == name);
        }

        public async Task<User?> FindByEmail(string email)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<bool> ExistsByUserName(string name)
        {
            return await _context.Users
                .AnyAsync(u => u.UserName == name);
        }

        public async Task<IList<User>> SearchUsers(string q)
        {
            return await _context.Users
                .Where(u => u.IsActive &&
                    (EF.Functions.Like(u.UserName, $"%{q}%") ||
                     EF.Functions.Like(u.FullName, $"%{q}%")))
                .ToListAsync();
        }

        public async Task<IList<User>> FindAllActive()
        {
            return await _context.Users
                .Where(u => u.IsActive)
                .ToListAsync();
        }

        public async Task UpdateCounters(int id, string field, int delta)
        {
            // Dynamically update only the requested counter field
            if (field == "FollowerCount")
                await _context.Users
                    .Where(u => u.UserId == id)
                    .ExecuteUpdateAsync(s => s.SetProperty(u => u.FollowerCount, u => u.FollowerCount + delta));

            else if (field == "FollowingCount")
                await _context.Users
                    .Where(u => u.UserId == id)
                    .ExecuteUpdateAsync(s => s.SetProperty(u => u.FollowingCount, u => u.FollowingCount + delta));

            else if (field == "PostCount")
                await _context.Users
                    .Where(u => u.UserId == id)
                    .ExecuteUpdateAsync(s => s.SetProperty(u => u.PostCount, u => u.PostCount + delta));
        }

        public async Task<User> Create(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<User> Update(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task Save()
        {
            await _context.SaveChangesAsync();
        }
    }
}