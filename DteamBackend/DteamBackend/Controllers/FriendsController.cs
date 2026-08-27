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
        private readonly ILogger<FriendsController> _logger;

        public FriendsController(AppDbContext context, ILogger<FriendsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                           ?? User.FindFirst("sub")?.Value;

            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }

        private static FriendRequestDto MapToFriendRequestDto(FriendRequest fr) => new()
        {
            Id = fr.Id,
            SenderId = fr.SenderId,
            SenderUsername = fr.Sender?.Username ?? string.Empty,
            SenderAvatarUrl = fr.Sender?.AvatarUrl,
            ReceiverId = fr.ReceiverId,
            ReceiverUsername = fr.Receiver?.Username ?? string.Empty,
            ReceiverAvatarUrl = fr.Receiver?.AvatarUrl,
            Status = fr.Status,
            CreatedAt = fr.CreatedAt
        };

        [HttpPost("requests")]
        [HttpPost("send-request")]
        public async Task<ActionResult<FriendRequestDto>> SendFriendRequest([FromBody] SendFriendRequestDto dto)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == Guid.Empty)
            {
                return Unauthorized(new { message = "Користувач не авторизований." });
            }

            var sender = await _context.Users.FirstOrDefaultAsync(u => u.Id == currentUserId);
            if (sender == null)
            {
                return NotFound(new { message = "Користувача не знайдено." });
            }

            if (sender.IsBanned)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "Ваш акаунт заблоковано." });
            }

            Duser? receiver = null;
            if (dto.ReceiverId.HasValue && dto.ReceiverId.Value != Guid.Empty)
            {
                receiver = await _context.Users.FirstOrDefaultAsync(u => u.Id == dto.ReceiverId.Value);
            }
            else if (!string.IsNullOrWhiteSpace(dto.ReceiverUsername))
            {
                var normUsername = dto.ReceiverUsername.Trim().ToLower();
                receiver = await _context.Users.FirstOrDefaultAsync(u => u.Username.ToLower() == normUsername);
            }

            if (receiver == null)
            {
                return NotFound(new { message = "Користувача-одержувача не знайдено." });
            }

            if (receiver.Id == currentUserId)
            {
                return BadRequest(new { message = "Ви не можете надіслати заявку в друзі самому собі." });
            }

            if (receiver.IsBanned)
            {
                return BadRequest(new { message = "Цей користувач заблокований в системі." });
            }

            var alreadyFriends = await _context.UserFriends.AnyAsync(f => 
                (f.UserId == currentUserId && f.FriendId == receiver.Id && f.Status == FriendshipStatus.Accepted) ||
                (f.UserId == receiver.Id && f.FriendId == currentUserId && f.Status == FriendshipStatus.Accepted));

            if (alreadyFriends)
            {
                return BadRequest(new { message = $"Ви вже є друзями з користувачем '{receiver.Username}'." });
            }

            var existingOutgoingRequest = await _context.FriendRequests.FirstOrDefaultAsync(fr =>
                fr.SenderId == currentUserId && fr.ReceiverId == receiver.Id && fr.Status == FriendRequestStatus.Pending);

            if (existingOutgoingRequest != null)
            {
                return BadRequest(new { message = "Ви вже надіслали заявку в друзі цьому користувачу. Заявка очікує на розгляд." });
            }

            var existingIncomingRequest = await _context.FriendRequests.FirstOrDefaultAsync(fr =>
                fr.SenderId == receiver.Id && fr.ReceiverId == currentUserId && fr.Status == FriendRequestStatus.Pending);

            if (existingIncomingRequest != null)
            {
                return BadRequest(new { 
                    message = $"Користувач '{receiver.Username}' вже надіслав вам заявку в друзі. Ви можете просто прийняти її.",
                    existingRequestId = existingIncomingRequest.Id
                });
            }

            var friendRequest = new FriendRequest
            {
                Id = Guid.NewGuid(),
                SenderId = currentUserId,
                Sender = sender,
                ReceiverId = receiver.Id,
                Receiver = receiver,
                Status = FriendRequestStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            await _context.FriendRequests.AddAsync(friendRequest);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"[FriendsController] User {sender.Username} ({currentUserId}) sent friend request to {receiver.Username} ({receiver.Id})");

            return Ok(MapToFriendRequestDto(friendRequest));
        }

        [HttpGet("requests")]
        public async Task<ActionResult<List<FriendRequestDto>>> GetFriendRequests([FromQuery] string? type = "incoming")
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == Guid.Empty)
            {
                return Unauthorized(new { message = "Користувач не авторизований." });
            }

            IQueryable<FriendRequest> query = _context.FriendRequests
                .Include(fr => fr.Sender)
                .Include(fr => fr.Receiver)
                .AsNoTracking();

            var normType = type?.Trim().ToLower() ?? "incoming";

            if (normType == "outgoing")
            {
                query = query.Where(fr => fr.SenderId == currentUserId && fr.Status == FriendRequestStatus.Pending);
            }
            else if (normType == "all")
            {
                query = query.Where(fr => (fr.ReceiverId == currentUserId || fr.SenderId == currentUserId) && fr.Status == FriendRequestStatus.Pending);
            }
            else
            {
                query = query.Where(fr => fr.ReceiverId == currentUserId && fr.Status == FriendRequestStatus.Pending);
            }

            var requests = await query
                .OrderByDescending(fr => fr.CreatedAt)
                .Select(fr => MapToFriendRequestDto(fr))
                .ToListAsync();

            return Ok(requests);
        }

        [HttpPost("requests/{requestId:guid}/accept")]
        [HttpPost("accept/{requestId:guid}")]
        public async Task<ActionResult<FriendActionResponseDto>> AcceptFriendRequest(Guid requestId)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == Guid.Empty)
            {
                return Unauthorized(new { message = "Користувач не авторизований." });
            }

            var request = await _context.FriendRequests
                .Include(fr => fr.Sender)
                .Include(fr => fr.Receiver)
                .FirstOrDefaultAsync(fr => fr.Id == requestId);

            if (request == null)
            {
                return NotFound(new { message = "Заявку в друзі не знайдено." });
            }

            if (request.ReceiverId != currentUserId)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "Тільки одержувач заявки може прийняти її." });
            }

            if (request.Status != FriendRequestStatus.Pending)
            {
                return BadRequest(new { message = $"Ця заявка вже має статус: {request.Status}." });
            }

            request.Status = FriendRequestStatus.Accepted;
            request.RespondedAt = DateTime.UtcNow;

            var existingSenderFriend = await _context.UserFriends
                .FirstOrDefaultAsync(f => f.UserId == request.SenderId && f.FriendId == request.ReceiverId);

            if (existingSenderFriend == null)
            {
                await _context.UserFriends.AddAsync(new UserFriend
                {
                    UserId = request.SenderId,
                    FriendId = request.ReceiverId,
                    Status = FriendshipStatus.Accepted,
                    CreatedAt = DateTime.UtcNow
                });
            }
            else
            {
                existingSenderFriend.Status = FriendshipStatus.Accepted;
            }

            var existingReceiverFriend = await _context.UserFriends
                .FirstOrDefaultAsync(f => f.UserId == request.ReceiverId && f.FriendId == request.SenderId);

            if (existingReceiverFriend == null)
            {
                await _context.UserFriends.AddAsync(new UserFriend
                {
                    UserId = request.ReceiverId,
                    FriendId = request.SenderId,
                    Status = FriendshipStatus.Accepted,
                    CreatedAt = DateTime.UtcNow
                });
            }
            else
            {
                existingReceiverFriend.Status = FriendshipStatus.Accepted;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation($"[FriendsController] User {request.Receiver.Username} accepted friend request from {request.Sender.Username}");

            return Ok(new FriendActionResponseDto
            {
                Success = true,
                Message = $"Заявку в друзі від '{request.Sender?.Username}' успішно прийнято! Тепер ви друзі."
            });
        }

        [HttpPost("requests/{requestId:guid}/reject")]
        public async Task<ActionResult<FriendActionResponseDto>> RejectFriendRequest(Guid requestId)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == Guid.Empty)
            {
                return Unauthorized(new { message = "Користувач не авторизований." });
            }

            var request = await _context.FriendRequests
                .Include(fr => fr.Sender)
                .FirstOrDefaultAsync(fr => fr.Id == requestId);

            if (request == null)
            {
                return NotFound(new { message = "Заявку в друзі не знайдено." });
            }

            if (request.ReceiverId != currentUserId)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "Тільки одержувач заявки може відхилити її." });
            }

            if (request.Status != FriendRequestStatus.Pending)
            {
                return BadRequest(new { message = $"Ця заявка вже має статус: {request.Status}." });
            }

            request.Status = FriendRequestStatus.Rejected;
            request.RespondedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new FriendActionResponseDto
            {
                Success = true,
                Message = $"Заявку в друзі від '{request.Sender?.Username}' відхилено."
            });
        }

        [HttpPost("requests/{requestId:guid}/cancel")]
        public async Task<ActionResult<FriendActionResponseDto>> CancelFriendRequest(Guid requestId)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == Guid.Empty)
            {
                return Unauthorized(new { message = "Користувач не авторизований." });
            }

            var request = await _context.FriendRequests.FirstOrDefaultAsync(fr => fr.Id == requestId);

            if (request == null)
            {
                return NotFound(new { message = "Заявку в друзі не знайдено." });
            }

            if (request.SenderId != currentUserId)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "Ви можете скасувати тільки власну заявку." });
            }

            if (request.Status != FriendRequestStatus.Pending)
            {
                return BadRequest(new { message = $"Заявка вже має статус: {request.Status}." });
            }

            request.Status = FriendRequestStatus.Cancelled;
            request.RespondedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new FriendActionResponseDto
            {
                Success = true,
                Message = "Заявку в друзі успішно скасовано."
            });
        }

        [HttpGet]
        [HttpGet("list")]
        public async Task<ActionResult<List<FriendDto>>> GetFriendsList()
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == Guid.Empty)
            {
                return Unauthorized(new { message = "Користувач не авторизований." });
            }

            var friendships = await _context.UserFriends
                .Include(f => f.Friend)
                .AsNoTracking()
                .Where(f => f.UserId == currentUserId && f.Status == FriendshipStatus.Accepted)
                .OrderBy(f => f.Friend.Username)
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

            return Ok(friendships);
        }

        [HttpDelete("{friendId:guid}")]
        public async Task<ActionResult<FriendActionResponseDto>> RemoveFriend(Guid friendId)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == Guid.Empty)
            {
                return Unauthorized(new { message = "Користувач не авторизований." });
            }

            var mutualFriendships = await _context.UserFriends
                .Where(f => (f.UserId == currentUserId && f.FriendId == friendId) ||
                            (f.UserId == friendId && f.FriendId == currentUserId))
                .ToListAsync();

            if (mutualFriendships.Count == 0)
            {
                return NotFound(new { message = "Користувача не знайдено у вашому списку друзів." });
            }

            _context.UserFriends.RemoveRange(mutualFriendships);
            await _context.SaveChangesAsync();

            return Ok(new FriendActionResponseDto
            {
                Success = true,
                Message = "Користувача успішно видалено зі списку друзів."
            });
        }

        [HttpPost("{targetUserId:guid}/block")]
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
        public async Task<ActionResult<List<FriendDto>>> GetBlockedList()
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == Guid.Empty)
            {
                return Unauthorized(new { message = "Користувач не авторизований." });
            }

            var blocked = await _context.UserFriends
                .Include(f => f.Friend)
                .AsNoTracking()
                .Where(f => f.UserId == currentUserId && f.Status == FriendshipStatus.Blocked)
                .OrderBy(f => f.Friend.Username)
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
