using System.ComponentModel.DataAnnotations;

namespace DteamBackend.DTOs.Auth
{
    public class LoginDto
    {
        [Required(ErrorMessage = "Введите email или имя пользователя")]
        public string EmailOrUsername { get; set; } = string.Empty;

        [Required(ErrorMessage = "Пароль обязателен")]
        public string Password { get; set; } = string.Empty;
    }
}
