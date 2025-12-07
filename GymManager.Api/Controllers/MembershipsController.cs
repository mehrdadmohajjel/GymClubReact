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

        private Guid GetUserId() => Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
        private Guid? GetGymId() { var g = User.FindFirst("gymId")?.Value; if (Guid.TryParse(g, out var gid)) return gid; return null; }

        [Authorize(Policy = "GymAdminOnly")]
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreateMembershipDto dto)
        {
            var gymId = GetGymId() ?? throw new Exception("GymId missing");
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == dto.UserId && u.GymId == gymId);
            if (user == null) return NotFound("User not found");

            var mem = new Membership
            {
                Id = Guid.NewGuid(),
                GymId = gymId,
                UserId = dto.UserId,
                Type = dto.Type,
                RemainingSessions = dto.Type == MembershipType.SessionBased ? dto.Sessions ?? 0 : 0,
                ExpiresAt = dto.Type == MembershipType.Monthly ? DateTime.UtcNow.AddMonths(dto.Months ?? 1) : null,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _db.Memberships.Add(mem);

            // Payment record
            var pay = new Payment
            {
                Id = Guid.NewGuid(),
                GymId = gymId,
                UserId = dto.UserId,
                Amount = dto.Amount,
                IsOnline = dto.IsOnline,
                IsPaid = !dto.IsOnline
            };
            _db.Payments.Add(pay);

            await _db.SaveChangesAsync();
            return Ok(new { membership = mem, payment = pay });
        }

        [Authorize(Policy = "GymAdminOnly")]
        [HttpPost("{id}/renew")]
        public async Task<IActionResult> Renew(Guid id, [FromBody] RenewDto dto)
        {
            var gymId = GetGymId() ?? throw new Exception("GymId missing");
            var mem = await _db.Memberships.FirstOrDefaultAsync(m => m.Id == id && m.GymId == gymId);
            if (mem == null) return NotFound();

            if (mem.Type == MembershipType.SessionBased)
            {
                mem.RemainingSessions += dto.Sessions ?? 0;
            }
            else
            {
                // extend by months
                mem.ExpiresAt = (mem.ExpiresAt ?? DateTime.UtcNow).AddMonths(dto.Months ?? 1);
            }

            // record payment
            var pay = new Payment
            {
                Id = Guid.NewGuid(),
                GymId = gymId,
                UserId = mem.UserId,
                Amount = dto.Amount,
                IsOnline = dto.IsOnline,
                IsPaid = !dto.IsOnline
            };
            _db.Payments.Add(pay);

            await _db.SaveChangesAsync();
            return Ok(mem);
        }

        [Authorize]
        [HttpGet("my")]
        public async Task<IActionResult> MyMemberships()
        {
            var uid = GetUserId();
            var list = await _db.Memberships.Include(m => m.Gym).Where(m => m.UserId == uid).ToListAsync();
            return Ok(list);
        }

        [Authorize(Policy = "GymAdminOnly")]
        [HttpPost("{id}/deduct-session")]
        public async Task<IActionResult> Deduct(Guid id)
        {
            var gymId = GetGymId() ?? throw new Exception("GymId missing");
            var mem = await _db.Memberships.FirstOrDefaultAsync(m => m.Id == id && m.GymId == gymId);
            if (mem == null) return NotFound();
            if (mem.Type != MembershipType.SessionBased) return BadRequest("Not session based");
            if (mem.RemainingSessions <= 0) return BadRequest("No sessions left");
            mem.RemainingSessions--;
            await _db.SaveChangesAsync();
            return Ok(mem);
        }
    }

    public record CreateMembershipDto(Guid UserId, MembershipType Type, int? Sessions, int? Months, decimal Amount, bool IsOnline);
    public record RenewDto(int? Sessions, int? Months, decimal Amount, bool IsOnline);
}