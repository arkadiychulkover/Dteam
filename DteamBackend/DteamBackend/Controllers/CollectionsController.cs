using System.Security.Claims;
using DteamBackend.Data;
using DteamBackend.Models;
using DteamBackend.Models.DTO;
using DteamBackend.Models.DTO.Library;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DteamBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CollectionsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CollectionsController> _logger;

        public CollectionsController(AppDbContext context, ILogger<CollectionsController> logger)
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

        [HttpGet]
        [ProducesResponseType(typeof(List<GameCollectionDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<GameCollectionDto>>> GetMyCollections()
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized(new { message = "Користувач не авторизований." });
            }

            var collections = await _context.GameCollections
                .AsNoTracking()
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.UpdatedAt)
                .Include(c => c.Items)
                    .ThenInclude(i => i.Game)
                        .ThenInclude(g => g.Owner)
                .ToListAsync();

            var result = collections.Select(MapToCollectionDto).ToList();
            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(GameCollectionDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GameCollectionDto>> GetCollection(Guid id)
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized(new { message = "Користувач не авторизований." });
            }

            var collection = await _context.GameCollections
                .AsNoTracking()
                .Include(c => c.Items)
                    .ThenInclude(i => i.Game)
                        .ThenInclude(g => g.Owner)
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (collection == null)
            {
                return NotFound(new { message = "Колекцію не знайдено." });
            }

            return Ok(MapToCollectionDto(collection));
        }

        [HttpPost]
        [ProducesResponseType(typeof(GameCollectionDto), StatusCodes.Status201Created)]
        public async Task<ActionResult<GameCollectionDto>> CreateCollection([FromBody] CreateGameCollectionDto dto)
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized(new { message = "Користувач не авторизований." });
            }

            var normalizedName = dto.Name.Trim();
            var exists = await _context.GameCollections
                .AnyAsync(c => c.UserId == userId && c.Name.ToLower() == normalizedName.ToLower());

            if (exists)
            {
                return BadRequest(new { message = $"Колекція з назвою '{normalizedName}' вже існує." });
            }

            var collection = new GameCollection
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = normalizedName,
                Description = dto.Description?.Trim(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            if (dto.InitialGameIds != null && dto.InitialGameIds.Count > 0)
            {
                var validGameIds = await _context.Games
                    .Where(g => dto.InitialGameIds.Contains(g.Id))
                    .Select(g => g.Id)
                    .ToListAsync();

                foreach (var gameId in validGameIds)
                {
                    collection.Items.Add(new GameCollectionItem
                    {
                        CollectionId = collection.Id,
                        GameId = gameId,
                        AddedAt = DateTime.UtcNow
                    });
                }
            }

            _context.GameCollections.Add(collection);
            await _context.SaveChangesAsync();

            _logger.LogInformation("[Collections] User {UserId} created collection {CollectionId} '{Name}'", userId, collection.Id, collection.Name);

            var created = await _context.GameCollections
                .AsNoTracking()
                .Include(c => c.Items)
                    .ThenInclude(i => i.Game)
                        .ThenInclude(g => g.Owner)
                .FirstAsync(c => c.Id == collection.Id);

            return CreatedAtAction(nameof(GetCollection), new { id = collection.Id }, MapToCollectionDto(created));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(GameCollectionDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GameCollectionDto>> UpdateCollection(Guid id, [FromBody] UpdateGameCollectionDto dto)
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized(new { message = "Користувач не авторизований." });
            }

            var collection = await _context.GameCollections
                .Include(c => c.Items)
                    .ThenInclude(i => i.Game)
                        .ThenInclude(g => g.Owner)
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (collection == null)
            {
                return NotFound(new { message = "Колекцію не знайдено." });
            }

            var normalizedName = dto.Name.Trim();
            var duplicate = await _context.GameCollections
                .AnyAsync(c => c.UserId == userId && c.Id != id && c.Name.ToLower() == normalizedName.ToLower());

            if (duplicate)
            {
                return BadRequest(new { message = $"Колекція з назвою '{normalizedName}' вже існує." });
            }

            collection.Name = normalizedName;
            collection.Description = dto.Description?.Trim();
            collection.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(MapToCollectionDto(collection));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteCollection(Guid id)
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized(new { message = "Користувач не авторизований." });
            }

            var collection = await _context.GameCollections
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (collection == null)
            {
                return NotFound(new { message = "Колекцію не знайдено." });
            }

            _context.GameCollections.Remove(collection);
            await _context.SaveChangesAsync();

            _logger.LogInformation("[Collections] User {UserId} deleted collection {CollectionId}", userId, id);

            return Ok(new { message = "Колекцію успішно видалено." });
        }

        [HttpPost("{id:guid}/games")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddGameToCollection(Guid id, [FromBody] AddGameToCollectionDto dto)
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized(new { message = "Користувач не авторизований." });
            }

            var collection = await _context.GameCollections
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (collection == null)
            {
                return NotFound(new { message = "Колекцію не знайдено." });
            }

            var gameExists = await _context.Games.AnyAsync(g => g.Id == dto.GameId);
            if (!gameExists)
            {
                return NotFound(new { message = "Гру не знайдено." });
            }

            var alreadyInCollection = collection.Items.Any(i => i.GameId == dto.GameId);
            if (alreadyInCollection)
            {
                return BadRequest(new { message = "Ця гра вже є у цій колекції." });
            }

            collection.Items.Add(new GameCollectionItem
            {
                CollectionId = id,
                GameId = dto.GameId,
                AddedAt = DateTime.UtcNow
            });
            collection.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var updatedCollection = await _context.GameCollections
                .AsNoTracking()
                .Include(c => c.Items)
                    .ThenInclude(i => i.Game)
                        .ThenInclude(g => g.Owner)
                .FirstAsync(c => c.Id == id);

            return Ok(MapToCollectionDto(updatedCollection));
        }

        [HttpDelete("{id:guid}/games/{gameId:guid}")]
        [ProducesResponseType(typeof(GameCollectionDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GameCollectionDto>> RemoveGameFromCollection(Guid id, Guid gameId)
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized(new { message = "Користувач не авторизований." });
            }

            var collection = await _context.GameCollections
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (collection == null)
            {
                return NotFound(new { message = "Колекцію не знайдено." });
            }

            var item = collection.Items.FirstOrDefault(i => i.GameId == gameId);
            if (item == null)
            {
                return NotFound(new { message = "Гру не знайдено в цій колекції." });
            }

            collection.Items.Remove(item);
            collection.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var updatedCollection = await _context.GameCollections
                .AsNoTracking()
                .Include(c => c.Items)
                    .ThenInclude(i => i.Game)
                        .ThenInclude(g => g.Owner)
                .FirstAsync(c => c.Id == id);

            return Ok(MapToCollectionDto(updatedCollection));
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

        private static GameCollectionDto MapToCollectionDto(GameCollection collection)
        {
            var items = collection.Items ?? new List<GameCollectionItem>();
            return new GameCollectionDto
            {
                Id = collection.Id,
                UserId = collection.UserId,
                Name = collection.Name,
                Description = collection.Description,
                CreatedAt = collection.CreatedAt,
                UpdatedAt = collection.UpdatedAt,
                GamesCount = items.Count,
                Items = items.Select(i => new GameCollectionItemDto
                {
                    GameId = i.GameId,
                    Title = i.Game?.Title ?? string.Empty,
                    CoverImageUrl = i.Game?.CoverImageUrl ?? i.Game?.HeaderImageUrl,
                    AddedAt = i.AddedAt
                }).ToList(),
                Games = items
                    .Where(i => i.Game != null)
                    .Select(i => MapToGameDto(i.Game))
                    .ToList()
            };
        }
    }
}
