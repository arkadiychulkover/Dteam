using DteamBackend.Data;

namespace DteamBackend.Interfaces
{
    public interface IInitDataService
    {
        Task InitializeAsync(AppDbContext context);
        Task EnsureCommunityDataAsync(AppDbContext context);
        Task EnsureReviewSchemaAsync(AppDbContext context);
        Task EnsureChatSchemaAsync(AppDbContext context);
        Task EnsureActivitySchemaAsync(AppDbContext context);
        Task EnsureUserOnlineTrackingSchemaAsync(AppDbContext context);
        Task EnsureTasteVectorSchemaAsync(AppDbContext context);
    }
}
