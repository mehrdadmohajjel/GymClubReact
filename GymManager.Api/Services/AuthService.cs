using GymManager.Api.Data;
using GymManager.Api.DTOs;
using GymManager.Api.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;


namespace GymManager.Api.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _config;
        private readonly TimeSpan _accessTokenValidity = TimeSpan.FromMinutes(60);
        private readonly TimeSpan _refreshTokenValidity = TimeSpan.FromDays(30);

        public AuthService(AppDbContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        public async Task<AuthResultDto> RegisterAsync(RegisterDto dto, Role defaultRole = Role.Athlete)
        {
            if (await _db.Users.AnyAsync(u => u.NationalCode == dto.NationalCode))
                throw new Exception("National code already registered");

            var user = new User
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                NationalCode = dto.NationalCode,
                Phone = dto.Phone,
                Role = defaultRole,
                GymId = dto.GymId
            };

            // hash
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            return await GenerateTokensForUser(user);
        }

        public async Task<AuthResultDto> LoginAsync(LoginDto dto)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.NationalCode == dto.NationalCode && u.IsActive);

            if (user == null) throw new Exception("Invalid credentials");

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                throw new Exception("Invalid credentials");

            // check gym match (if provided) - optional
            if (dto.GymId.HasValue && user.GymId != dto.GymId)
                throw new Exception("User not in the provided gym");

            return await GenerateTokensForUser(user);
        }

        private async Task<AuthResultDto> GenerateTokensForUser(User user)
        {
            var jwtSecret = _config["Jwt:Secret"];
            var key = Encoding.ASCII.GetBytes(jwtSecret);

            var tokenHandler = new JwtSecurityTokenHandler();
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim("role", user.Role.ToString()),
                new Claim("gymId", user.GymId?.ToString() ?? ""),
                new Claim("nationalCode", user.NationalCode)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.Add(_accessTokenValidity),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var accessToken = tokenHandler.WriteToken(token);

            // refresh token
            var refreshTokenString = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            var refreshToken = new RefreshToken
            {
                Token = refreshTokenString,
                ExpiresAt = DateTime.UtcNow.Add(_refreshTokenValidity),
                UserId = user.Id
            };

            _db.RefreshTokens.Add(refreshToken);
            await _db.SaveChangesAsync();

            return new AuthResultDto(accessToken, refreshTokenString, tokenDescriptor.Expires.Value);
        }

        public async Task<AuthResultDto> RefreshTokenAsync(string refreshToken)
        {
            var token = await _db.RefreshTokens.Include(t => t.User).FirstOrDefaultAsync(t => t.Token == refreshToken);
            if (token == null || token.IsRevoked || token.ExpiresAt < DateTime.UtcNow)
                throw new Exception("Invalid refresh token");

            // optionally revoke old token and create new one
            token.IsRevoked = true;
            var newRefreshTokenString = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            var newRefreshToken = new RefreshToken
            {
                Token = newRefreshTokenString,
                ExpiresAt = DateTime.UtcNow.Add(_refreshTokenValidity),
                UserId = token.UserId
            };

            _db.RefreshTokens.Add(newRefreshToken);
            await _db.SaveChangesAsync();

            // generate access token
            var user = token.User;
            return await GenerateTokensForUser(user);
        }

        public async Task RevokeRefreshTokenAsync(string token)
        {
            var r = await _db.RefreshTokens.FirstOrDefaultAsync(t => t.Token == token);
            if (r != null)
            {
                r.IsRevoked = true;
                await _db.SaveChangesAsync();
            }
        }
    }

}
