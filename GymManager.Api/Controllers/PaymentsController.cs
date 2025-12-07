using GymManager.Api.Data;
using GymManager.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GymManager.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _config;
        public PaymentsController(AppDbContext db, IConfiguration config)
        {
            _db = db; _config = config;
        }

        // Create payment (client calls to create invoice and get redirect URL)
        [Authorize]
        [HttpPost("create")]
        public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentDto dto)
        {
            // create record
            var gymIdClaim = User.FindFirst("gymId")?.Value;
            Guid? gymId = null;
            if (!string.IsNullOrEmpty(gymIdClaim)) gymId = Guid.Parse(gymIdClaim);
            var payment = new Payment
            {
                GymId = gymId ?? Guid.Empty,
                UserId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value),
                Amount = dto.Amount,
                IsOnline = dto.IsOnline
            };
            _db.Payments.Add(payment);
            await _db.SaveChangesAsync();

            if (!dto.IsOnline)
            {
                payment.IsPaid = true;
                await _db.SaveChangesAsync();
                return Ok(new { message = "Offline payment recorded" });
            }

            // Online: build test redirect URL (simulate)
            var fakeGatewayUrl = $"{_config["FrontendUrl"]}/payments/mockpay?paymentId={payment.Id}";
            return Ok(new { redirect = fakeGatewayUrl, paymentId = payment.Id });
        }

        // Callback to mark paid (in real with Parbad callback)
        [HttpPost("callback")]
        public async Task<IActionResult> Callback([FromBody] PaymentCallbackDto dto)
        {
            var p = await _db.Payments.FindAsync(dto.PaymentId);
            if (p == null) return NotFound();
            p.IsPaid = dto.Success;
            if (dto.Success) p.GatewayReference = dto.Reference;
            await _db.SaveChangesAsync();
            return Ok();
        }
    }

    public record CreatePaymentDto(decimal Amount, bool IsOnline);
    public record PaymentCallbackDto(Guid PaymentId, bool Success, string? Reference);
}