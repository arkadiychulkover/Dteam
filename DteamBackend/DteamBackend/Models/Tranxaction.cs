using System.ComponentModel.DataAnnotations;

namespace DteamBackend.Models
{
    public class Tranxaction
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(256)]
        public string TxhHash { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        public Guid? UserId { get; set; }
        public Duser? User { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

