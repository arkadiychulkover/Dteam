using System.ComponentModel.DataAnnotations;
using DteamBackend.Validation;

namespace DteamBackend.Models.DTO
{
    public class SettingsResponseDto
    {
        public UserProfileSettingsDto Profile { get; set; } = new();
        public NotificationPreferencesDto Preferences { get; set; } = new();
        public WalletSummaryDto WalletSummary { get; set; } = new();
    }

    public class UserProfileSettingsDto
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Bio { get; set; }
        public string? AvatarUrl { get; set; }
        public string? BannerUrl { get; set; }
        public string PreferredLanguage { get; set; } = "uk";
        public DateTime CreatedAt { get; set; }
    }

    public class WalletSummaryDto
    {
        public long BalanceInNanoTons { get; set; }
        public decimal FormattedBalance { get; set; }
        public string Currency { get; set; } = "TON";
    }

    public class UpdateGeneralSettingsDto
    {
        [Required(ErrorMessage = "Нікнейм обов'язковий")]
        [MinLength(3, ErrorMessage = "Нікнейм має містити щонайменше 3 символи")]
        [MaxLength(50, ErrorMessage = "Нікнейм не може перевищувати 50 символів")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email обов'язковий")]
        [EmailAddress(ErrorMessage = "Невірний формат email")]
        [MaxLength(256)]
        public string Email { get; set; } = string.Empty;

        [MaxLength(100, ErrorMessage = "Опис про себе не може перевищувати 100 символів")]
        public string? Bio { get; set; }

        [MaxLength(500)]
        public string? AvatarUrl { get; set; }

        [MaxLength(500)]
        public string? BannerUrl { get; set; }

        [MaxLength(10)]
        public string? PreferredLanguage { get; set; }
    }

    public class ChangePasswordDto
    {
        [Required(ErrorMessage = "Поточний пароль обов'язковий")]
        public string OldPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Новий пароль обов'язковий")]
        [StrongPassword(MinLength = 7)]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Підтвердження паролю обов'язкове")]
        [Compare(nameof(NewPassword), ErrorMessage = "Паролі не співпадають")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }

    public class NotificationPreferencesDto
    {
        public bool NotifyBigSales { get; set; } = true;
        public bool NotifyWishlistDiscounts { get; set; } = true;
        public bool NotifyProfileComments { get; set; } = true;
        public bool NotifyFriendRequests { get; set; } = true;
        public bool NotifyFriendRequestAccepted { get; set; } = true;
        public bool NotifyFriendRequestDeclined { get; set; } = true;
        public bool ChatNotificationsEnabled { get; set; } = true;
        public bool ChatSoundEnabled { get; set; } = true;
    }

    public class WalletTransactionItemDto
    {
        public Guid Id { get; set; }
        public decimal Amount { get; set; }
        public string Type { get; set; } = "Deposit";
        public string Title { get; set; } = string.Empty;
        public string Currency { get; set; } = "TON";
        public DateTime Date { get; set; }
    }

    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
    }

    public class DeleteAccountDto
    {
        [Required(ErrorMessage = "Підтвердження нікнейму обов'язкове")]
        public string UsernameConfirmation { get; set; } = string.Empty;

        [Required(ErrorMessage = "Пароль обов'язковий")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Повторіть пароль")]
        [Compare(nameof(Password), ErrorMessage = "Паролі не співпадають")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
