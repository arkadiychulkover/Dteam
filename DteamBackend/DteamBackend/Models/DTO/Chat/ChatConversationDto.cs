using DteamBackend.Models.Enums;

namespace DteamBackend.Models.DTO.Chat
{
    public class ChatConversationDto
    {
        public Guid FriendId { get; set; }
        public string FriendUsername { get; set; } = string.Empty;
        public string? FriendAvatarUrl { get; set; }
        public UserStatus FriendStatus { get; set; } = UserStatus.Offline;
        public ChatMessageDto? LastMessage { get; set; }
        public int UnreadCount { get; set; }
        public DateTimeOffset? LastActivityAt { get; set; }
    }
}
