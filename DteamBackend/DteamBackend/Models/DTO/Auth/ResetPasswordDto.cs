using System.ComponentModel.DataAnnotations;
using DteamBackend.Validation;

namespace DteamBackend.Models.DTO.Auth
{
    public class ResetPasswordDto
    {
        [Required(ErrorMessage = "Токен восстановления обязателен")]
        public string Token { get; set; } = string.Empty;

        [Required(ErrorMessage = "Новый пароль обязателен")]
        [StrongPassword(MinLength = 7)]
        public string NewPassword { get; set; } = string.Empty;
    }

    public class VerifyCodeDto
    {
        [Required(ErrorMessage = "Email обязателен")]
        [EmailAddress(ErrorMessage = "Неверный формат Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Код подтверждения обязателен")]
        public string Code { get; set; } = string.Empty;
    }
}
