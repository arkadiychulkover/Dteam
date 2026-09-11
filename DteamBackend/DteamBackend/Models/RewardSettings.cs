using System;
using System.ComponentModel.DataAnnotations;

namespace DteamBackend.Models
{
    public class RewardSettings
    {
        [Key]
        public int Id { get; set; } = 1;

        [Range(1, 10080, ErrorMessage = "Інтервал видачі нагороди має бути від 1 хвилини до 7 днів (10080 хв).")]
        public int RewardIntervalMinutes { get; set; } = 60;

        [Range(0, 1000000, ErrorMessage = "Кількість токенів за годину має бути додатньою.")]
        public decimal TokensPerHour { get; set; } = 50m;

        public bool IsEnabled { get; set; } = true;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
