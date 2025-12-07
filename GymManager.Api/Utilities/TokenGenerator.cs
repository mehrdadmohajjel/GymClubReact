using System.Security.Cryptography;

namespace GymManager.Api.Utilities
{
    public static class TokenGenerator
    {
        public static string GenerateToken(int length = 48)
        {
            var bytes = RandomNumberGenerator.GetBytes(length);
            return Convert.ToBase64String(bytes).Replace("+", "").Replace("/", "").Replace("=", "");
        }
    }

}
