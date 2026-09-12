using System;
using System.ComponentModel.DataAnnotations;

namespace DteamBackend.Models.DTO.Token
{
    public class VerifyWalletDto
    {
        public Guid? UserId { get; set; }

        [Required(ErrorMessage = "Wallet address is required")]
        public string WalletAddress { get; set; } = string.Empty;
    }

    public class VerifyWalletResponseDto
    {
        public bool IsMatch { get; set; }
        public string? RegisteredAddress { get; set; }
        public string ProvidedAddress { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    public class TokenOperationDto
    {
        public Guid? UserId { get; set; }
        public string? WalletAddress { get; set; }

        [Range(0.000000000000000001, double.MaxValue, ErrorMessage = "Amount must be positive")]
        public decimal Amount { get; set; }
    }

    public class TokenOperationResponseDto
    {
        public bool Success { get; set; }
        public string? TransactionHash { get; set; }
        public decimal Amount { get; set; }
        public string TargetAddress { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
