using DteamBackend.Data;
using DteamBackend.Interfaces;
using DteamBackend.Models;
using DteamBackend.Models.DTO;
using DteamBackend.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace DteamBackend.Services
{
    public class ActivityService : IActivityService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ActivityService> _logger;

        public ActivityService(AppDbContext context, ILogger<ActivityService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<UserActivity> LogActivityAsync(
            Guid userId,
            UserActivityType type,
            string title,
            string? description = null,
            string? details = null,
            Guid? relatedEntityId = null,
            string? imageUrl = null)
        {
            try
            {
                var activity = new UserActivity
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Type = type,
                    Title = title,
                    Description = description,
                    Details = details,
                    RelatedEntityId = relatedEntityId,
                    ImageUrl = imageUrl,
                    CreatedAt = DateTime.UtcNow
                };

                await _context.UserActivities.AddAsync(activity);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"[ActivityService] Logged activity {type} for User {userId}: '{title}'");
                return activity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[ActivityService] Error logging activity for User {userId}");
                throw;
            }
        }

        public async Task<List<UserActivityDto>> GetFriendsActivitiesAsync(Guid userId, int limit = 50, int offset = 0)
        {
            // 1. Get all friends of the user
            var friendIds = await _context.Users
                .AsNoTracking()
                .Where(u => u.Id == userId)
                .SelectMany(u => u.Friends.Select(f => f.Id))
                .ToListAsync();

            // Include current user's activities as well so they see their own timeline too
            var targetUserIds = new HashSet<Guid>(friendIds) { userId };

            var activities = await _context.UserActivities
                .Include(a => a.User)
                .AsNoTracking()
                .Where(a => targetUserIds.Contains(a.UserId))
                .OrderByDescending(a => a.CreatedAt)
                .Skip(offset)
                .Take(Math.Min(limit, 100))
                .ToListAsync();

            return activities.Select(MapToDto).ToList();
        }

        public async Task<List<UserActivityDto>> GetUserActivitiesAsync(Guid userId, int limit = 50, int offset = 0)
        {
            var activities = await _context.UserActivities
                .Include(a => a.User)
                .AsNoTracking()
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.CreatedAt)
                .Skip(offset)
                .Take(Math.Min(limit, 100))
                .ToListAsync();

            return activities.Select(MapToDto).ToList();
        }

        public async Task<List<UserActivityDto>> GetGlobalFeedAsync(int limit = 50, int offset = 0)
        {
            var activities = await _context.UserActivities
                .Include(a => a.User)
                .AsNoTracking()
                .OrderByDescending(a => a.CreatedAt)
                .Skip(offset)
                .Take(Math.Min(limit, 100))
                .ToListAsync();

            return activities.Select(MapToDto).ToList();
        }

        private static UserActivityDto MapToDto(UserActivity a) => new()
        {
            Id = a.Id,
            UserId = a.UserId,
            Username = a.User?.Username ?? "Користувач",
            UserAvatarUrl = a.User?.AvatarUrl,
            Type = a.Type,
            Title = a.Title,
            Description = a.Description,
            Details = a.Details,
            RelatedEntityId = a.RelatedEntityId,
            ImageUrl = a.ImageUrl,
            CreatedAt = a.CreatedAt
        };
    }
}
