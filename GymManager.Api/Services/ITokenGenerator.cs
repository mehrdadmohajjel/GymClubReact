using GymManager.Api.Models;

namespace GymManager.Api.Services
{
    public interface ITokenGenerator
    {
        string GenerateJwtToken(User user);
        string GenerateResetToken();
    }
}
