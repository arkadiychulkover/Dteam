using System.ComponentModel.DataAnnotations;

namespace DteamBackend.Models
{
    public class UserWishlist
    {
        public Guid UserId { get; set; }
        public Duser User { get; set; } = null!;

        public Guid GameId { get; set; }
        public Game Game { get; set; } = null!;

        public DateTime AddedAt { get; set; } = DateTime.UtcNow;

        public int Priority { get; set; } = 0;

        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}

