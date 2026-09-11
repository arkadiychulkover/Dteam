using System;
using System.ComponentModel.DataAnnotations;

namespace DteamBackend.Models.DTO
{
    public class RewardSettingsDto
    {
        [Range(1, 10080, ErrorMessage = "Інтервал видачі нагороди має бути від 1 хвилини до 10080 хвилин.")]
        public int RewardIntervalMinutes { get; set; } = 60;

        [Range(0, 1000000, ErrorMessage = "Кількість токенів за годину має бути від 0 до 1,000,000.")]
        public decimal TokensPerHour { get; set; } = 50m;

        public bool IsEnabled { get; set; } = true;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public class UpdateRewardSettingsDto
    {
        [Range(1, 10080, ErrorMessage = "Інтервал видачі нагороди має бути від 1 хвилини до 10080 хвилин.")]
        public int RewardIntervalMinutes { get; set; } = 60;

        [Range(0, 1000000, ErrorMessage = "Кількість токенів за годину має бути від 0 до 1,000,000.")]
        public decimal TokensPerHour { get; set; } = 50m;

        public bool IsEnabled { get; set; } = true;
    }
}
