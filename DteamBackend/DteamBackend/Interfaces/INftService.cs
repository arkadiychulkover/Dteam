using DteamBackend.Models;
using DteamBackend.Models.Enums;

namespace DteamBackend.Interfaces
{
    public interface INftService
    {
        Task EnsureNftCollectionInitializedAsync();
        Task<List<NftItem>> GetAllNftsAsync(NftRarity? rarity = null, bool? isMinted = null);
        Task<NftItem?> GetNftByIdAsync(Guid id);
        Task<NftItem?> GetNftByTokenIdAsync(int tokenId);
        Task<int> GetTotalCountAsync();
        Task<List<NftItem>> GetUserGiftsAsync(Guid userId);
        Task<List<NftTransfer>> GetTransfersByNftIdAsync(Guid nftId);
        Task<List<NftTransfer>> GetTransfersByTokenIdAsync(int tokenId);
        Task<NftItem> MintNftAsync(string recipientAddress, int? tokenId = null, string? customDescription = null);
        Task<NftItem> MintToUserAsync(Guid userId, int? tokenId = null, string? customDescription = null);
    }
}
