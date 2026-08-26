namespace DteamBackend.Models.DTO.Auth
{
    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public UserDto User { get; set; } = default!;
    }
}
