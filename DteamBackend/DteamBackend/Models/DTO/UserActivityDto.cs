using DteamBackend.Models.Enums;

namespace DteamBackend.Models.DTO
{
    public class UserActivityDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? UserAvatarUrl { get; set; }
        public UserActivityType Type { get; set; }
        public string TypeName => Type.ToString();
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Details { get; set; }
        public Guid? RelatedEntityId { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
