using System.Collections.Concurrent;
using System.Security.Claims;
using DteamBackend.Data;
using DteamBackend.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace DteamBackend.Hubs
{
    [Authorize]
    public class FriendsHub : Hub
    {
        private readonly AppDbContext _context;
        private readonly ILogger<FriendsHub> _logger;

        private static readonly ConcurrentDictionary<Guid, HashSet<string>> ConnectedUsers = new();

        public FriendsHub(AppDbContext context, ILogger<FriendsHub> logger)
        {
            _context = context;
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
                var connectionId = Context.ConnectionId;
                bool becameOnline = false;

                ConnectedUsers.AddOrUpdate(userId,
                    _ =>
                    {
                        becameOnline = true;
                        return new HashSet<string> { connectionId };
                    },
                    (_, connections) =>
                    {
                        lock (connections)
                        {
                            if (connections.Count == 0)
                            {
                                becameOnline = true;
                            }
                            connections.Add(connectionId);
                        }
                        return connections;
                    });

                var friendIds = await _context.UserFriends
                    .AsNoTracking()
                    .Where(f => f.UserId == userId && f.Status == FriendshipStatus.Accepted)
                    .Select(f => f.FriendId)
                    .ToListAsync();

                var onlineFriendIds = friendIds
                    .Where(fId => ConnectedUsers.TryGetValue(fId, out var conns) && conns.Count > 0)
                    .Select(fId => fId.ToString())
                    .ToList();

                await Clients.Caller.SendAsync("OnlineFriendsList", onlineFriendIds);

                if (becameOnline && friendIds.Count > 0)
                {
                    var friendUserIds = friendIds.Select(id => id.ToString()).ToList();
                    await Clients.Users(friendUserIds).SendAsync("UserOnline", userId.ToString());
                    await Clients.Users(friendUserIds).SendAsync("UserStatusChanged", new
                    {
                        userId = userId.ToString(),
                        status = "Online"
                    });
                }

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (user != null)
                {
                    user.Status = UserStatus.Online;
                    user.LastLoginAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = GetCurrentUserId();
            if (userId != Guid.Empty)
            {
                var connectionId = Context.ConnectionId;
                bool becameOffline = false;

                if (ConnectedUsers.TryGetValue(userId, out var connections))
                {
                    lock (connections)
                    {
                        connections.Remove(connectionId);
                        if (connections.Count == 0)
                        {
                            becameOffline = true;
                        }
                    }

                    if (becameOffline)
                    {
                        ConnectedUsers.TryRemove(userId, out _);
                    }
                }

                if (becameOffline)
                {
                    var friendIds = await _context.UserFriends
                        .AsNoTracking()
                        .Where(f => f.UserId == userId && f.Status == FriendshipStatus.Accepted)
                        .Select(f => f.FriendId)
                        .ToListAsync();

                    if (friendIds.Count > 0)
                    {
                        var friendUserIds = friendIds.Select(id => id.ToString()).ToList();
                        await Clients.Users(friendUserIds).SendAsync("UserOffline", userId.ToString());
                        await Clients.Users(friendUserIds).SendAsync("UserStatusChanged", new
                        {
                            userId = userId.ToString(),
                            status = "Offline"
                        });
                    }

                    var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                    if (user != null)
                    {
                        user.Status = UserStatus.Offline;
                        user.LastLoginAt = DateTime.UtcNow;
                        await _context.SaveChangesAsync();
                    }
                }
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}

