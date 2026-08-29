using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using DteamBackend.Data;
using DteamBackend.Models;
using DteamBackend.Models.DTO;
using DteamBackend.Models.DTO.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace DteamBackend.Services
{
    public interface IJwtTokenService
    {
        string GenerateAccessToken(Duser user);
        Task<RefreshToken> GenerateRefreshTokenAsync(Duser user);
        Task<AuthResponseDto> RefreshTokensAsync(string rawRefreshToken);
        Task RevokeUserTokensAsync(Guid userId);
    }

    public class JwtTokenService : IJwtTokenService
    {
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _db;

        public JwtTokenService(IConfiguration configuration, AppDbContext db)
        {
            _configuration = configuration;
            _db = db;
        }

        public string GenerateAccessToken(Duser user)
        {
            var secretKey = _configuration["Jwt:Secret"]
                ?? "DteamSuperSecretJwtKey2026_dteam_io_security_token_key_spec_32bytes_long";

            var issuer = _configuration["Jwt:Issuer"] ?? "DteamBackend";
            var audience = _configuration["Jwt:Audience"] ?? "DteamApp";

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.IsAdmin ? "Admin" : "User"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(15),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<RefreshToken> GenerateRefreshTokenAsync(Duser user)
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);

            var refreshToken = new RefreshToken
            {
                Id = Guid.NewGuid(),
                Token = Convert.ToBase64String(randomNumber),
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow,
                IsUsed = false,
                IsRevoked = false
            };

            _db.RefreshTokens.Add(refreshToken);
            await _db.SaveChangesAsync();

            return refreshToken;
        }

        public async Task<AuthResponseDto> RefreshTokensAsync(string rawRefreshToken)
        {
            var existingToken = await _db.RefreshTokens
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Token == rawRefreshToken);

            if (existingToken == null || !existingToken.IsActive || existingToken.User.IsBanned)
            {
                throw new UnauthorizedAccessException("Invalid or expired refresh token");
            }

            existingToken.IsUsed = true;
            _db.RefreshTokens.Update(existingToken);

            var newAccessToken = GenerateAccessToken(existingToken.User);
            var newRefreshToken = await GenerateRefreshTokenAsync(existingToken.User);

            await _db.SaveChangesAsync();

            return new AuthResponseDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken.Token,
                User = UserDto.FromEntity(existingToken.User)
            };
        }

        public async Task RevokeUserTokensAsync(Guid userId)
        {
            var activeTokens = await _db.RefreshTokens
                .Where(r => r.UserId == userId && !r.IsRevoked && !r.IsUsed)
                .ToListAsync();

            foreach (var token in activeTokens)
            {
                token.IsRevoked = true;
            }

            await _db.SaveChangesAsync();
        }
    }
}