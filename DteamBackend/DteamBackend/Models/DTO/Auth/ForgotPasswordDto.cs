using System.ComponentModel.DataAnnotations;

namespace DteamBackend.Models.DTO.Auth
{
    public class ForgotPasswordDto
    {
        [Required(ErrorMessage = "Email обязателен")]
        [EmailAddress(ErrorMessage = "Неверный формат Email")]
        public string Email { get; set; } = string.Empty;
    }
}
