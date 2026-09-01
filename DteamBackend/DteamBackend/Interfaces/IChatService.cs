using DteamBackend.Models.DTO.Chat;

namespace DteamBackend.Interfaces
{
    public interface IChatService
    {
        Task<ChatMessageDto> SendMessageAsync(Guid currentUserId, SendMessageRequestDto dto, CancellationToken cancellationToken = default);
        Task<CursorHistoryResponseDto> GetHistoryAsync(Guid currentUserId, Guid friendId, string? cursor, int limit = 50, CancellationToken cancellationToken = default);
        Task<List<ChatMessageDto>> GetMissedMessagesAsync(Guid currentUserId, Guid friendId, DateTimeOffset afterTimestamp, CancellationToken cancellationToken = default);
        Task<List<ChatConversationDto>> GetConversationsAsync(Guid currentUserId, CancellationToken cancellationToken = default);
        Task<bool> MarkAsReadAsync(Guid currentUserId, Guid messageId, CancellationToken cancellationToken = default);
        Task<bool> ClearHistoryAsync(Guid currentUserId, Guid friendId, CancellationToken cancellationToken = default);
        Task<ChatMediaSummaryDto> GetMediaSummaryAsync(Guid currentUserId, Guid friendId, CancellationToken cancellationToken = default);
        Task<ChatUploadResponseDto> UploadFileAsync(Guid currentUserId, IFormFile file, int? duration, CancellationToken cancellationToken = default);
        Task<(Stream Stream, string ContentType, string FileName)?> GetMediaContentAsync(Guid currentUserId, Guid messageId, CancellationToken cancellationToken = default);
        Task SendTypingAsync(Guid currentUserId, Guid receiverId, bool isTyping, CancellationToken cancellationToken = default);
    }
}
