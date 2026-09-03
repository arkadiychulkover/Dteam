using System;
using System.Threading.Tasks;

namespace DteamBackend.Interfaces
{
    public interface IHardhatTokenService
    {
        /// <summary>
        /// Начисление токенов пользователю по его UserId (ищет HardhatAddress в базе)
        /// </summary>
        Task<string> AwardTokensAsync(Guid userId, decimal amount);

        /// <summary>
        /// Начисление токенов напрямую по адресу кошелька
        /// </summary>
        Task<string> AwardTokensByAddressAsync(string recipientAddress, decimal amount);

        /// <summary>
        /// Снятие (сжигание) токенов у пользователя по его UserId
        /// </summary>
        Task<string> DebitTokensAsync(Guid userId, decimal amount);

        /// <summary>
        /// Снятие (сжигание) токенов напрямую по адресу кошелька
        /// </summary>
        Task<string> DebitTokensByAddressAsync(string fromAddress, decimal amount);

        /// <summary>
        /// Получение баланса токенов кошелька
        /// </summary>
        Task<decimal> GetBalanceAsync(string walletAddress);

        /// <summary>
        /// Обновление адреса администратора в БД из appsettings.json
        /// </summary>
        Task<string> UpdateAdminAddressFromSettingsAsync();

        /// <summary>
        /// Проверка соответствия текущего кошелька с фронта с зарегистрированным в БД
        /// </summary>
        Task<(bool isMatch, string? registeredAddress, string providedAddress)> VerifyWalletMatchAsync(Guid userId, string providedAddress);
    }
}
