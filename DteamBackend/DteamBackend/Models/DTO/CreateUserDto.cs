using System.ComponentModel.DataAnnotations;
using DteamBackend.Models.Enums;

namespace DteamBackend.Models.DTO
{
    public class CreateUserDto
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [MaxLength(256)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Username is required")]
        [MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? WalletAddress { get; set; }

        public bool IsAdmin { get; set; } = false;

        public long BalanceInNanoTons { get; set; } = 0;

        public UserStatus Status { get; set; } = UserStatus.Offline;

        [MaxLength(500)]
        public string? AvatarUrl { get; set; }

        [MaxLength(1000)]
        public string? Bio { get; set; }
    }
}

