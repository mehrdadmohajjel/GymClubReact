using GymManager.Api.Data;
using GymManager.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GymManager.Api.Services
{
    public class GymService : IGymService
    {
        private readonly AppDbContext _db;
        public GymService(AppDbContext db) { _db = db; }

        public async Task<Gym> CreateGymAsync(Gym gym)
        {
            gym.Id = Guid.NewGuid();
            gym.CreatedAt = DateTime.UtcNow;
            gym.IsApproved = false;
            _db.Gyms.Add(gym);
            await _db.SaveChangesAsync();
            return gym;
        }

        public async Task<IEnumerable<Gym>> GetAllGymsAsync(bool onlyApproved = true)
        {
            var q = _db.Gyms.AsQueryable();
            if (onlyApproved) q = q.Where(x => x.IsApproved);
            return await q.ToListAsync();
        }

        public async Task<Gym?> GetGymAsync(Guid id)
        {
            return await _db.Gyms.Include(g => g.Users).FirstOrDefaultAsync(g => g.Id == id);
        }

        public async Task ApproveGymAsync(Guid id)
        {
            var gym = await _db.Gyms.FindAsync(id);
            if (gym == null) throw new Exception("Gym not found");
            gym.IsApproved = true;
            await _db.SaveChangesAsync();
        }

        public async Task UpdateGymAsync(Gym gym)
        {
            var existing = await _db.Gyms.FindAsync(gym.Id);
            if (existing == null) throw new Exception("Gym not found");
            existing.Name = gym.Name;
            existing.Address = gym.Address;
            existing.Phone = gym.Phone;
            await _db.SaveChangesAsync();
        }

        public async Task DeleteGymAsync(Guid id)
        {
            var gym = await _db.Gyms.FindAsync(id);
            if (gym == null) throw new Exception("Gym not found");
            _db.Gyms.Remove(gym);
            await _db.SaveChangesAsync();
        }
    }
}
