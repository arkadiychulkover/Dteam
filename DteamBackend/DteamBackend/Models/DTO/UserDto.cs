using DteamBackend.Models.Enums;

namespace DteamBackend.Models.DTO
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string? WalletAddress { get; set; }
        public string? HardhatAddress { get; set; }
        public long BalanceInNanoTons { get; set; }
        public long TotalEarningsInNanoTons { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public DateTime? LastConnectedAt { get; set; }
        public DateTime? LastDisconnectedAt { get; set; }
        public long TotalTimeSpentSeconds { get; set; }
        public int TimeRewardNftsMintedCount { get; set; }
        public bool IsInFamily { get; set; }
        public Guid? FamilyOwnerId { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsBanned { get; set; }
        public UserStatus Status { get; set; }
        public string? AvatarUrl { get; set; }
        public string? Bio { get; set; }

        public static UserDto FromEntity(Duser user)
        {
            return new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                Username = user.Username,
                WalletAddress = user.WalletAddress,
                HardhatAddress = user.HardhatAddress,
                BalanceInNanoTons = user.BalanceInNanoTons,
                TotalEarningsInNanoTons = user.TotalEarningsInNanoTons,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                LastLoginAt = user.LastLoginAt,
                LastConnectedAt = user.LastConnectedAt,
                LastDisconnectedAt = user.LastDisconnectedAt,
                TotalTimeSpentSeconds = user.TotalTimeSpentSeconds,
                TimeRewardNftsMintedCount = user.TimeRewardNftsMintedCount,
                IsInFamily = user.IsInFamily,
                FamilyOwnerId = user.FamilyOwnerId,
                IsAdmin = user.IsAdmin,
                IsBanned = user.IsBanned,
                Status = user.Status,
                AvatarUrl = user.AvatarUrl,
                Bio = user.Bio
            };
        }
    }
}
