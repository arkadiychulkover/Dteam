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
    public class CartController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CartController(AppDbContext context)
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

        private static CartItemDto MapToCartItemDto(UserCartItem item) => new()
        {
            UserId = item.UserId,
            GameId = item.GameId,
            Game = MapToGameDto(item.Game),
            AddedAt = item.AddedAt
        };

        [HttpGet]
        public async Task<ActionResult<CartSummaryDto>> GetCart()
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized(new { message = "Користувач не авторизований." });
            }

            var items = await _context.UserCartItems
                .Include(c => c.Game)
                    .ThenInclude(g => g.Owner)
                .Include(c => c.Game)
                    .ThenInclude(g => g.ParentGame)
                .AsNoTracking()
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.AddedAt)
                .ToListAsync();

            var summary = new CartSummaryDto
            {
                Items = items.Select(MapToCartItemDto).ToList()
            };

            return Ok(summary);
        }

        [HttpPost]
        public async Task<ActionResult<CartItemDto>> AddToCart([FromBody] AddToCartDto dto)
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized(new { message = "Користувач не авторизований." });
            }

            var game = await _context.Games
                .Include(g => g.Owner)
                .Include(g => g.ParentGame)
                .FirstOrDefaultAsync(g => g.Id == dto.GameId && g.IsPublished);

            if (game == null)
            {
                return NotFound(new { message = $"Гру з ID '{dto.GameId}' не знайдено." });
            }

            var existingItem = await _context.UserCartItems
                .Include(c => c.Game)
                    .ThenInclude(g => g.Owner)
                .FirstOrDefaultAsync(c => c.UserId == userId && c.GameId == dto.GameId);

            if (existingItem != null)
            {
                return Ok(MapToCartItemDto(existingItem));
            }

            var cartItem = new UserCartItem
            {
                UserId = userId,
                GameId = dto.GameId,
                AddedAt = DateTime.UtcNow,
                Game = game
            };

            await _context.UserCartItems.AddAsync(cartItem);
            await _context.SaveChangesAsync();

            return Ok(MapToCartItemDto(cartItem));
        }

        [HttpDelete("{gameId:guid}")]
        public async Task<IActionResult> RemoveFromCart(Guid gameId)
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized(new { message = "Користувач не авторизований." });
            }

            var item = await _context.UserCartItems
                .FirstOrDefaultAsync(c => c.UserId == userId && c.GameId == gameId);

            if (item == null)
            {
                return NotFound(new { message = $"Товар з ID '{gameId}' не знайдено в кошику." });
            }

            _context.UserCartItems.Remove(item);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Товар успішно видалено з кошика",
                gameId,
                userId
            });
        }

        [HttpDelete]
        public async Task<IActionResult> ClearCart()
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized(new { message = "Користувач не авторизований." });
            }

            var items = await _context.UserCartItems
                .Where(c => c.UserId == userId)
                .ToListAsync();

            if (items.Count == 0)
            {
                return Ok(new { message = "Кошик уже порожній.", count = 0 });
            }

            _context.UserCartItems.RemoveRange(items);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Кошик успішно очищено.",
                count = items.Count,
                userId
            });
        }

        [HttpPost("move-to-wishlist/{gameId:guid}")]
        public async Task<IActionResult> MoveToWishlist(Guid gameId)
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized(new { message = "Користувач не авторизований." });
            }

            var cartItem = await _context.UserCartItems
                .FirstOrDefaultAsync(c => c.UserId == userId && c.GameId == gameId);

            if (cartItem != null)
            {
                _context.UserCartItems.Remove(cartItem);
            }

            var alreadyWishlisted = await _context.UserWishlists
                .AnyAsync(w => w.UserId == userId && w.GameId == gameId);

            if (!alreadyWishlisted)
            {
                var gameExists = await _context.Games.AnyAsync(g => g.Id == gameId);
                if (gameExists)
                {
                    await _context.UserWishlists.AddAsync(new UserWishlist
                    {
                        UserId = userId,
                        GameId = gameId,
                        AddedAt = DateTime.UtcNow,
                        Priority = 0
                    });
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Гру успішно переміщено до списку бажань",
                gameId
            });
        }

        [HttpPost("checkout")]
        [HttpPost("buy")]
        public async Task<ActionResult<CheckoutResultDto>> Checkout()
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized(new { message = "Користувач не авторизований." });
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return NotFound(new { message = "Користувача не знайдено." });
            }

            if (user.IsBanned)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "Ваш акаунт заблоковано." });
            }

            var cartItems = await _context.UserCartItems
                .Include(c => c.Game)
                    .ThenInclude(g => g.Owner)
                .Where(c => c.UserId == userId)
                .ToListAsync();

            if (cartItems.Count == 0)
            {
                return BadRequest(new { message = "Кошик порожній." });
            }

            long totalRequiredNanoTons = 0;
            var gamesToPurchase = new List<(Game game, long effectivePrice)>();

            foreach (var item in cartItems)
            {
                var game = item.Game;
                long effectivePrice = game.PriceInNanoTons;
                if (game.DiscountPercentage > 0)
                {
                    effectivePrice = (long)Math.Round((double)game.PriceInNanoTons * (100.0 - game.DiscountPercentage) / 100.0);
                }
                if (effectivePrice < 0) effectivePrice = 0;

                totalRequiredNanoTons += effectivePrice;
                gamesToPurchase.Add((game, effectivePrice));
            }

            if (user.BalanceInNanoTons < totalRequiredNanoTons)
            {
                return BadRequest(new
                {
                    message = "Недостатньо коштів на балансі для здійснення покупки.",
                    requiredBalanceInNanoTons = totalRequiredNanoTons,
                    currentBalanceInNanoTons = user.BalanceInNanoTons,
                    missingNanoTons = totalRequiredNanoTons - user.BalanceInNanoTons
                });
            }

            var existingOwnedGameIds = await _context.UserGames
                .Where(ug => ug.UserId == userId)
                .Select(ug => ug.GameId)
                .ToListAsync();

            var ownedSet = new HashSet<Guid>(existingOwnedGameIds);

            // Deduct balance
            user.BalanceInNanoTons -= totalRequiredNanoTons;
            user.UpdatedAt = DateTime.UtcNow;

            var addedCount = 0;
            foreach (var (game, effectivePrice) in gamesToPurchase)
            {
                if (!ownedSet.Contains(game.Id))
                {
                    await _context.UserGames.AddAsync(new UserGame
                    {
                        UserId = user.Id,
                        GameId = game.Id,
                        PurchasedAt = DateTime.UtcNow,
                        PlayTimeMinutes = 0,
                        IsFavorite = false
                    });

                    game.DownloadCount += 1;
                    if (game.Owner != null && effectivePrice > 0)
                    {
                        game.Owner.TotalEarningsInNanoTons += effectivePrice;
                    }

                    addedCount++;
                }
            }

            // Remove from wishlist if present
            var purchasedGameIds = gamesToPurchase.Select(g => g.game.Id).ToList();
            var wishlistsToRemove = await _context.UserWishlists
                .Where(w => w.UserId == userId && purchasedGameIds.Contains(w.GameId))
                .ToListAsync();

            if (wishlistsToRemove.Count > 0)
            {
                _context.UserWishlists.RemoveRange(wishlistsToRemove);
            }

            // Clear Cart
            _context.UserCartItems.RemoveRange(cartItems);

            await _context.SaveChangesAsync();

            return Ok(new CheckoutResultDto
            {
                Success = true,
                Message = addedCount == 1 
                    ? "Гру успішно придбано та додано до вашої бібліотеки!" 
                    : $"Успішно придбано {addedCount} ігор та додано до вашої бібліотеки!",
                NewBalanceInNanoTons = user.BalanceInNanoTons,
                TotalSpentInNanoTons = totalRequiredNanoTons,
                PurchasedGamesCount = addedCount
            });
        }
    }
}
