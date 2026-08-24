using System.ComponentModel.DataAnnotations;
using DteamBackend.Models.Enums;

namespace DteamBackend.Models
{
    public class Duser
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [MaxLength(256)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Username is required")]
        [MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        public string PasswordSalt { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? WalletAddress { get; set; }

        public string? PasswordResetToken { get; set; }

        public DateTime? PasswordResetTokenExpiresAt { get; set; }

        public long BalanceInNanoTons { get; set; } = 0;

        public long TotalEarningsInNanoTons { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public DateTime? LastLoginAt { get; set; }

        public bool IsInFamily { get; set; } = false;

        public Guid? FamilyOwnerId { get; set; }

        public Duser? FamilyOwner { get; set; }

        public ICollection<Duser> FamilyMembers { get; set; } = new List<Duser>();

        public bool IsAdmin { get; set; } = false;

        public bool IsBanned { get; set; } = false;

        public UserStatus Status { get; set; } = UserStatus.Offline;

        [MaxLength(500)]
        public string? AvatarUrl { get; set; }

        [MaxLength(1000)]
        public string? Bio { get; set; }

        public ICollection<UserFriend> Friendships { get; set; } = new List<UserFriend>();

        public ICollection<UserGame> Library { get; set; } = new List<UserGame>();

        public ICollection<Game> CreatedGames { get; set; } = new List<Game>();

        public ICollection<Review> Reviews { get; set; } = new List<Review>();

        public ICollection<UserWishlist> Wishlist { get; set; } = new List<UserWishlist>();
    }
}
