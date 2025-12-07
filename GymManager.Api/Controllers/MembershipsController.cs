using GymManager.Api.Data;
using GymManager.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GymManager.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MembershipsController : ControllerBase
    {
        private readonly AppDbContext _db;
        public MembershipsController(AppDbContext db) { _db = db; }

        private Guid? GetGymIdFromClaims()
        {
            var gymClaim = User.FindFirst("gymId")?.Value;
            if (string.IsNullOrEmpty(gymClaim)) return null;
            return Guid.Parse(gymClaim);
        }

        [Authorize(Policy = "GymAdminOnly")]
        [HttpPost("assign")]
        public async Task<IActionResult> Assign([FromBody] AssignMembershipDto dto)
        {
            var gymId = GetGymIdFromClaims();
            if (gymId == null) return Forbid();

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == dto.UserId && u.GymId == gymId);
            if (user == null) return NotFound();

            var membership = new Membership
            {
                GymId = gymId.Value,
                UserId = dto.UserId,
                Type = dto.Type,
                RemainingSessions = dto.Type == MembershipType.SessionBased ? dto.Sessions ?? 0 : 0,
                ExpiresAt = dto.Type == MembershipType.Monthly ? DateTime.UtcNow.AddMonths(dto.Months ?? 1) : null,
                IsActive = true
            };

            _db.Memberships.Add(membership);
            await _db.SaveChangesAsync();

            // register payment record
            var payment = new Payment
            {
                GymId = gymId.Value,
                UserId = dto.UserId,
                Amount = dto.Amount,
                IsOnline = dto.IsOnline,
                IsPaid = dto.IsOnline ? false : true
            };

            _db.Payments.Add(payment);
            await _db.SaveChangesAsync();

            return Ok(membership);
        }

        [Authorize]
        [HttpGet("my")]
        public async Task<IActionResult> MyMemberships()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();
            var uid = Guid.Parse(userId);
            var list = await _db.Memberships.Include(m => m.Gym).Where(m => m.UserId == uid).ToListAsync();
            return Ok(list);
        }

        [Authorize(Policy = "GymAdminOnly")]
        [HttpPost("{id}/deduct-session")]
        public async Task<IActionResult> DeductSession(Guid id)
        {
            var gymId = GetGymIdFromClaims();
            if (gymId == null) return Forbid();

            var membership = await _db.Memberships.FirstOrDefaultAsync(m => m.Id == id && m.GymId == gymId);
            if (membership == null) return NotFound();

            if (membership.Type != MembershipType.SessionBased)
                return BadRequest("Not session-based membership");

            membership.RemainingSessions = Math.Max(0, membership.RemainingSessions - 1);
            await _db.SaveChangesAsync();
            return Ok(membership);
        }
    }

    public record AssignMembershipDto(Guid UserId, MembershipType Type, int? Sessions, int? Months, decimal Amount, bool IsOnline);
}
