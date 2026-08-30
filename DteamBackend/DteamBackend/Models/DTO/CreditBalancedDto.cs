using System.ComponentModel.DataAnnotations;

namespace DteamBackend.Models.DTO
{
    public class CreditBalanceDto
    {
        [Required(ErrorMessage = "Сума поповнення обов'язкова.")]
        [Range(1, long.MaxValue, ErrorMessage = "Сума має бути більшою за 0.")]
        public long AmountInNanoTons { get; set; }

        [MaxLength(500)]
        public string? Reason { get; set; }
    }
}