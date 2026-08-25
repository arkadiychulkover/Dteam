using System.ComponentModel.DataAnnotations;

namespace DteamBackend.DTOs.Auth
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "Email обязателен")]
        [EmailAddress(ErrorMessage = "Неверный формат Email")]
        [MaxLength(256)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Имя пользователя обязательно")]
        [MaxLength(50)]
        [MinLength(3, ErrorMessage = "Имя пользователя должно содержать не менее 3 символов")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Пароль обязателен")]
        [MinLength(6, ErrorMessage = "Пароль должен быть не менее 6 символов")]
        public string Password { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? WalletAddress { get; set; }
    }
}
