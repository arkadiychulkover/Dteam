using System.Security.Claims;
using DteamBackend.Data;
using DteamBackend.Models;
using DteamBackend.Models.DTO;
using DteamBackend.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DteamBackend.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class FriendsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FriendsController(AppDbContext context)
        {
            _context = context;
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

            var directFriends = await _context.UserFriends
                .AsNoTracking()
                .Include(f => f.Friend)
                .Where(f => f.UserId == currentUserId && f.Status == FriendshipStatus.Accepted && f.Friend != null)
                .Select(f => new FriendDto
                {
                    Id = f.Friend.Id,
                    Username = f.Friend.Username,
                    Email = f.Friend.Email,
                    AvatarUrl = f.Friend.AvatarUrl,
                    Bio = f.Friend.Bio,
                    Status = f.Friend.Status,
                    LastLoginAt = f.Friend.LastLoginAt,
                    FriendsSince = f.CreatedAt
                })
                .ToListAsync();

            var reverseFriends = await _context.UserFriends
                .AsNoTracking()
                .Include(f => f.User)
                .Where(f => f.FriendId == currentUserId && f.Status == FriendshipStatus.Accepted && f.User != null)
                .Select(f => new FriendDto
                {
                    Id = f.User.Id,
                    Username = f.User.Username,
                    Email = f.User.Email,
                    AvatarUrl = f.User.AvatarUrl,
                    Bio = f.User.Bio,
                    Status = f.User.Status,
                    LastLoginAt = f.User.LastLoginAt,
                    FriendsSince = f.CreatedAt
                })
                .ToListAsync();

            var friends = directFriends
                .Concat(reverseFriends)
                .GroupBy(f => f.Id)
                .Select(g => g.First())
                .ToList();

            return Ok(friends);
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

            var query = _context.FriendRequests
                .Include(r => r.Sender)
                .Include(r => r.Receiver)
                .Where(r => r.Status == FriendRequestStatus.Pending);

            if (type == "incoming")
            {
                query = query.Where(r => r.ReceiverId == currentUserId);
            }
            else if (type == "outgoing")
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
                    SenderUsername = r.Sender.Username,
                    SenderAvatarUrl = r.Sender.AvatarUrl,
                    ReceiverId = r.ReceiverId,
                    ReceiverUsername = r.Receiver.Username,
                    ReceiverAvatarUrl = r.Receiver.AvatarUrl,
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
                .AnyAsync(f => (
                    (f.UserId == currentUserId && f.FriendId == receiver.Id) ||
                    (f.UserId == receiver.Id && f.FriendId == currentUserId)
                ) && f.Status == FriendshipStatus.Accepted);

            if (alreadyFriends)
            {
                return BadRequest(new { message = $"Ви вже є друзями з користувачем '{receiver.Username}'." });
            }

            var isBlocked = await _context.UserFriends
                .AnyAsync(f => (f.UserId == currentUserId && f.FriendId == receiver.Id && f.Status == FriendshipStatus.Blocked) ||
                               (f.UserId == receiver.Id && f.FriendId == currentUserId && f.Status == FriendshipStatus.Blocked));

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
                    return BadRequest(new { message = $"Користувач '{receiver.Username}' вже надіслав вам запит. Ви можете його прийняти." });
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

            var existingDirect = await _context.UserFriends
                .FirstOrDefaultAsync(f => f.UserId == request.ReceiverId && f.FriendId == request.SenderId);

            if (existingDirect == null)
            {
                _context.UserFriends.Add(new UserFriend
                {
                    UserId = request.ReceiverId,
                    FriendId = request.SenderId,
                    Status = FriendshipStatus.Accepted,
                    CreatedAt = DateTime.UtcNow
                });
            }
            else
            {
                existingDirect.Status = FriendshipStatus.Accepted;
            }

            var existingReverse = await _context.UserFriends
                .FirstOrDefaultAsync(f => f.UserId == request.SenderId && f.FriendId == request.ReceiverId);

            if (existingReverse == null)
            {
                _context.UserFriends.Add(new UserFriend
                {
                    UserId = request.SenderId,
                    FriendId = request.ReceiverId,
                    Status = FriendshipStatus.Accepted,
                    CreatedAt = DateTime.UtcNow
                });
            }
            else
            {
                existingReverse.Status = FriendshipStatus.Accepted;
            }

            await _context.SaveChangesAsync();

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

            var direct = await _context.UserFriends
                .FirstOrDefaultAsync(f => f.UserId == currentUserId && f.FriendId == friendId);

            var reverse = await _context.UserFriends
                .FirstOrDefaultAsync(f => f.UserId == friendId && f.FriendId == currentUserId);

            if (direct == null && reverse == null)
            {
                return NotFound(new { message = "Дружбу між користувачами не знайдено." });
            }

            if (direct != null)
            {
                _context.UserFriends.Remove(direct);
            }

            if (reverse != null)
            {
                _context.UserFriends.Remove(reverse);
            }

            await _context.SaveChangesAsync();

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

            var reverseFriendship = await _context.UserFriends
                .FirstOrDefaultAsync(f => f.UserId == targetUserId && f.FriendId == currentUserId);
            if (reverseFriendship != null)
            {
                _context.UserFriends.Remove(reverseFriendship);
            }

            var existingRecord = await _context.UserFriends
                .FirstOrDefaultAsync(f => f.UserId == currentUserId && f.FriendId == targetUserId);

            if (existingRecord != null)
            {
                existingRecord.Status = FriendshipStatus.Blocked;
            }
            else
            {
                _context.UserFriends.Add(new UserFriend
                {
                    UserId = currentUserId,
                    FriendId = targetUserId,
                    Status = FriendshipStatus.Blocked,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();

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

            var myRecord = await _context.UserFriends
                .FirstOrDefaultAsync(f => f.UserId == currentUserId && f.FriendId == targetUserId);

            if (myRecord != null)
            {
                myRecord.Status = FriendshipStatus.Accepted;
            }
            else
            {
                _context.UserFriends.Add(new UserFriend
                {
                    UserId = currentUserId,
                    FriendId = targetUserId,
                    Status = FriendshipStatus.Accepted,
                    CreatedAt = DateTime.UtcNow
                });
            }

            var reverseRecord = await _context.UserFriends
                .FirstOrDefaultAsync(f => f.UserId == targetUserId && f.FriendId == currentUserId);

            if (reverseRecord != null)
            {
                reverseRecord.Status = FriendshipStatus.Accepted;
            }
            else
            {
                _context.UserFriends.Add(new UserFriend
                {
                    UserId = targetUserId,
                    FriendId = currentUserId,
                    Status = FriendshipStatus.Accepted,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();

            return Ok(new FriendActionResponseDto
            {
                Success = true,
                Message = $"Користувача '{targetUser.Username}' успішно розблоковано та додано назад у друзі."
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

            var blocked = await _context.UserFriends
                .Include(f => f.Friend)
                .Where(f => f.UserId == currentUserId && f.Status == FriendshipStatus.Blocked)
                .Select(f => new FriendDto
                {
                    Id = f.Friend.Id,
                    Username = f.Friend.Username,
                    Email = f.Friend.Email,
                    AvatarUrl = f.Friend.AvatarUrl,
                    Bio = f.Friend.Bio,
                    Status = f.Friend.Status,
                    LastLoginAt = f.Friend.LastLoginAt,
                    FriendsSince = f.CreatedAt
                })
                .ToListAsync();

            return Ok(blocked);
        }
    }
}

