using System.Text.Json;
using DteamBackend.Data;
using DteamBackend.Hubs;
using DteamBackend.Interfaces;
using DteamBackend.Models;
using DteamBackend.Models.DTO.Notification;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace DteamBackend.Services
{
    public class NotificationService : INotificationService
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            AppDbContext context,
            IHubContext<NotificationHub> hubContext,
            ILogger<NotificationService> logger)
        {
            _context = context;
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task<NotificationDto?> NotifyAsync(CreateNotificationCommand command)
        {
            try
            {
                // 1. Проверка идемпотентности (если передан EventId, например хеш транзакции)
                if (!string.IsNullOrWhiteSpace(command.EventId))
                {
                    var exists = await _context.Notifications
                        .AnyAsync(n => n.UserId == command.UserId && n.EventId == command.EventId && !n.IsDeleted);

                    if (exists)
                    {
                        _logger.LogInformation(
                            "[NotificationService] Notification with EventId {EventId} for user {UserId} already exists, skipping duplicate.",
                            command.EventId, command.UserId);
                        return null;
                    }
                }

                // 2. Проверка настроек приватности пользователя (Policy)
                var preferences = await _context.UserNotificationPreferences
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.UserId == command.UserId);

                if (!NotificationPolicy.IsEnabled(preferences, command.Type))
                {
                    _logger.LogInformation(
                        "[NotificationService] User {UserId} has disabled notifications for type {Type}, skipping.",
                        command.UserId, command.Type);
                    return null;
                }

                // 3. Сохранение уведомления в БД
                var notification = new Notification
                {
                    Id = Guid.NewGuid(),
                    UserId = command.UserId,
                    ActorUserId = command.ActorUserId,
                    Type = command.Type,
                    EntityType = command.EntityType,
                    EntityId = command.EntityId,
                    EventId = command.EventId,
                    Title = command.Title,
                    Message = command.Message,
                    DataJson = command.Data != null ? JsonSerializer.Serialize(command.Data) : null,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };

                await _context.Notifications.AddAsync(notification);
                await _context.SaveChangesAsync();

                // 4. Загрузка данных инициатора (Actor)
                ActorDto? actorDto = null;
                if (command.ActorUserId.HasValue)
                {
                    actorDto = await _context.Users
                        .AsNoTracking()
                        .Where(u => u.Id == command.ActorUserId.Value)
                        .Select(u => new ActorDto
                        {
                            Id = u.Id,
                            Username = u.Username,
                            AvatarUrl = u.AvatarUrl
                        })
                        .FirstOrDefaultAsync();
                }

                var dto = new NotificationDto
                {
                    Id = notification.Id,
                    Type = notification.Type,
                    Title = notification.Title,
                    Message = notification.Message,
                    EntityType = notification.EntityType,
                    EntityId = notification.EntityId,
                    EventId = notification.EventId,
                    Actor = actorDto,
                    DataJson = notification.DataJson,
                    IsRead = notification.IsRead,
                    CreatedAt = notification.CreatedAt,
                    ReadAt = notification.ReadAt
                };

                // 5. Отправка в реальном времени через SignalR
                try
                {
                    await _hubContext.Clients.User(command.UserId.ToString()).SendAsync("ReceiveNotification", dto);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "[NotificationService] Failed to send realtime notification to user {UserId}", command.UserId);
                }

                return dto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[NotificationService] Error in NotifyAsync for user {UserId}", command.UserId);
                return null;
            }
        }

        public async Task<NotificationFeedResponseDto> GetFeedAsync(Guid userId, int limit = 30, DateTime? before = null)
        {
            if (limit < 1) limit = 30;
            if (limit > 100) limit = 100;

            var query = _context.Notifications
                .AsNoTracking()
                .Include(n => n.ActorUser)
                .Where(n => n.UserId == userId && !n.IsDeleted);

            if (before.HasValue)
            {
                query = query.Where(n => n.CreatedAt < before.Value);
            }

            var items = await query
                .OrderByDescending(n => n.CreatedAt)
                .Take(limit + 1)
                .Select(n => new NotificationDto
                {
                    Id = n.Id,
                    Type = n.Type,
                    Title = n.Title,
                    Message = n.Message,
                    EntityType = n.EntityType,
                    EntityId = n.EntityId,
                    EventId = n.EventId,
                    Actor = n.ActorUser != null ? new ActorDto
                    {
                        Id = n.ActorUser.Id,
                        Username = n.ActorUser.Username,
                        AvatarUrl = n.ActorUser.AvatarUrl
                    } : null,
                    DataJson = n.DataJson,
                    IsRead = n.IsRead,
                    CreatedAt = n.CreatedAt,
                    ReadAt = n.ReadAt
                })
                .ToListAsync();

            bool hasMore = items.Count > limit;
            if (hasMore)
            {
                items.RemoveAt(limit);
            }

            string? nextCursor = hasMore && items.Count > 0
                ? items.Last().CreatedAt.ToString("o")
                : null;

            var unreadCount = await _context.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead && !n.IsDeleted);

            return new NotificationFeedResponseDto
            {
                Items = items,
                UnreadCount = unreadCount,
                NextCursor = nextCursor,
                HasMore = hasMore
            };
        }

        public async Task<bool> MarkAsReadAsync(Guid userId, Guid notificationId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId && !n.IsDeleted);

            if (notification == null)
            {
                return false;
            }

            if (!notification.IsRead)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            return true;
        }

        public async Task<int> MarkAllAsReadAsync(Guid userId)
        {
            var unreadNotifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead && !n.IsDeleted)
                .ToListAsync();

            if (unreadNotifications.Count == 0)
            {
                return 0;
            }

            var now = DateTime.UtcNow;
            foreach (var n in unreadNotifications)
            {
                n.IsRead = true;
                n.ReadAt = now;
            }

            await _context.SaveChangesAsync();
            return unreadNotifications.Count;
        }

        public async Task<bool> DeleteAsync(Guid userId, Guid notificationId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId && !n.IsDeleted);

            if (notification == null)
            {
                return false;
            }

            notification.IsDeleted = true;
            notification.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetUnreadCountAsync(Guid userId)
        {
            return await _context.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead && !n.IsDeleted);
        }
    }
}
