using System.Security.Claims;
using System.Text.Json;
using DteamBackend.Data;
using DteamBackend.Interfaces;
using DteamBackend.Models;
using DteamBackend.Models.DTO;
using DteamBackend.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using DteamBackend.Hubs;

using DteamBackend.Models.DTO.Notification;

namespace DteamBackend.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class FriendsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IActivityService _activityService;
        private readonly IHubContext<FriendsHub> _friendsHub;
        private readonly INotificationService _notificationService;

        public FriendsController(
            AppDbContext context,
            IActivityService activityService,
            IHubContext<FriendsHub> friendsHub,
            INotificationService notificationService)
        {
            _context = context;
            _activityService = activityService;
            _friendsHub = friendsHub;
            _notificationService = notificationService;
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                           ?? User.FindFirst("sub")?.Value;

            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }

        [HttpGet]
        [HttpGet("list")]
        [ProducesResponseType(typeof(List<FriendDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<List<FriendDto>>> GetFriends()
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == Guid.Empty)
            {
                return Unauthorized(new { message = "Користувач не авторизований." });
            }

            var friendships = await _context.UserFriends
                .AsNoTracking()
                .Where(uf => uf.UserId == currentUserId || uf.FriendId == currentUserId)
                .Include(uf => uf.User)
                .Include(uf => uf.Friend)
                .ToListAsync();

            var friends = friendships
                .Select(uf =>
                {
                    var friendUser = uf.UserId == currentUserId ? uf.Friend : uf.User;
                    return new
                    {
                        User = friendUser,
                        FriendsSince = uf.CreatedAt
                    };
                })
                .Where(x => x.User != null && x.User.Id != currentUserId && !x.User.IsDeleted)
                .GroupBy(x => x.User.Id)
                .Select(g =>
                {
                    var first = g.First();
                    return new FriendDto
                    {
                        Id = first.User.Id,
                        Username = first.User.Username,
                        Email = first.User.Email,
                        AvatarUrl = first.User.AvatarUrl,
                        Bio = first.User.Bio,
                        Status = first.User.Status,
                        LastLoginAt = first.User.LastLoginAt,
                        FriendsSince = first.FriendsSince
                    };
                })
                .OrderBy(f => f.Username)
                .ToList();

            return Ok(friends);
        }

        [HttpGet("game/{gameId:guid}")]
        [ProducesResponseType(typeof(FriendsGameStatusDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<FriendsGameStatusDto>> GetFriendsGameStatus(Guid gameId)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == Guid.Empty)
            {
                return Unauthorized(new { message = "Користувач не авторизований." });
            }

            var allFriendIds = await _context.UserFriends
                .AsNoTracking()
                .Where(uf => uf.UserId == currentUserId || uf.FriendId == currentUserId)
                .Select(uf => uf.UserId == currentUserId ? uf.FriendId : uf.UserId)
                .Distinct()
                .ToListAsync();

            if (!allFriendIds.Any())
            {
                return Ok(new FriendsGameStatusDto());
            }

            var friendsWhoOwn = await _context.UserGames
                .AsNoTracking()
                .Where(ug => ug.GameId == gameId && allFriendIds.Contains(ug.UserId))
                .Include(ug => ug.User)
                .Select(ug => new FriendDto
                {
                    Id = ug.User.Id,
                    Username = ug.User.Username,
                    Email = ug.User.Email,
                    AvatarUrl = ug.User.AvatarUrl,
                    Bio = ug.User.Bio,
                    Status = ug.User.Status,
                    LastLoginAt = ug.User.LastLoginAt
                })
                .ToListAsync();

            var owningUserIds = friendsWhoOwn.Select(f => f.Id).ToHashSet();
            var friendsWhoWishlist = await _context.UserWishlists
                .AsNoTracking()
                .Where(uw => uw.GameId == gameId && allFriendIds.Contains(uw.UserId) && !owningUserIds.Contains(uw.UserId))
                .Include(uw => uw.User)
                .Select(uw => new FriendDto
                {
                    Id = uw.User.Id,
                    Username = uw.User.Username,
                    Email = uw.User.Email,
                    AvatarUrl = uw.User.AvatarUrl,
                    Bio = uw.User.Bio,
                    Status = uw.User.Status,
                    LastLoginAt = uw.User.LastLoginAt
                })
                .ToListAsync();

            return Ok(new FriendsGameStatusDto
            {
                FriendsWhoOwn = friendsWhoOwn,
                FriendsWhoWishlist = friendsWhoWishlist
            });
        }

        [HttpGet("requests")]
        [ProducesResponseType(typeof(List<FriendRequestDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<List<FriendRequestDto>>> GetFriendRequests([FromQuery] string? type = "incoming")
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == Guid.Empty)
            {
                return Unauthorized(new { message = "Користувач не авторизований." });
            }

            var normType = (type ?? "incoming").Trim().ToLowerInvariant();

            var query = _context.FriendRequests
                .AsNoTracking()
                .Include(r => r.Sender)
                .Include(r => r.Receiver)
                .Where(r => r.Status == FriendRequestStatus.Pending);

            if (normType == "incoming")
            {
                query = query.Where(r => r.ReceiverId == currentUserId);
            }
            else if (normType == "outgoing")
            {
                query = query.Where(r => r.SenderId == currentUserId);
            }
            else
            {
                query = query.Where(r => r.ReceiverId == currentUserId || r.SenderId == currentUserId);
            }

            var requests = await query
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new FriendRequestDto
                {
                    Id = r.Id,
                    SenderId = r.SenderId,
                    SenderUsername = r.Sender != null ? r.Sender.Username : "Користувач",
                    SenderAvatarUrl = r.Sender != null ? r.Sender.AvatarUrl : null,
                    ReceiverId = r.ReceiverId,
                    ReceiverUsername = r.Receiver != null ? r.Receiver.Username : "Користувач",
                    ReceiverAvatarUrl = r.Receiver != null ? r.Receiver.AvatarUrl : null,
                    Status = r.Status,
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync();

            return Ok(requests);
        }

        [HttpPost("requests")]
        [ProducesResponseType(typeof(FriendActionResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<FriendActionResponseDto>> SendFriendRequest([FromBody] SendFriendRequestDto requestDto)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == Guid.Empty)
            {
                return Unauthorized(new { message = "Користувач не авторизований." });
            }

            var sender = await _context.Users.FirstOrDefaultAsync(u => u.Id == currentUserId);
            if (sender == null)
            {
                return NotFound(new { message = "Відправника не знайдено." });
            }

            Duser? receiver = null;

            if (requestDto.ReceiverId.HasValue && requestDto.ReceiverId.Value != Guid.Empty)
            {
                receiver = await _context.Users.FirstOrDefaultAsync(u => u.Id == requestDto.ReceiverId.Value);
            }
            else if (!string.IsNullOrWhiteSpace(requestDto.ReceiverUsername))
            {
                var search = requestDto.ReceiverUsername.Trim();
                receiver = await _context.Users.FirstOrDefaultAsync(u =>
                    u.Username.ToLower() == search.ToLower() ||
                    u.Email.ToLower() == search.ToLower());

                if (receiver == null && Guid.TryParse(search, out var parsedId))
                {
                    receiver = await _context.Users.FirstOrDefaultAsync(u => u.Id == parsedId);
                }
            }

            if (receiver == null)
            {
                return NotFound(new { message = "Користувача, якому надсилається запит, не знайдено." });
            }

            if (receiver.Id == currentUserId)
            {
                return BadRequest(new { message = "Ви не можете надіслати запит у друзі самому собі." });
            }

            var alreadyFriends = await _context.UserFriends
                .AnyAsync(f => (f.UserId == currentUserId && f.FriendId == receiver.Id) ||
                               (f.UserId == receiver.Id && f.FriendId == currentUserId));

            if (alreadyFriends)
            {
                return BadRequest(new { message = $"Ви вже є друзями з користувачем '{receiver.Username}'." });
            }

            var isBlocked = await _context.UserBlocks
                .AnyAsync(b => (b.UserId == currentUserId && b.BlockedUserId == receiver.Id) ||
                               (b.UserId == receiver.Id && b.BlockedUserId == currentUserId));

            if (isBlocked)
            {
                return BadRequest(new { message = "Неможливо надіслати запит: один із користувачів заблокований." });
            }

            var pendingRequest = await _context.FriendRequests
                .FirstOrDefaultAsync(r =>
                    ((r.SenderId == currentUserId && r.ReceiverId == receiver.Id) ||
                     (r.SenderId == receiver.Id && r.ReceiverId == currentUserId)) &&
                    r.Status == FriendRequestStatus.Pending);

            if (pendingRequest != null)
            {
                if (pendingRequest.SenderId == currentUserId)
                {
                    return BadRequest(new { message = "Ви вже надіслали запит цьому користувачеві. Запит очікує на відповідь." });
                }
                else
                {

                    pendingRequest.Status = FriendRequestStatus.Accepted;
                    pendingRequest.RespondedAt = DateTime.UtcNow;

                    if (!await _context.UserFriends.AnyAsync(f => f.UserId == currentUserId && f.FriendId == receiver.Id))
                    {
                        _context.UserFriends.Add(new UserFriend { UserId = currentUserId, FriendId = receiver.Id, CreatedAt = DateTime.UtcNow });
                    }
                    if (!await _context.UserFriends.AnyAsync(f => f.UserId == receiver.Id && f.FriendId == currentUserId))
                    {
                        _context.UserFriends.Add(new UserFriend { UserId = receiver.Id, FriendId = currentUserId, CreatedAt = DateTime.UtcNow });
                    }
                    await _context.SaveChangesAsync();

                    var mutualUserIds = new[] { currentUserId.ToString().ToLowerInvariant(), receiver.Id.ToString().ToLowerInvariant() };
                    try
                    {
                        await _friendsHub.Clients.Users(mutualUserIds)
                            .SendAsync("FriendRequestAccepted", new
                            {
                                requestId = pendingRequest.Id,
                                senderId = receiver.Id,
                                senderUsername = receiver.Username,
                                receiverId = currentUserId,
                                receiverUsername = sender.Username
                            });

                        await _friendsHub.Clients.Groups(mutualUserIds)
                            .SendAsync("FriendRequestAccepted", new
                            {
                                requestId = pendingRequest.Id,
                                senderId = receiver.Id,
                                senderUsername = receiver.Username,
                                receiverId = currentUserId,
                                receiverUsername = sender.Username
                            });
                    }
                    catch { }

                    await _notificationService.NotifyAsync(new CreateNotificationCommand
                    {
                        UserId = receiver.Id,
                        ActorUserId = currentUserId,
                        Type = NotificationTypes.FriendAccepted,
                        EntityType = "friend_request",
                        EntityId = pendingRequest.Id,
                        Title = "Запит у друзі прийнято",
                        Message = $"{sender.Username} прийняв(ла) ваш запит у друзі"
                    });

                    return Ok(new FriendActionResponseDto
                    {
                        Success = true,
                        Message = $"Запит взаємний! Користувача '{receiver.Username}' успішно додано в друзі."
                    });
                }
            }

            var newRequest = new FriendRequest
            {
                SenderId = currentUserId,
                ReceiverId = receiver.Id,
                Status = FriendRequestStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            _context.FriendRequests.Add(newRequest);
            await _context.SaveChangesAsync();

            await _notificationService.NotifyAsync(new CreateNotificationCommand
            {
                UserId = receiver.Id,
                ActorUserId = currentUserId,
                Type = NotificationTypes.FriendRequest,
                EntityType = "friend_request",
                EntityId = newRequest.Id,
                Title = "Запит у друзі",
                Message = $"{sender.Username} надіслав(ла) вам запит у друзі"
            });

            try
            {
                var receiverIdStr = receiver.Id.ToString().ToLowerInvariant();
                await _friendsHub.Clients.User(receiverIdStr)
                    .SendAsync("FriendRequestReceived", new
                    {
                        requestId = newRequest.Id,
                        senderId = currentUserId,
                        senderUsername = sender.Username,
                        senderAvatarUrl = sender.AvatarUrl,
                        createdAt = newRequest.CreatedAt
                    });

                await _friendsHub.Clients.Group(receiverIdStr)
                    .SendAsync("FriendRequestReceived", new
                    {
                        requestId = newRequest.Id,
                        senderId = currentUserId,
                        senderUsername = sender.Username,
                        senderAvatarUrl = sender.AvatarUrl,
                        createdAt = newRequest.CreatedAt
                    });
            }
            catch { }

            return Ok(new FriendActionResponseDto
            {
                Success = true,
                Message = $"Запит у друзі успішно надіслано користувачеві '{receiver.Username}'."
            });
        }

        [HttpPost("requests/{requestId:guid}/accept")]
        [ProducesResponseType(typeof(FriendActionResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<FriendActionResponseDto>> AcceptFriendRequest(Guid requestId)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == Guid.Empty)
            {
                return Unauthorized(new { message = "Користувач не авторизований." });
            }

            var request = await _context.FriendRequests
                .Include(r => r.Sender)
                .Include(r => r.Receiver)
                .FirstOrDefaultAsync(r => r.Id == requestId && r.ReceiverId == currentUserId);

            if (request == null)
            {
                return NotFound(new { message = "Запит у друзі не знайдено або у вас немає прав на його прийняття." });
            }

            if (request.Status != FriendRequestStatus.Pending)
            {
                return BadRequest(new { message = "Цей запит уже був оброблений раніше." });
            }

            request.Status = FriendRequestStatus.Accepted;
            request.RespondedAt = DateTime.UtcNow;

            if (!await _context.UserFriends.AnyAsync(f => f.UserId == request.ReceiverId && f.FriendId == request.SenderId))
            {
                _context.UserFriends.Add(new UserFriend
                {
                    UserId = request.ReceiverId,
                    FriendId = request.SenderId,
                    CreatedAt = DateTime.UtcNow
                });
            }

            if (!await _context.UserFriends.AnyAsync(f => f.UserId == request.SenderId && f.FriendId == request.ReceiverId))
            {
                _context.UserFriends.Add(new UserFriend
                {
                    UserId = request.SenderId,
                    FriendId = request.ReceiverId,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();

            await _notificationService.NotifyAsync(new CreateNotificationCommand
            {
                UserId = request.SenderId,
                ActorUserId = currentUserId,
                Type = NotificationTypes.FriendAccepted,
                EntityType = "friend_request",
                EntityId = request.Id,
                Title = "Запит у друзі прийнято",
                Message = $"{request.Receiver.Username} прийняв(ла) ваш запит у друзі"
            });

            try
            {
                var notifyUserIds = new[] { request.SenderId.ToString().ToLowerInvariant(), request.ReceiverId.ToString().ToLowerInvariant() };
                await _friendsHub.Clients.Users(notifyUserIds)
                    .SendAsync("FriendRequestAccepted", new
                    {
                        requestId = request.Id,
                        senderId = request.SenderId,
                        senderUsername = request.Sender.Username,
                        receiverId = request.ReceiverId,
                        receiverUsername = request.Receiver.Username
                    });

                await _friendsHub.Clients.Groups(notifyUserIds)
                    .SendAsync("FriendRequestAccepted", new
                    {
                        requestId = request.Id,
                        senderId = request.SenderId,
                        senderUsername = request.Sender.Username,
                        receiverId = request.ReceiverId,
                        receiverUsername = request.Receiver.Username
                    });
            }
            catch { }

            try
            {
                await _activityService.LogActivityAsync(
                    userId: request.ReceiverId,
                    type: UserActivityType.FriendAdded,
                    title: $"Подружився(лася) з {request.Sender.Username}",
                    description: null,
                    details: JsonSerializer.Serialize(new { friendId = request.SenderId, friendUsername = request.Sender.Username }),
                    relatedEntityId: request.SenderId,
                    imageUrl: request.Sender.AvatarUrl
                );

                await _activityService.LogActivityAsync(
                    userId: request.SenderId,
                    type: UserActivityType.FriendAdded,
                    title: $"Подружився(лася) з {request.Receiver.Username}",
                    description: null,
                    details: JsonSerializer.Serialize(new { friendId = request.ReceiverId, friendUsername = request.Receiver.Username }),
                    relatedEntityId: request.ReceiverId,
                    imageUrl: request.Receiver.AvatarUrl
                );
            }
            catch { }

            return Ok(new FriendActionResponseDto
            {
                Success = true,
                Message = $"Запит у друзі від '{request.Sender.Username}' успішно прийнято!"
            });
        }

        [HttpPost("requests/{requestId:guid}/reject")]
        [ProducesResponseType(typeof(FriendActionResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<FriendActionResponseDto>> RejectFriendRequest(Guid requestId)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == Guid.Empty)
            {
                return Unauthorized(new { message = "Користувач не авторизований." });
            }

            var request = await _context.FriendRequests
                .Include(r => r.Sender)
                .FirstOrDefaultAsync(r => r.Id == requestId && r.ReceiverId == currentUserId);

            if (request == null)
            {
                return NotFound(new { message = "Запит у друзі не знайдено або у вас немає прав на його відхилення." });
            }

            if (request.Status != FriendRequestStatus.Pending)
            {
                return BadRequest(new { message = "Цей запит уже був оброблений раніше." });
            }

            request.Status = FriendRequestStatus.Rejected;
            request.RespondedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var senderIdStr = request.SenderId.ToString().ToLowerInvariant();
            try
            {
                await _friendsHub.Clients.User(senderIdStr).SendAsync("FriendRequestRejected", request.Id);
                await _friendsHub.Clients.Group(senderIdStr).SendAsync("FriendRequestRejected", request.Id);
            }
            catch { }

            return Ok(new FriendActionResponseDto
            {
                Success = true,
                Message = $"Запит у друзі від '{request.Sender.Username}' відхилено."
            });
        }

        [HttpPost("requests/{requestId:guid}/cancel")]
        [ProducesResponseType(typeof(FriendActionResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<FriendActionResponseDto>> CancelFriendRequest(Guid requestId)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == Guid.Empty)
            {
                return Unauthorized(new { message = "Користувач не авторизований." });
            }

            var request = await _context.FriendRequests
                .FirstOrDefaultAsync(r => r.Id == requestId && r.SenderId == currentUserId);

            if (request == null)
            {
                return NotFound(new { message = "Запит у друзі не знайдено або ви не є його відправником." });
            }

            if (request.Status != FriendRequestStatus.Pending)
            {
                return BadRequest(new { message = "Цей запит уже був оброблений і не може бути скасований." });
            }

            request.Status = FriendRequestStatus.Cancelled;
            request.RespondedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var receiverIdStr = request.ReceiverId.ToString().ToLowerInvariant();
            try
            {
                await _friendsHub.Clients.User(receiverIdStr).SendAsync("FriendRequestCancelled", request.Id);
                await _friendsHub.Clients.Group(receiverIdStr).SendAsync("FriendRequestCancelled", request.Id);
            }
            catch { }

            return Ok(new FriendActionResponseDto
            {
                Success = true,
                Message = "Запит у друзі успішно скасовано."
            });
        }

        [HttpDelete("{friendId:guid}")]
        [ProducesResponseType(typeof(FriendActionResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<FriendActionResponseDto>> RemoveFriend(Guid friendId)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == Guid.Empty)
            {
                return Unauthorized(new { message = "Користувач не авторизований." });
            }

            var friendships = await _context.UserFriends
                .Where(f => (f.UserId == currentUserId && f.FriendId == friendId) ||
                            (f.UserId == friendId && f.FriendId == currentUserId))
                .ToListAsync();

            if (!friendships.Any())
            {
                return NotFound(new { message = "Дружбу між користувачами не знайдено." });
            }

            _context.UserFriends.RemoveRange(friendships);

            var oldRequests = await _context.FriendRequests
                .Where(r => (r.SenderId == currentUserId && r.ReceiverId == friendId) ||
                            (r.SenderId == friendId && r.ReceiverId == currentUserId))
                .ToListAsync();

            if (oldRequests.Any())
            {
                _context.FriendRequests.RemoveRange(oldRequests);
            }

            await _context.SaveChangesAsync();

            var notifyUserIds = new[] { currentUserId.ToString().ToLowerInvariant(), friendId.ToString().ToLowerInvariant() };
            try
            {
                await _friendsHub.Clients.Users(notifyUserIds).SendAsync("FriendRemoved", currentUserId.ToString().ToLowerInvariant());
                await _friendsHub.Clients.Groups(notifyUserIds).SendAsync("FriendRemoved", currentUserId.ToString().ToLowerInvariant());
            }
            catch { }

            return Ok(new FriendActionResponseDto
            {
                Success = true,
                Message = "Користувача успішно видалено зі списку друзів."
            });
        }

        [HttpPost("{targetUserId:guid}/block")]
        [ProducesResponseType(typeof(FriendActionResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<FriendActionResponseDto>> BlockUser(Guid targetUserId)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == Guid.Empty)
            {
                return Unauthorized(new { message = "Користувач не авторизований." });
            }

            if (currentUserId == targetUserId)
            {
                return BadRequest(new { message = "Не можна заблокувати самого себе." });
            }

            var targetUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == targetUserId);
            if (targetUser == null)
            {
                return NotFound(new { message = "Користувача не знайдено." });
            }

            var friendships = await _context.UserFriends
                .Where(f => (f.UserId == currentUserId && f.FriendId == targetUserId) ||
                            (f.UserId == targetUserId && f.FriendId == currentUserId))
                .ToListAsync();

            if (friendships.Any())
            {
                _context.UserFriends.RemoveRange(friendships);
            }

            var oldRequests = await _context.FriendRequests
                .Where(r => (r.SenderId == currentUserId && r.ReceiverId == targetUserId) ||
                            (r.SenderId == targetUserId && r.ReceiverId == currentUserId))
                .ToListAsync();

            if (oldRequests.Any())
            {
                _context.FriendRequests.RemoveRange(oldRequests);
            }

            var isBlocked = await _context.UserBlocks
                .AnyAsync(b => b.UserId == currentUserId && b.BlockedUserId == targetUserId);

            if (!isBlocked)
            {
                _context.UserBlocks.Add(new UserBlock
                {
                    UserId = currentUserId,
                    BlockedUserId = targetUserId,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();

            var notifyUserIds = new[] { currentUserId.ToString().ToLowerInvariant(), targetUserId.ToString().ToLowerInvariant() };
            try
            {
                await _friendsHub.Clients.Users(notifyUserIds).SendAsync("FriendRemoved", currentUserId.ToString().ToLowerInvariant());
                await _friendsHub.Clients.Groups(notifyUserIds).SendAsync("FriendRemoved", currentUserId.ToString().ToLowerInvariant());
            }
            catch { }

            return Ok(new FriendActionResponseDto
            {
                Success = true,
                Message = $"Користувача '{targetUser.Username}' успішно заблоковано."
            });
        }

        [HttpPost("{targetUserId:guid}/unblock")]
        [ProducesResponseType(typeof(FriendActionResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<FriendActionResponseDto>> UnblockUser(Guid targetUserId)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == Guid.Empty)
            {
                return Unauthorized(new { message = "Користувач не авторизований." });
            }

            var targetUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == targetUserId);
            if (targetUser == null)
            {
                return NotFound(new { message = "Користувача не знайдено." });
            }

            var block = await _context.UserBlocks
                .FirstOrDefaultAsync(b => b.UserId == currentUserId && b.BlockedUserId == targetUserId);

            if (block != null)
            {
                _context.UserBlocks.Remove(block);
                await _context.SaveChangesAsync();
            }

            return Ok(new FriendActionResponseDto
            {
                Success = true,
                Message = $"Користувача '{targetUser.Username}' успішно розблоковано."
            });
        }

        [HttpGet("blocked")]
        [ProducesResponseType(typeof(List<FriendDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<List<FriendDto>>> GetBlockedList()
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == Guid.Empty)
            {
                return Unauthorized(new { message = "Користувач не авторизований." });
            }

            var blocked = await _context.UserBlocks
                .AsNoTracking()
                .Where(ub => ub.UserId == currentUserId)
                .Include(ub => ub.BlockedUser)
                .Where(ub => ub.BlockedUser != null && !ub.BlockedUser.IsDeleted)
                .Select(ub => new FriendDto
                {
                    Id = ub.BlockedUser.Id,
                    Username = ub.BlockedUser.Username,
                    Email = ub.BlockedUser.Email,
                    AvatarUrl = ub.BlockedUser.AvatarUrl,
                    Bio = ub.BlockedUser.Bio,
                    Status = ub.BlockedUser.Status,
                    LastLoginAt = ub.BlockedUser.LastLoginAt,
                    FriendsSince = ub.CreatedAt
                })
                .ToListAsync();

            return Ok(blocked);
        }
    }
}
