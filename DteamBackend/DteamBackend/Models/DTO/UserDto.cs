using DteamBackend.Models.Enums;

namespace DteamBackend.Models.DTO
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string? WalletAddress { get; set; }
        public long BalanceInNanoTons { get; set; }
        public long TotalEarningsInNanoTons { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public bool IsInFamily { get; set; }
        public Guid? FamilyOwnerId { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsBanned { get; set; }
        public UserStatus Status { get; set; }
        public string? AvatarUrl { get; set; }
        public string? Bio { get; set; }
    }
}
