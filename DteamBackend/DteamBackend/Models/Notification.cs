using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DteamBackend.Models
{
    public static class NotificationTypes
    {
        public const string FriendRequest = "friend_request";
        public const string FriendAccepted = "friend_accepted";
        public const string WalletDeposit = "wallet_deposit";
        public const string ChatMessage = "chat_message";
        public const string System = "system";
        public const string SecurityAlert = "security_alert";
    }

    public class Notification
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public Duser User { get; set; } = null!;

        public Guid? ActorUserId { get; set; }
        [ForeignKey(nameof(ActorUserId))]
        public Duser? ActorUser { get; set; }

        [Required, MaxLength(64)]
        public string Type { get; set; } = string.Empty;

        [MaxLength(64)]
        public string? EntityType { get; set; }

        public Guid? EntityId { get; set; }

        [MaxLength(128)]
        public string? EventId { get; set; }

        [Required, MaxLength(256)]
        public string Title { get; set; } = string.Empty;

        [Required, MaxLength(1024)]
        public string Message { get; set; } = string.Empty;

        public string? DataJson { get; set; }

        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReadAt { get; set; }

        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
    }
}
