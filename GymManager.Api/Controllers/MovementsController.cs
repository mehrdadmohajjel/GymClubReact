using GymManager.Api.Data;
using GymManager.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymManager.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MovementsController : ControllerBase
    {
        private readonly AppDbContext _db;
        public MovementsController(AppDbContext db) { _db = db; }

        private Guid? GetGymId()
        {
            var g = User.FindFirst("gymId")?.Value;
            if (Guid.TryParse(g, out var gid)) return gid;
            return null;
        }

        [Authorize(Policy = "GymAdminOnly")]
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] MovementCreateDto dto)
        {
            var gymId = GetGymId() ?? throw new Exception("GymId missing");
            var mv = new Movement { Id = Guid.NewGuid(), GymId = gymId, Name = dto.Name, VideoUrl = dto.VideoUrl };
            _db.Movements.Add(mv);
            await _db.SaveChangesAsync();
            return Ok(mv);
        }

        [Authorize]
        [HttpGet("list")]
        public async Task<IActionResult> List()
        {
            var gymId = GetGymId();
            var q = _db.Movements.AsQueryable();
            if (gymId.HasValue) q = q.Where(m => m.GymId == gymId.Value);
            var list = await q.ToListAsync();
            return Ok(list);
        }

        [Authorize(Policy = "GymAdminOnly")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] MovementCreateDto dto)
        {
            var gymId = GetGymId() ?? throw new Exception("GymId missing");
            var mv = await _db.Movements.FirstOrDefaultAsync(m => m.Id == id && m.GymId == gymId);
            if (mv == null) return NotFound();
            mv.Name = dto.Name;
            mv.VideoUrl = dto.VideoUrl;
            await _db.SaveChangesAsync();
            return Ok(mv);
        }

        [Authorize(Policy = "GymAdminOnly")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var gymId = GetGymId() ?? throw new Exception("GymId missing");
            var mv = await _db.Movements.FirstOrDefaultAsync(m => m.Id == id && m.GymId == gymId);
            if (mv == null) return NotFound();
            _db.Movements.Remove(mv);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }

    public record MovementCreateDto(string Name, string? VideoUrl);
}