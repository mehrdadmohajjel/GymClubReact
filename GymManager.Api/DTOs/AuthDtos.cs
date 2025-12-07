namespace GymManager.Api.DTOs
{
    public record RegisterDto(string FirstName, string LastName, string NationalCode, string Phone, string Password, Guid? GymId = null);
    public record LoginDto(string NationalCode, string Password, Guid? GymId = null);
    public record AuthResultDto(string AccessToken, string RefreshToken, DateTime ExpiresAt);
    public record RefreshRequestDto(string RefreshToken);

}
