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
    public class AttendanceController : ControllerBase
    {
        private readonly AppDbContext _db;
        public AttendanceController(AppDbContext db) { _db = db; }

        [Authorize(Policy = "GymAdminOnly")]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterAttendanceDto dto)
        {
            var gymIdClaim = User.FindFirst("gymId")?.Value;
            if (string.IsNullOrEmpty(gymIdClaim)) return Forbid();
            var gymId = Guid.Parse(gymIdClaim);

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == dto.UserId && u.GymId == gymId);
            if (user == null) return NotFound();

            // find active membership
            var membership = await _db.Memberships.Where(m => m.UserId == user.Id && m.GymId == gymId && m.IsActive)
                .OrderByDescending(m => m.CreatedAt).FirstOrDefaultAsync();

            if (membership == null) return BadRequest("No active membership");

            if (membership.Type == MembershipType.SessionBased)
            {
                if (membership.RemainingSessions <= 0) return BadRequest("No sessions left");
                membership.RemainingSessions -= 1;
            }
            else if (membership.Type == MembershipType.Monthly)
            {
                if (membership.ExpiresAt.HasValue && membership.ExpiresAt < DateTime.UtcNow)
                    return BadRequest("Membership expired");
            }

            var attendance = new Attendance { GymId = gymId, UserId = user.Id, Note = dto.Note };
            _db.Attendances.Add(attendance);
            await _db.SaveChangesAsync();

            return Ok(new { membership, attendance });
        }

        [Authorize(Policy = "GymAdminOnly")]
        [HttpGet("today")]
        public async Task<IActionResult> Today()
        {
            var gymIdClaim = User.FindFirst("gymId")?.Value;
            if (string.IsNullOrEmpty(gymIdClaim)) return Forbid();
            var gymId = Guid.Parse(gymIdClaim);

            var today = DateTime.UtcNow.Date;
            var list = await _db.Attendances
                .Include(a => a.User)
                .Where(a => a.GymId == gymId && a.EnteredAt >= today)
                .ToListAsync();

            return Ok(list);
        }
    }

    public record RegisterAttendanceDto(Guid UserId, string? Note);
}
