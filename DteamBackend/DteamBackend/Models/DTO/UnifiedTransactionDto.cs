using System;

namespace DteamBackend.Models.DTO
{
    public class UnifiedTransactionDto
    {
        public Guid Id { get; set; }
        public string Type { get; set; } = "deposit"; // "deposit" | "purchase" | "reward" | "payout"
        public string Title { get; set; } = string.Empty;
        public decimal AmountTon { get; set; }
        public long AmountNanoTon { get; set; }
        public string? TxHash { get; set; }
        public string Status { get; set; } = "completed"; // "completed" | "pending" | "failed"
        public DateTime CreatedAt { get; set; }
        public string? GameCoverUrl { get; set; }
        public Guid? GameId { get; set; }
    }
}
