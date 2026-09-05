using System.ComponentModel.DataAnnotations;

namespace DteamBackend.Models.DTO.Auth
{
    public class RefreshTokenRequestDto
    {
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }
}
