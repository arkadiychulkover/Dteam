using System.ComponentModel.DataAnnotations;

namespace DteamBackend.Models.DTO.Auth
{
    public class ForgotPasswordDto
    {
        [Required(ErrorMessage = "Email або логін обов'язковий")]
        public string Email { get; set; } = string.Empty;
    }
}
