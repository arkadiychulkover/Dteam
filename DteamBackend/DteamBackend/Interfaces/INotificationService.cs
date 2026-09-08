using DteamBackend.Models.DTO.Notification;

namespace DteamBackend.Interfaces
{
    public interface INotificationService
    {
        Task<NotificationDto?> NotifyAsync(CreateNotificationCommand command);
        Task<NotificationFeedResponseDto> GetFeedAsync(Guid userId, int limit = 30, DateTime? before = null);
        Task<bool> MarkAsReadAsync(Guid userId, Guid notificationId);
        Task<int> MarkAllAsReadAsync(Guid userId);
        Task<bool> DeleteAsync(Guid userId, Guid notificationId);
        Task<int> GetUnreadCountAsync(Guid userId);
    }
}
