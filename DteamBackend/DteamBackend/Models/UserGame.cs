using System.ComponentModel.DataAnnotations;

namespace DteamBackend.Models
{
    public class UserGame
    {
        public Guid UserId { get; set; }
        public Duser User { get; set; } = null!;

        public Guid GameId { get; set; }
        public Game Game { get; set; } = null!;

        public DateTime PurchasedAt { get; set; } = DateTime.UtcNow;

        public long PlayTimeMinutes { get; set; } = 0;

        public DateTime? LastPlayedAt { get; set; }

        public bool IsFavorite { get; set; } = false;
    }
}
