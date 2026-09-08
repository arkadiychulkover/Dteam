using System.ComponentModel.DataAnnotations;

namespace DteamBackend.Models
{
    public enum WalletTransactionType
    {
        Deposit = 1,
        Purchase = 2,
        Refund = 3,
        Withdrawal = 4
    }

    public enum WalletTransactionStatus
    {
        Completed = 1,
        Pending = 2,
        Failed = 3
    }

    public class WalletTransaction
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid UserId { get; set; }
        public Duser User { get; set; } = null!;

        public WalletTransactionType Type { get; set; }
        public WalletTransactionStatus Status { get; set; } = WalletTransactionStatus.Completed;

        // Positive amount in nanoTONs (1 TON = 1_000_000_000 nanoTONs)
        public long AmountInNanoTons { get; set; }

        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(10)]
        public string Currency { get; set; } = "TON";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [MaxLength(256)]
        public string? ReferenceId { get; set; }

        public string? Metadata { get; set; }
    }
}
