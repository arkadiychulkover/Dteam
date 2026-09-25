using DteamBackend.Models;
using DteamBackend.Models.DTO;
using DteamBackend.Models.Enums;

namespace DteamBackend.Interfaces
{
    public interface IActivityService
    {
        Task<UserActivity> LogActivityAsync(
            Guid userId,
            UserActivityType type,
            string title,
            string? description = null,
            string? details = null,
            Guid? relatedEntityId = null,
            string? imageUrl = null);

        Task<List<UserActivityDto>> GetFriendsActivitiesAsync(Guid userId, int limit = 50, int offset = 0);

        Task<List<UserActivityDto>> GetUserActivitiesAsync(Guid userId, int limit = 50, int offset = 0);

        Task<List<UserActivityDto>> GetGlobalFeedAsync(int limit = 50, int offset = 0);
    }
}
