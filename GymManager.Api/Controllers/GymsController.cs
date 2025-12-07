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
    public class GymsController : ControllerBase
    {
        private readonly AppDbContext _db;
        public GymsController(AppDbContext db) { _db = db; }

        [HttpPost("request")]
        public async Task<IActionResult> RequestGym([FromBody] Gym model)
        {
            model.IsApproved = false;
            model.Id = Guid.NewGuid();
            await _db.Gyms.AddAsync(model);
            await _db.SaveChangesAsync();
            return Accepted(new { message = "Gym requested" });
        }

        [Authorize(Policy = "SuperAdminOnly")]
        [HttpGet("pending")]
        public async Task<IActionResult> GetPending()
        {
            var pending = await _db.Gyms.Where(g => !g.IsApproved).ToListAsync();
            return Ok(pending);
        }

        [Authorize(Policy = "SuperAdminOnly")]
        [HttpPost("{id}/approve")]
        public async Task<IActionResult> Approve(Guid id)
        {
            var g = await _db.Gyms.FindAsync(id);
            if (g == null) return NotFound();
            g.IsApproved = true;
            await _db.SaveChangesAsync();
            return Ok(g);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetGyms()
        {
            var gyms = await _db.Gyms.Where(g => g.IsApproved).ToListAsync();
            return Ok(gyms);
        }
    }
}
