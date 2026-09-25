using System.Text.Json;

namespace DteamBackend.Models.DTO.Notification
{
    public class CreateNotificationCommand
    {
        public Guid UserId { get; set; }
        public Guid? ActorUserId { get; set; }
        public string Type { get; set; } = string.Empty;
        public string? EntityType { get; set; }
        public Guid? EntityId { get; set; }
        public string? EventId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public object? Data { get; set; }
    }

    public class ActorDto
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
    }

    public class NotificationDto
    {
        public Guid Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? EntityType { get; set; }
        public Guid? EntityId { get; set; }
        public string? EventId { get; set; }
        public ActorDto? Actor { get; set; }
        public string? DataJson { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ReadAt { get; set; }
    }

    public class NotificationFeedResponseDto
    {
        public List<NotificationDto> Items { get; set; } = new();
        public int UnreadCount { get; set; }
        public string? NextCursor { get; set; }
        public bool HasMore { get; set; }
    }
}
