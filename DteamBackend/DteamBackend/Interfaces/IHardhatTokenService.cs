using System;
using System.Threading.Tasks;

namespace DteamBackend.Interfaces
{
    public interface IHardhatTokenService
    {

        Task<string> AwardTokensAsync(Guid userId, decimal amount);

        Task<string> AwardTokensByAddressAsync(string recipientAddress, decimal amount);

        Task<string> DebitTokensAsync(Guid userId, decimal amount);

        Task<string> DebitTokensByAddressAsync(string fromAddress, decimal amount);

        Task<decimal> GetBalanceAsync(string walletAddress);

        Task<string> UpdateAdminAddressFromSettingsAsync();

        Task<(bool isMatch, string? registeredAddress, string providedAddress)> VerifyWalletMatchAsync(Guid userId, string providedAddress);
    }
}

