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
                           ?? Context.User?.FindFirst("sub")?.Value
                           ?? Context.User?.FindFirst("nameid")?.Value
                           ?? Context.UserIdentifier;

            if (Guid.TryParse(userIdClaim, out var userId))
            {
                return userId;
            }

            try
            {
                var httpContext = Context.GetHttpContext();
                var queryToken = httpContext?.Request.Query["access_token"].ToString();
                if (string.IsNullOrEmpty(queryToken))
                {
                    queryToken = httpContext?.Request.Query["token"].ToString();
                }

                if (!string.IsNullOrEmpty(queryToken))
                {
                    var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                    if (handler.CanReadToken(queryToken))
                    {
                        var jwt = handler.ReadJwtToken(queryToken);
                        var sub = jwt.Claims.FirstOrDefault(c =>
                            c.Type == System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub ||
                            c.Type == ClaimTypes.NameIdentifier ||
                            c.Type == "nameid")?.Value;

                        if (Guid.TryParse(sub, out var parsedGuid))
                        {
                            return parsedGuid;
                        }
                    }
                }
            }
            catch
            {
            }

            return Guid.Empty;
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

                await Groups.AddToGroupAsync(connectionId, userId.ToString().ToLowerInvariant());

                var friendIds = await _context.UserFriends
                    .AsNoTracking()
                    .Where(uf => uf.UserId == userId || uf.FriendId == userId)
                    .Select(uf => uf.UserId == userId ? uf.FriendId : uf.UserId)
                    .Distinct()
                    .ToListAsync();

                var onlineFriendIds = friendIds
                    .Where(fId => ConnectedUsers.TryGetValue(fId, out var conns) && conns.Count > 0)
                    .Select(fId => fId.ToString().ToLowerInvariant())
                    .ToList();

                await Clients.Caller.SendAsync("OnlineFriendsList", onlineFriendIds);

                if (becameOnline && friendIds.Count > 0)
                {
                    var friendUserIds = friendIds.Select(id => id.ToString().ToLowerInvariant()).ToList();
                    await Clients.Users(friendUserIds).SendAsync("UserOnline", userId.ToString().ToLowerInvariant());
                    await Clients.Groups(friendUserIds).SendAsync("UserOnline", userId.ToString().ToLowerInvariant());
                    await Clients.Users(friendUserIds).SendAsync("UserStatusChanged", new
                    {
                        userId = userId.ToString().ToLowerInvariant(),
                        status = "Online"
                    });
                    await Clients.Groups(friendUserIds).SendAsync("UserStatusChanged", new
                    {
                        userId = userId.ToString().ToLowerInvariant(),
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
                await Groups.RemoveFromGroupAsync(connectionId, userId.ToString().ToLowerInvariant());
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
                        .Where(uf => uf.UserId == userId || uf.FriendId == userId)
                        .Select(uf => uf.UserId == userId ? uf.FriendId : uf.UserId)
                        .Distinct()
                        .ToListAsync();

                    if (friendIds.Count > 0)
                    {
                        var friendUserIds = friendIds.Select(id => id.ToString().ToLowerInvariant()).ToList();
                        await Clients.Users(friendUserIds).SendAsync("UserOffline", userId.ToString().ToLowerInvariant());
                        await Clients.Groups(friendUserIds).SendAsync("UserOffline", userId.ToString().ToLowerInvariant());
                        await Clients.Users(friendUserIds).SendAsync("UserStatusChanged", new
                        {
                            userId = userId.ToString().ToLowerInvariant(),
                            status = "Offline"
                        });
                        await Clients.Groups(friendUserIds).SendAsync("UserStatusChanged", new
                        {
                            userId = userId.ToString().ToLowerInvariant(),
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

