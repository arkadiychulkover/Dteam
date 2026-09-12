using DteamBackend.Data;

namespace DteamBackend.Interfaces
{
    public interface IInitDataService
    {
        Task EnsureAllSchemasAsync(AppDbContext context);
        Task InitializeAsync(AppDbContext context);
        Task EnsureCommunityDataAsync(AppDbContext context);
        Task EnsureReviewSchemaAsync(AppDbContext context);
        Task EnsureChatSchemaAsync(AppDbContext context);
        Task EnsureActivitySchemaAsync(AppDbContext context);
        Task EnsureUserOnlineTrackingSchemaAsync(AppDbContext context);
        Task EnsureTasteVectorSchemaAsync(AppDbContext context);
        Task EnsureNotificationAndSettingsSchemaAsync(AppDbContext context);
        Task EnsureCollectionSchemaAsync(AppDbContext context);
        Task EnsureGamesSeededAsync(AppDbContext context);
    }
}
