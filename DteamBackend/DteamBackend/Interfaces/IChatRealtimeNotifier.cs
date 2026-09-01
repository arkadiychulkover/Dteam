using DteamBackend.Models.DTO.Chat;

namespace DteamBackend.Interfaces
{
    public interface IChatRealtimeNotifier
    {
        Task NotifyMessageReceivedAsync(Guid recipientUserId, ChatMessageDto message, CancellationToken cancellationToken = default);
        Task NotifyMessageReadAsync(Guid recipientUserId, Guid messageId, Guid readerUserId, DateTimeOffset readAt, CancellationToken cancellationToken = default);
        Task NotifyUserTypingAsync(Guid recipientUserId, Guid typingUserId, bool isTyping, CancellationToken cancellationToken = default);
        Task NotifyHistoryClearedAsync(Guid recipientUserId, Guid clearedWithUserId, CancellationToken cancellationToken = default);
    }
}
