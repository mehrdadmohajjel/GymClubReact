using GymManager.Api.Models;

namespace GymManager.Api.Services
{
    public interface IGymService
    {
        Task<Gym> CreateGymAsync(Gym gym);
        Task<IEnumerable<Gym>> GetAllGymsAsync(bool onlyApproved = true);
        Task<Gym?> GetGymAsync(Guid id);
        Task ApproveGymAsync(Guid id);
        Task UpdateGymAsync(Gym gym);
        Task DeleteGymAsync(Guid id);
    }

}
