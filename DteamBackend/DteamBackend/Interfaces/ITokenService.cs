using DteamBackend.Models;
using DteamBackend.Models.DTO.Auth;

namespace DteamBackend.Interfaces
{
    public interface ITokenService
    {
        string GenerateAccessToken(Duser user);
        Task<RefreshToken> GenerateRefreshTokenAsync(Duser user);
        Task<AuthResponseDto> RefreshTokensAsync(string refreshToken);
    }
}
