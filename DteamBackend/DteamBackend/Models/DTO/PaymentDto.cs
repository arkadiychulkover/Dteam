using System.ComponentModel.DataAnnotations;

namespace DteamBackend.Models.DTO
{
    public class VerifyTransactionDto
    {
        [Required(ErrorMessage = "TxhHash is required")]
        public string TxhHash { get; set; } = string.Empty;

        [Range(0.000000001, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }
    }

    public class TranxactionDto
    {
        public Guid Id { get; set; }
        public string TxhHash { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public Guid? UserId { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class PaymentVerificationResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public long NewBalanceInNanoTons { get; set; }
        public decimal Amount { get; set; }
        public string TxhHash { get; set; } = string.Empty;
    }
}

