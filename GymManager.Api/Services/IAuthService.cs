using GymManager.Api.DTOs;
using GymManager.Api.Models;

namespace GymManager.Api.Services
{
    public interface IAuthService
    {
        Task<AuthResultDto> RegisterAsync(RegisterDto dto, Role defaultRole = Role.Athlete);
        Task<AuthResultDto> LoginAsync(LoginDto dto);
        Task<AuthResultDto> RefreshTokenAsync(string refreshToken);
        Task RevokeRefreshTokenAsync(string token);
    }

}
