using DteamBackend.Data;
using DteamBackend.Interfaces;
using DteamBackend.Models;
using DteamBackend.Models.DTO;
using DteamBackend.Models.DTO.Notification;
using DteamBackend.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace DteamBackend.Services
{
    public interface IAccountService
    {
        Task<(bool Success, string? Error)> ChangePasswordAsync(Guid userId, ChangePasswordDto dto);
        Task<(bool Success, string? Error)> DeleteAccountAsync(Guid userId, DeleteAccountDto dto);
    }

    public class AccountService : IAccountService
    {
        private readonly AppDbContext _db;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly INotificationService _notificationService;
        private readonly ILogger<AccountService> _logger;

        public AccountService(
            AppDbContext db,
            IPasswordHasher passwordHasher,
            IJwtTokenService jwtTokenService,
            INotificationService notificationService,
            ILogger<AccountService> logger)
        {
            _db = db;
            _passwordHasher = passwordHasher;
            _jwtTokenService = jwtTokenService;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<(bool Success, string? Error)> ChangePasswordAsync(Guid userId, ChangePasswordDto dto)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return (false, "Користувача не знайдено.");
            }

            if (!_passwordHasher.VerifyPasswordHash(dto.OldPassword, user.PasswordHash, user.PasswordSalt))
            {
                return (false, "Невірний поточний пароль.");
            }

            var newPass = dto.NewPassword.Trim();
            if (_passwordHasher.VerifyPasswordHash(newPass, user.PasswordHash, user.PasswordSalt))
            {
                return (false, "Новий пароль не повинен співпадати з поточним паролем.");
            }

            _passwordHasher.CreatePasswordHash(newPass, out string newHash, out string newSalt);
            user.PasswordHash = newHash;
            user.PasswordSalt = newSalt;
            user.UpdatedAt = DateTime.UtcNow;

            await _jwtTokenService.RevokeUserTokensAsync(userId);

            await _db.SaveChangesAsync();
            _logger.LogInformation($"[AccountService] User {userId} changed password successfully. All refresh tokens revoked.");

            try
            {
                await _notificationService.NotifyAsync(new CreateNotificationCommand
                {
                    UserId = userId,
                    Type = NotificationTypes.SecurityAlert,
                    Title = "Безпека акаунту",
                    Message = "Пароль вашого облікового запису було успішно змінено. Якщо це були не ви, негайно зверніться до підтримки."
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[AccountService] Failed to send password change notification to user {UserId}", userId);
            }

            return (true, null);
        }

        public async Task<(bool Success, string? Error)> DeleteAccountAsync(Guid userId, DeleteAccountDto dto)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return (false, "Користувача не знайдено.");
            }

            if (!string.Equals(user.Username.Trim(), dto.UsernameConfirmation.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return (false, "Введений нікнейм не співпадає з вашим поточним нікнеймом.");
            }

            if (!_passwordHasher.VerifyPasswordHash(dto.Password, user.PasswordHash, user.PasswordSalt))
            {
                return (false, "Невірний пароль для підтвердження видалення.");
            }

            await _jwtTokenService.RevokeUserTokensAsync(userId);

            user.IsDeleted = true;
            user.DeletedAt = DateTime.UtcNow;
            user.Status = UserStatus.Offline;

            user.Email = $"deleted_{user.Id:N}@deleted.local";
            user.Username = $"deleted_{user.Id:N}"[..16];
            user.Bio = null;
            user.AvatarUrl = null;
            user.BannerUrl = null;
            user.WalletAddress = null;
            user.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            _logger.LogInformation($"[AccountService] User {userId} soft-deleted and anonymized successfully.");

            return (true, null);
        }
    }
}
