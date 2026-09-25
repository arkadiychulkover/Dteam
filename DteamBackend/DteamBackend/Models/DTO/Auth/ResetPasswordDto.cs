using System.ComponentModel.DataAnnotations;
using DteamBackend.Validation;

namespace DteamBackend.Models.DTO.Auth
{
    public class ResetPasswordDto
    {
        [Required(ErrorMessage = "Токен відновлення обов'язковий")]
        public string Token { get; set; } = string.Empty;

        [Required(ErrorMessage = "Новий пароль обов'язковий")]
        [StrongPassword(MinLength = 7)]
        public string NewPassword { get; set; } = string.Empty;
    }

    public class VerifyCodeDto
    {
        [Required(ErrorMessage = "Email або логін обов'язковий")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Код підтвердження обов'язковий")]
        public string Code { get; set; } = string.Empty;
    }
}
