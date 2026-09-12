namespace DteamBackend.Models
{
    public class UserFriend
    {
        public Guid UserId { get; set; }
        public Duser User { get; set; } = null!;

        public Guid FriendId { get; set; }
        public Duser Friend { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
