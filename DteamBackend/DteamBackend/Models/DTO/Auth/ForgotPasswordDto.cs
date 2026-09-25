using System.ComponentModel.DataAnnotations;

namespace DteamBackend.Models.DTO.Auth
{
    public class ForgotPasswordDto
    {
        [Required(ErrorMessage = "Email або логін обов'язковий")]
        [MinLength(3, ErrorMessage = "Введіть щонайменше 3 символи")]
        public string Email { get; set; } = string.Empty;

        public string? EmailOrUsername { get; set; }
    }
}
