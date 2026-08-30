using System.ComponentModel.DataAnnotations;
using DteamBackend.Models.Enums;

namespace DteamBackend.Models
{
    public class UserFriend
    {
        public Guid UserId { get; set; }
        public Duser User { get; set; } = null!;

        public Guid FriendId { get; set; }
        public Duser Friend { get; set; } = null!;

        public FriendshipStatus Status { get; set; } = FriendshipStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
