using System.ComponentModel.DataAnnotations;

namespace DteamBackend.Models
{
    public class NftTransfer
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid NftItemId { get; set; }

        public NftItem? NftItem { get; set; }

        public int TokenId { get; set; }

        [Required]
        [MaxLength(100)]
        public string FromAddress { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string ToAddress { get; set; } = string.Empty;

        public Guid? FromUserId { get; set; }

        public Duser? FromUser { get; set; }

        public Guid? ToUserId { get; set; }

        public Duser? ToUser { get; set; }

        [MaxLength(200)]
        public string? TransactionHash { get; set; }

        public long BlockNumber { get; set; }

        public DateTime TransferredAt { get; set; } = DateTime.UtcNow;
    }
}
