using System.ComponentModel.DataAnnotations;
using DteamBackend.Models.Enums;

namespace DteamBackend.Models
{
    public class NftItem
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public int? TokenId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(500)]
        public string? GiftMessage { get; set; }

        [Required]
        public NftRarity Rarity { get; set; } = NftRarity.Common;

        [Required]
        [MaxLength(255)]
        public string ImageUrl { get; set; } = string.Empty;

        public string Image => ImageUrl;

        public int BackgroundIndex { get; set; }

        public int PatternIndex { get; set; }

        public int ModelIndex { get; set; }

        [MaxLength(100)]
        public string ContractAddress { get; set; } = "0x9fE46736679d2D9a65F0992F2272dE9f3c7fa6e0";

        [MaxLength(100)]
        public string? OwnerAddress { get; set; }

        [MaxLength(100)]
        public string? FromAddress { get; set; }

        public Guid? UserId { get; set; }

        public Duser? User { get; set; }

        public bool IsMinted { get; set; } = false;

        public decimal PriceInPoints { get; set; } = 100m;

        public DateTime? LastTransferredAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
