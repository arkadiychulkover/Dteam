using System.ComponentModel.DataAnnotations;
using DteamBackend.Models.Enums;

namespace DteamBackend.Models.DTO
{
    public class UpdateUserDto
    {
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [MaxLength(256)]
        public string? Email { get; set; }

        [MaxLength(50)]
        public string? Username { get; set; }

        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string? Password { get; set; }

        [MaxLength(100)]
        public string? WalletAddress { get; set; }

        public bool? IsAdmin { get; set; }

        public bool? IsBanned { get; set; }

        public UserStatus? Status { get; set; }

        [Range(0, long.MaxValue, ErrorMessage = "Balance must be non-negative")]
        public long? BalanceInNanoTons { get; set; }

        [MaxLength(500)]
        public string? AvatarUrl { get; set; }

        [MaxLength(1000)]
        public string? Bio { get; set; }
    }
}
