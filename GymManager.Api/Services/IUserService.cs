using GymManager.Api.Models;

namespace GymManager.Api.Services
{
    public interface IUserService
    {
        Task<User> CreateUserAsync(User user, string plainPassword);
        Task<User?> GetUserByIdAsync(Guid id);
        Task<IEnumerable<User>> GetUsersByGymAsync(Guid gymId, int page = 1, int pageSize = 20, string? search = null);
        Task UpdateUserAsync(User user);
        Task DeleteUserAsync(Guid id);
        Task ChangePasswordAsync(Guid userId, string currentPassword, string newPassword);
    }
}
