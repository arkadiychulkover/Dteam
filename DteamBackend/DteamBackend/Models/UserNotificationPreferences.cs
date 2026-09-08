using System.ComponentModel.DataAnnotations;

namespace DteamBackend.Models
{
    public class UserNotificationPreferences
    {
        [Key]
        public Guid UserId { get; set; }
        public Duser User { get; set; } = null!;

        // Беззвучні сповіщення
        public bool NotifyBigSales { get; set; } = true;
        public bool NotifyWishlistDiscounts { get; set; } = true;
        public bool NotifyProfileComments { get; set; } = true;
        public bool NotifyFriendRequests { get; set; } = true;
        public bool NotifyFriendRequestAccepted { get; set; } = true;
        public bool NotifyFriendRequestDeclined { get; set; } = true;

        // Чат
        public bool ChatNotificationsEnabled { get; set; } = true;
        public bool ChatSoundEnabled { get; set; } = true;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
