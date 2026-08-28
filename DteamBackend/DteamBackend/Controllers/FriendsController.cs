using System.Security.Claims;
using DteamBackend.Data;
using DteamBackend.Models;
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
        public async Task<IActionResult> GetFriends()
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized(new { message = "Користувач не авторизований." });
            }

            var friendships = await _context.UserFriends
                .Include(uf => uf.User)
                .Include(uf => uf.Friend)
                .AsNoTracking()
                .Where(uf => uf.UserId == userId || uf.FriendId == userId)
                .ToListAsync();

            var result = friendships.Select(uf => {
                var isSender = uf.UserId == userId;
                var otherUser = isSender ? uf.Friend : uf.User;
                return new
                {
                    userId = userId,
                    friendId = otherUser.Id,
                    status = (int)uf.Status,
                    isIncoming = !isSender && uf.Status == FriendshipStatus.Pending,
                    createdAt = uf.CreatedAt,
                    friend = new
                    {
                        id = otherUser.Id,
                        username = otherUser.Username,
                        email = otherUser.Email,
                        avatarUrl = otherUser.AvatarUrl,
                        status = (int)otherUser.Status,
                        bio = otherUser.Bio,
                        isInFamily = otherUser.IsInFamily
                    }
                };
            });

            return Ok(result);
        }

        public class SendFriendRequestDto
        {
            public string UsernameOrEmail { get; set; } = string.Empty;
        }

        [HttpPost("request")]
        public async Task<IActionResult> SendFriendRequest([FromBody] SendFriendRequestDto dto)
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized(new { message = "Користувач не авторизований." });
            }

            if (string.IsNullOrWhiteSpace(dto.UsernameOrEmail))
            {
                return BadRequest(new { message = "Укажіть ім'я користувача або email." });
            }

            var queryStr = dto.UsernameOrEmail.Trim().ToLower();

            var targetUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Username.ToLower() == queryStr || u.Email.ToLower() == queryStr);

            if (targetUser == null)
            {
                return NotFound(new { message = $"Користувача '{dto.UsernameOrEmail}' не знайдено." });
            }

            if (targetUser.Id == userId)
            {
                return BadRequest(new { message = "Ви не можете додати самого себе в друзі." });
            }

            var existingRel = await _context.UserFriends
                .FirstOrDefaultAsync(uf => 
                    (uf.UserId == userId && uf.FriendId == targetUser.Id) ||
                    (uf.UserId == targetUser.Id && uf.FriendId == userId));

            if (existingRel != null)
            {
                if (existingRel.Status == FriendshipStatus.Accepted)
                {
                    return BadRequest(new { message = $"Ви вже є друзями з користувачем '{targetUser.Username}'." });
                }
                return BadRequest(new { message = $"Запит у друзі з '{targetUser.Username}' вже існує." });
            }

            var newFriendship = new UserFriend
            {
                UserId = userId,
                FriendId = targetUser.Id,
                Status = FriendshipStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            await _context.UserFriends.AddAsync(newFriendship);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = $"Запит у друзі надіслано користувачеві '{targetUser.Username}'. Очікується підтвердження.",
                friendId = targetUser.Id,
                status = (int)FriendshipStatus.Pending,
                friend = new
                {
                    id = targetUser.Id,
                    username = targetUser.Username,
                    email = targetUser.Email,
                    avatarUrl = targetUser.AvatarUrl,
                    status = (int)targetUser.Status,
                    bio = targetUser.Bio
                }
            });
        }

        [HttpPost("accept/{friendId:guid}")]
        public async Task<IActionResult> AcceptFriendRequest(Guid friendId)
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized(new { message = "Користувач не авторизований." });
            }

            var friendship = await _context.UserFriends
                .FirstOrDefaultAsync(uf => 
                    (uf.UserId == friendId && uf.FriendId == userId) ||
                    (uf.UserId == userId && uf.FriendId == friendId));

            if (friendship == null)
            {
                return NotFound(new { message = "Запит у друзі не знайдено." });
            }

            friendship.Status = FriendshipStatus.Accepted;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Запит у друзі прийнято!", friendId, status = (int)FriendshipStatus.Accepted });
        }

        [HttpPost("decline/{friendId:guid}")]
        [HttpDelete("{friendId:guid}")]
        public async Task<IActionResult> DeclineOrRemoveFriend(Guid friendId)
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized(new { message = "Користувач не авторизований." });
            }

            var friendship = await _context.UserFriends
                .FirstOrDefaultAsync(uf => 
                    (uf.UserId == friendId && uf.FriendId == userId) ||
                    (uf.UserId == userId && uf.FriendId == friendId));

            if (friendship != null)
            {
                _context.UserFriends.Remove(friendship);
                await _context.SaveChangesAsync();
            }

            return Ok(new { message = "Запит або дружбу видалено.", friendId });
        }
    }
}
