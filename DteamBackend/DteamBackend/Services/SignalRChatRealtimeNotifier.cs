using DteamBackend.Hubs;
using DteamBackend.Interfaces;
using DteamBackend.Models.DTO.Chat;
using Microsoft.AspNetCore.SignalR;

namespace DteamBackend.Services
{
    public class SignalRChatRealtimeNotifier : IChatRealtimeNotifier
    {
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly ILogger<SignalRChatRealtimeNotifier> _logger;

        public SignalRChatRealtimeNotifier(IHubContext<ChatHub> hubContext, ILogger<SignalRChatRealtimeNotifier> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task NotifyMessageReceivedAsync(Guid recipientUserId, ChatMessageDto message, CancellationToken cancellationToken = default)
        {
            try
            {
                var groupName = $"user:{recipientUserId}";
                await _hubContext.Clients.Group(groupName).SendAsync("ReceiveMessage", message, cancellationToken);
                _logger.LogDebug("[SignalRChatNotifier] Sent ReceiveMessage to {Group} for message {MessageId}", groupName, message.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[SignalRChatNotifier] Error notifying user {UserId} of received message {MessageId}", recipientUserId, message.Id);
            }
        }

        public async Task NotifyMessageReadAsync(Guid recipientUserId, Guid messageId, Guid readerUserId, DateTimeOffset readAt, CancellationToken cancellationToken = default)
        {
            try
            {
                var groupName = $"user:{recipientUserId}";
                await _hubContext.Clients.Group(groupName).SendAsync("MessageRead", new
                {
                    messageId,
                    readerUserId,
                    readAt
                }, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[SignalRChatNotifier] Error sending MessageRead to user {UserId}", recipientUserId);
            }
        }

        public async Task NotifyUserTypingAsync(Guid recipientUserId, Guid typingUserId, bool isTyping, CancellationToken cancellationToken = default)
        {
            try
            {
                var groupName = $"user:{recipientUserId}";
                await _hubContext.Clients.Group(groupName).SendAsync("UserTyping", new
                {
                    userId = typingUserId,
                    isTyping
                }, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[SignalRChatNotifier] Error sending UserTyping to user {UserId}", recipientUserId);
            }
        }

        public async Task NotifyHistoryClearedAsync(Guid recipientUserId, Guid clearedWithUserId, CancellationToken cancellationToken = default)
        {
            try
            {
                var groupName = $"user:{recipientUserId}";
                await _hubContext.Clients.Group(groupName).SendAsync("HistoryCleared", new
                {
                    clearedWithUserId
                }, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[SignalRChatNotifier] Error sending HistoryCleared to user {UserId}", recipientUserId);
            }
        }

        public async Task NotifyMessageDeletedAsync(Guid recipientUserId, Guid messageId, Guid peerId, CancellationToken cancellationToken = default)
        {
            try
            {
                var groupName = $"user:{recipientUserId}";
                await _hubContext.Clients.Group(groupName).SendAsync("MessageDeleted", new
                {
                    messageId,
                    peerId
                }, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[SignalRChatNotifier] Error sending MessageDeleted to user {UserId}", recipientUserId);
            }
        }
    }
}
