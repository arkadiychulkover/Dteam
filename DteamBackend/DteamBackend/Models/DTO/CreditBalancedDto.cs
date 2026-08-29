using System.ComponentModel.DataAnnotations;

namespace DteamBackend.Models.DTO
{
    public class CreditBalanceDto
    {
        [Required]
        public long AmountInNanoTons { get; set; }

        [MaxLength(500)]
        public string? Reason { get; set; }
    }
}