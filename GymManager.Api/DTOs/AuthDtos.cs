namespace GymManager.Api.DTOs
{
    public record RegisterDto(string FirstName, string LastName, string Email, string NationalCode, string Phone, string Password, Guid? GymId = null);
    public record LoginDto(string Username, string NationalCode, string Password, Guid? GymId = null);
    public record AuthResultDto(string AccessToken, string RefreshToken, DateTime ExpiresAt);
    public record RefreshRequestDto(string RefreshToken);
    public class ForgotPasswordDto
    {
        public string Email { get; set; }
    }

    public class ResetPasswordDto
    {
        public string Token { get; set; }
        public string NewPassword { get; set; }
    }
}
