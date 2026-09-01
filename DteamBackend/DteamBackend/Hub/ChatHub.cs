using System.Security.Claims;
using DteamBackend.Interfaces;
using DteamBackend.Models.DTO.Chat;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace DteamBackend.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly ILogger<ChatHub> _logger;

        public ChatHub(ILogger<ChatHub> logger)
        {
            _logger = logger;
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                           ?? Context.User?.FindFirst("sub")?.Value;

            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = GetCurrentUserId();
            if (userId != Guid.Empty)
            {
                var groupName = $"user:{userId}";
                await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
                _logger.LogInformation("[ChatHub] User {UserId} connected with connectionId {ConnectionId} and joined group {GroupName}", userId, Context.ConnectionId, groupName);
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = GetCurrentUserId();
            if (userId != Guid.Empty)
            {
                var groupName = $"user:{userId}";
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
                _logger.LogInformation("[ChatHub] User {UserId} disconnected with connectionId {ConnectionId}", userId, Context.ConnectionId);
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}
