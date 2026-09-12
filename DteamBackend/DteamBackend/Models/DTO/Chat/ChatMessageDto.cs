using DteamBackend.Models.Enums;

namespace DteamBackend.Models.DTO.Chat
{
    public class ChatMessageDto
    {
        public Guid Id { get; set; }
        public string ClientMessageId { get; set; } = string.Empty;
        public Guid SenderId { get; set; }
        public Guid ReceiverId { get; set; }
        public string? Content { get; set; }
        public ChatMessageType Type { get; set; }
        public MessageDeliveryStatus Status { get; set; }
        public string? MediaUrl { get; set; }
        public string? OriginalFileName { get; set; }
        public string? ContentType { get; set; }
        public long? FileSize { get; set; }
        public int? Duration { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? ReadAt { get; set; }
    }
}
