using GymManager.Api.Data;
using GymManager.Api.DTOs;
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
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _db;
        public UsersController(AppDbContext db) { _db = db; }

        private Guid? GetGymIdFromClaims()
        {
            var gymClaim = User.FindFirst("gymId")?.Value;
            if (string.IsNullOrEmpty(gymClaim)) return null;
            return Guid.Parse(gymClaim);
        }

        [Authorize(Policy = "GymAdminOnly")]
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] DTOs.RegisterDto dto)
        {
            var gymId = GetGymIdFromClaims();
            if (gymId == null) return Forbid();

            var reg = new RegisterDto(dto.FirstName, dto.LastName, dto.NationalCode, dto.Phone, dto.Password, gymId);
            // create as athlete by default
            // reuse AuthService or directly create user
            var existing = await _db.Users.AnyAsync(u => u.NationalCode == reg.NationalCode);
            if (existing) return BadRequest("National code exists");

            var user = new User
            {
                FirstName = reg.FirstName,
                LastName = reg.LastName,
                NationalCode = reg.NationalCode,
                Phone = reg.Phone,
                GymId = gymId,
                Role = Role.Athlete,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(reg.Password)
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            return Ok(user);
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();
            var user = await _db.Users.Include(u => u.Gym).FirstOrDefaultAsync(u => u.Id == Guid.Parse(userId));
            return Ok(user);
        }

        [Authorize(Policy = "GymAdminOnly")]
        [HttpGet("list")]
        public async Task<IActionResult> List(int page = 1, int pageSize = 20, string? search = null)
        {
            var gymId = GetGymIdFromClaims();
            if (gymId == null) return Forbid();

            var q = _db.Users.Where(u => u.GymId == gymId);
            if (!string.IsNullOrEmpty(search))
                q = q.Where(u => u.FirstName.Contains(search) || u.LastName.Contains(search) || u.NationalCode.Contains(search));

            var total = await q.CountAsync();
            var data = await q.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return Ok(new { total, data });
        }

        // edit, delete endpoints similar...
    }

}
