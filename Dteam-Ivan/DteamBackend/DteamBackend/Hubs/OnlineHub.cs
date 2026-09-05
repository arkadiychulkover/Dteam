using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace DteamBackend.Hubs
{
    public class OnlineHub : Hub
    {
        private static readonly ConcurrentDictionary<string, string> OnlineUsers = new();

        public override async Task OnConnectedAsync()
        {
            var connectionId = Context.ConnectionId;
            var userId = Context.UserIdentifier ?? connectionId;

            OnlineUsers.TryAdd(connectionId, userId);

            await Clients.All.SendAsync("UpdateOnlineCount", GetUniqueOnlineCount());

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var connectionId = Context.ConnectionId;
            OnlineUsers.TryRemove(connectionId, out _);

            await Clients.All.SendAsync("UpdateOnlineCount", GetUniqueOnlineCount());

            await base.OnDisconnectedAsync(exception);
        }

        private int GetUniqueOnlineCount()
        {
            return OnlineUsers.Values.Distinct().Count();
        }
    }
}