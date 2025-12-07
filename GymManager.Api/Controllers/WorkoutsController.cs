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
    public class WorkoutsController : ControllerBase
    {
        private readonly AppDbContext _db;
        public WorkoutsController(AppDbContext db) { _db = db; }

        private Guid GetUserId() => Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
        private Guid? GetGymId()
        {
            var g = User.FindFirst("gymId")?.Value;
            if (Guid.TryParse(g, out var gid)) return gid;
            return null;
        }

        [Authorize(Policy = "TrainerOnly")]
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreateWorkoutDto dto)
        {
            // dto.Days is expected as JSON string or structured data from front-end
            var trainerId = GetUserId();
            var gymId = GetGymId() ?? throw new Exception("GymId missing");
            var plan = new WorkoutPlan
            {
                Id = Guid.NewGuid(),
                GymId = gymId,
                TrainerId = trainerId,
                AthleteId = dto.AthleteId,
                Title = dto.Title,
                CreatedAt = DateTime.UtcNow
            };
            _db.WorkoutPlans.Add(plan);
            // parse days array
            // Expect dto.Days as array of { dayIndex: number, movementName: string, sets:int, reps:int }
            var days = dto.Days ?? new List<WorkoutDayDto>();
            foreach (var d in days)
            {
                var wd = new WorkoutDay
                {
                    Id = Guid.NewGuid(),
                    WorkoutPlanId = plan.Id,
                    DayIndex = d.DayIndex,
                    MovementName = d.MovementName,
                    Sets = d.Sets,
                    Reps = d.Reps
                };
                _db.WorkoutDays.Add(wd);
            }
            await _db.SaveChangesAsync();
            return Ok(plan);
        }

        [Authorize]
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetForUser(Guid userId)
        {
            var gymId = GetGymId();
            var plans = await _db.WorkoutPlans
                .Include(p => p.Days)
                .Where(p => p.AthleteId == userId && p.GymId == gymId)
                .ToListAsync();
            return Ok(plans);
        }

        [Authorize(Policy = "TrainerOnly")]
        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateWorkoutDto dto)
        {
            var plan = await _db.WorkoutPlans.Include(p => p.Days).FirstOrDefaultAsync(p => p.Id == id);
            if (plan == null) return NotFound();
            plan.Title = dto.Title ?? plan.Title;

            // replace days: remove existing and add new
            _db.WorkoutDays.RemoveRange(plan.Days);
            foreach (var d in dto.Days ?? new List<WorkoutDayDto>())
            {
                var wd = new WorkoutDay
                {
                    Id = Guid.NewGuid(),
                    WorkoutPlanId = plan.Id,
                    DayIndex = d.DayIndex,
                    MovementName = d.MovementName,
                    Sets = d.Sets,
                    Reps = d.Reps
                };
                _db.WorkoutDays.Add(wd);
            }
            await _db.SaveChangesAsync();
            return Ok(plan);
        }

        [Authorize(Policy = "TrainerOnly")]
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var plan = await _db.WorkoutPlans.FindAsync(id);
            if (plan == null) return NotFound();
            _db.WorkoutPlans.Remove(plan);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }

    public record CreateWorkoutDto(Guid AthleteId, string Title, List<WorkoutDayDto>? Days);
    public record UpdateWorkoutDto(string? Title, List<WorkoutDayDto>? Days);
    public record WorkoutDayDto(int DayIndex, string MovementName, int Sets, int Reps);
}
