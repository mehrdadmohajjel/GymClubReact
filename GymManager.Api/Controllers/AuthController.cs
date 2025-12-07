using GymManager.Api.DTOs;
using GymManager.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GymManager.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _auth;
        private readonly ITokenGenerator _tokens;
        private readonly IEmailService _email;

        public AuthController(IAuthService auth, ITokenGenerator tokens, IEmailService email) { _auth = auth;_tokens = tokens;_email = email; }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            var result = await _auth.RegisterAsync(dto);
            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var result = await _auth.LoginAsync(dto);
            return Ok(result);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequestDto dto)
        {
            var result = await _auth.RefreshTokenAsync(dto.RefreshToken);
            return Ok(result);
        }

        [Authorize]
        [HttpPost("revoke")]
        public async Task<IActionResult> Revoke([FromBody] RefreshRequestDto dto)
        {
            await _auth.RevokeRefreshTokenAsync(dto.RefreshToken);
            return NoContent();
        }
        [HttpPost("forgot")]
        public async Task<IActionResult> Forgot([FromBody] ForgotDto dto)
        {
            await _auth.RequestPasswordResetAsync(dto.NationalCode);
            // for security, return generic message
            return Ok(new { message = "If the account exists, a password reset link has been sent." });
        }

        [HttpPost("reset")]
        public async Task<IActionResult> Reset([FromBody] ResetDto dto)
        {
            await _auth.ResetPasswordAsync(dto.Token, dto.NewPassword);
            return Ok(new { message = "Password has been reset" });
        }
        public record ForgotDto(string NationalCode);
        public record ResetDto(string Token, string NewPassword);

    }

}
