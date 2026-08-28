using System.Security.Claims;
using DteamBackend.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DteamBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UserController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("is-banned")]
        public async Task<IActionResult> CheckIsBanned([FromQuery] Guid? userId)
        {
            var targetId = userId;
            if ((targetId == null || targetId == Guid.Empty) && User.Identity?.IsAuthenticated == true)
            {
                var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                           ?? User.FindFirst("sub")?.Value;
                if (Guid.TryParse(idClaim, out var parsedId))
                {
                    targetId = parsedId;
                }
            }

            if (targetId == null || targetId == Guid.Empty)
            {
                return BadRequest(new { message = "Параметр userId обязателен или выполните вход" });
            }

            var user = await _context.Users
                .AsNoTracking()
                .Where(u => u.Id == targetId.Value)
                .Select(u => new
                {
                    UserId = u.Id,
                    u.Username,
                    u.Email,
                    u.IsBanned,
                    u.IsAdmin
                })
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound(new { message = $"Пользователь с ID '{userId}' не найден" });
            }

            return Ok(new
            {
                userId = user.UserId,
                username = user.Username,
                email = user.Email,
                isBanned = user.IsBanned,
                isAdmin = user.IsAdmin
            });
        }

        [HttpGet("library")]
        public async Task<IActionResult> GetUserLibrary()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                           ?? User.FindFirst("sub")?.Value;

            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { message = "Користувач не авторизований." });
            }

            var userGames = await _context.UserGames
                .Include(ug => ug.Game)
                    .ThenInclude(g => g.Owner)
                .AsNoTracking()
                .Where(ug => ug.UserId == userId)
                .OrderByDescending(ug => ug.PurchasedAt)
                .Select(ug => new
                {
                    userId = ug.UserId.ToString(),
                    gameId = ug.GameId.ToString(),
                    title = ug.Game.Title,
                    coverImageUrl = ug.Game.CoverImageUrl,
                    headerImageUrl = ug.Game.HeaderImageUrl,
                    purchasedAt = ug.PurchasedAt,
                    playtimeMinutes = ug.PlayTimeMinutes,
                    isFavorite = ug.IsFavorite,
                    isInstalled = false,
                    diskSize = ug.Game.SizeInBytes > 0 ? $"{Math.Round((double)ug.Game.SizeInBytes / (1024 * 1024 * 1024), 1)} ГБ" : "— ГБ",
                    playtimeLabel = ug.PlayTimeMinutes > 0 ? $"{Math.Round((double)ug.PlayTimeMinutes / 60, 1)} год. у грі" : "0 год. у грі",
                    lastPlayedAt = ug.LastPlayedAt,
                    lastPlayedLabel = ug.LastPlayedAt.HasValue ? ug.LastPlayedAt.Value.ToString("dd.MM.yyyy") : "Ніколи",
                    achievementsUnlocked = 0,
                    achievementsTotal = 0,
                    cloudSync = "Синхронізовано"
                })
                .ToListAsync();

            return Ok(userGames);
        }
    }
}
