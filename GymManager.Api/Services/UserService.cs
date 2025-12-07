using GymManager.Api.Data;
using GymManager.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GymManager.Api.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _db;
        public UserService(AppDbContext db) { _db = db; }

        public async Task<User> CreateUserAsync(User user, string plainPassword)
        {
            if (await _db.Users.AnyAsync(u => u.NationalCode == user.NationalCode))
                throw new Exception("National code already exists");

            user.Id = Guid.NewGuid();
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(plainPassword);
            user.CreatedAt = DateTime.UtcNow;
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            return user;
        }

        public async Task<User?> GetUserByIdAsync(Guid id)
        {
            return await _db.Users.Include(u => u.Gym).FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<IEnumerable<User>> GetUsersByGymAsync(Guid gymId, int page = 1, int pageSize = 20, string? search = null)
        {
            var q = _db.Users.Where(u => u.GymId == gymId);
            if (!string.IsNullOrEmpty(search))
                q = q.Where(u => u.FirstName.Contains(search) || u.LastName.Contains(search) || u.NationalCode.Contains(search));
            return await q.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        }

        public async Task UpdateUserAsync(User user)
        {
            var e = await _db.Users.FindAsync(user.Id);
            if (e == null) throw new Exception("User not found");
            e.FirstName = user.FirstName;
            e.LastName = user.LastName;
            e.Phone = user.Phone;
            e.IsActive = user.IsActive;
            e.Role = user.Role;
            await _db.SaveChangesAsync();
        }

        public async Task DeleteUserAsync(Guid id)
        {
            var u = await _db.Users.FindAsync(id);
            if (u == null) throw new Exception("User not found");
            _db.Users.Remove(u);
            await _db.SaveChangesAsync();
        }

        public async Task ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
        {
            var u = await _db.Users.FindAsync(userId);
            if (u == null) throw new Exception("User not found");
            if (!BCrypt.Net.BCrypt.Verify(currentPassword, u.PasswordHash))
                throw new Exception("Current password incorrect");
            u.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            await _db.SaveChangesAsync();
        }
    }
}
