using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DteamBackend.Models;
using Microsoft.IdentityModel.Tokens;

namespace DteamBackend.Services
{
    public interface IJwtTokenService
    {
        string GenerateToken(Duser user);
    }

    public class JwtTokenService : IJwtTokenService
    {
        private readonly IConfiguration _configuration;

        public JwtTokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateToken(Duser user)
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
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
