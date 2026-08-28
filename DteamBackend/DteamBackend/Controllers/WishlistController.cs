using System.Security.Claims;
using DteamBackend.Data;
using DteamBackend.Models;
using DteamBackend.Models.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DteamBackend.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class WishlistController : ControllerBase
    {
        private readonly AppDbContext _context;

        public WishlistController(AppDbContext context)
        {
            _context = context;
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                           ?? User.FindFirst("sub")?.Value;

            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }

        private static GameDto MapToGameDto(Game game) => new()
        {
            Id = game.Id,
            Title = game.Title,
            Description = game.Description,
            ShortDescription = game.ShortDescription,
            PriceInNanoTons = game.PriceInNanoTons,
            DiscountPercentage = game.DiscountPercentage,
            ServerArchivePath = game.ServerArchivePath,
            OwnerId = game.OwnerId,
            OwnerUsername = game.Owner?.Username,
            DownloadCount = game.DownloadCount,
            AverageRating = game.AverageRating,
            ReviewsCount = game.ReviewsCount,
            IsDlc = game.IsDlc,
            ParentGameId = game.ParentGameId,
            ParentGameTitle = game.ParentGame?.Title,
            Genres = game.Genres ?? new List<string>(),
            Platforms = game.Platforms ?? new List<string>(),
            Features = game.Features ?? new List<string>(),
            Tags = game.Tags ?? new List<string>(),
            Version = game.Version,
            SizeInBytes = game.SizeInBytes,
            IsPublished = game.IsPublished,
            HeaderImageUrl = game.HeaderImageUrl,
            CoverImageUrl = game.CoverImageUrl,
            ScreenshotUrls = game.ScreenshotUrls ?? new List<string>(),
            TrailerUrl = game.TrailerUrl,
            CreatedAt = game.CreatedAt,
            UpdatedAt = game.UpdatedAt
        };

        private static WishlistItemDto MapToWishlistItemDto(UserWishlist item) => new()
        {
            UserId = item.UserId,
            GameId = item.GameId,
            Game = MapToGameDto(item.Game),
            AddedAt = item.AddedAt,
            Priority = item.Priority,
            Notes = item.Notes
        };

        [HttpGet]
        public async Task<ActionResult<IEnumerable<WishlistItemDto>>> GetWishlist(
            [FromQuery] string? search,
            [FromQuery] string? sortBy = "date_added")
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized(new { message = "Пользователь не авторизован." });
            }

            var query = _context.UserWishlists
                .Include(w => w.Game)
                    .ThenInclude(g => g.Owner)
                .Include(w => w.Game)
                    .ThenInclude(g => g.ParentGame)
                .AsNoTracking()
                .Where(w => w.UserId == userId);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                query = query.Where(w =>
                    w.Game.Title.ToLower().Contains(s) ||
                    (w.Game.ShortDescription != null && w.Game.ShortDescription.ToLower().Contains(s)) ||
                    w.Game.Tags.Any(t => t.ToLower().Contains(s)) ||
                    w.Game.Genres.Any(g => g.ToLower().Contains(s)));
            }

            query = sortBy?.ToLower() switch
            {
                "discounts" or "discount" => query.OrderByDescending(w => w.Game.DiscountPercentage),
                "price_asc" => query.OrderBy(w => w.Game.PriceInNanoTons * (100 - w.Game.DiscountPercentage) / 100),
                "price_desc" => query.OrderByDescending(w => w.Game.PriceInNanoTons * (100 - w.Game.DiscountPercentage) / 100),
                "priority" => query.OrderByDescending(w => w.Priority).ThenByDescending(w => w.AddedAt),
                "title" => query.OrderBy(w => w.Game.Title),
                "date_added" or _ => query.OrderByDescending(w => w.AddedAt)
            };

            var items = await query.ToListAsync();

            return Ok(items.Select(MapToWishlistItemDto));
        }

        [HttpGet("{gameId:guid}")]
        public async Task<ActionResult<object>> CheckGameInWishlist(Guid gameId)
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized(new { message = "Пользователь не авторизован." });
            }

            var item = await _context.UserWishlists
                .Include(w => w.Game)
                    .ThenInclude(g => g.Owner)
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.UserId == userId && w.GameId == gameId);

            if (item == null)
            {
                return Ok(new { isInWishlist = false, item = (WishlistItemDto?)null });
            }

            return Ok(new { isInWishlist = true, item = MapToWishlistItemDto(item) });
        }

        [HttpPost]
        public async Task<ActionResult<WishlistItemDto>> AddToWishlist([FromBody] AddToWishlistDto dto)
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized(new { message = "Пользователь не авторизован." });
            }

            var game = await _context.Games
                .Include(g => g.Owner)
                .Include(g => g.ParentGame)
                .FirstOrDefaultAsync(g => g.Id == dto.GameId);

            if (game == null)
            {
                return NotFound(new { message = $"Игра с ID '{dto.GameId}' не найдена." });
            }

            var existingItem = await _context.UserWishlists
                .Include(w => w.Game)
                    .ThenInclude(g => g.Owner)
                .FirstOrDefaultAsync(w => w.UserId == userId && w.GameId == dto.GameId);

            if (existingItem != null)
            {
                if (dto.Priority.HasValue) existingItem.Priority = dto.Priority.Value;
                if (dto.Notes != null) existingItem.Notes = dto.Notes;
                await _context.SaveChangesAsync();

                return Ok(MapToWishlistItemDto(existingItem));
            }

            var wishlistItem = new UserWishlist
            {
                UserId = userId,
                GameId = dto.GameId,
                AddedAt = DateTime.UtcNow,
                Priority = dto.Priority ?? 0,
                Notes = dto.Notes
            };

            await _context.UserWishlists.AddAsync(wishlistItem);
            await _context.SaveChangesAsync();

            wishlistItem.Game = game;

            return CreatedAtAction(nameof(CheckGameInWishlist), new { gameId = dto.GameId }, MapToWishlistItemDto(wishlistItem));
        }

        [HttpPut("{gameId:guid}")]
        public async Task<ActionResult<WishlistItemDto>> UpdateWishlistItem(
            Guid gameId,
            [FromBody] UpdateWishlistItemDto dto)
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized(new { message = "Пользователь не авторизован." });
            }

            var item = await _context.UserWishlists
                .Include(w => w.Game)
                    .ThenInclude(g => g.Owner)
                .FirstOrDefaultAsync(w => w.UserId == userId && w.GameId == gameId);

            if (item == null)
            {
                return NotFound(new { message = $"Игра с ID '{gameId}' не найдена в списке желаемого." });
            }

            if (dto.Priority.HasValue) item.Priority = dto.Priority.Value;
            if (dto.Notes != null) item.Notes = dto.Notes;

            await _context.SaveChangesAsync();

            return Ok(MapToWishlistItemDto(item));
        }

        [HttpDelete("{gameId:guid}")]
        public async Task<IActionResult> RemoveFromWishlist(Guid gameId)
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized(new { message = "Пользователь не авторизован." });
            }

            var item = await _context.UserWishlists
                .FirstOrDefaultAsync(w => w.UserId == userId && w.GameId == gameId);

            if (item == null)
            {
                return NotFound(new { message = $"Игра с ID '{gameId}' не найдена в списке желаемого." });
            }

            _context.UserWishlists.Remove(item);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Игра успешно удалена из списка желаемого",
                gameId,
                userId
            });
        }

        [HttpDelete]
        public async Task<IActionResult> ClearWishlist()
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized(new { message = "Пользователь не авторизован." });
            }

            var items = await _context.UserWishlists
                .Where(w => w.UserId == userId)
                .ToListAsync();

            if (items.Count == 0)
            {
                return Ok(new { message = "Список желаемого уже пуст.", count = 0 });
            }

            _context.UserWishlists.RemoveRange(items);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Список желаемого успешно очищен.",
                count = items.Count,
                userId
            });
        }
    }
}
