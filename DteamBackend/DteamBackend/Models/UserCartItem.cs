using System.ComponentModel.DataAnnotations;

namespace DteamBackend.Models
{
    public class UserCartItem
    {
        public Guid UserId { get; set; }
        public Duser User { get; set; } = null!;

        public Guid GameId { get; set; }
        public Game Game { get; set; } = null!;

        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    }
}
