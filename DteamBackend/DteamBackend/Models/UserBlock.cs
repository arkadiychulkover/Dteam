using System;

namespace DteamBackend.Models
{
    public class UserBlock
    {
        public Guid UserId { get; set; }
        public Duser User { get; set; } = null!;

        public Guid BlockedUserId { get; set; }
        public Duser BlockedUser { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
