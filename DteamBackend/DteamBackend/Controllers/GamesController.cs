using DteamBackend.Data;
using DteamBackend.Models;
using DteamBackend.Models.DTO;
using DteamBackend.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DteamBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GamesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public GamesController(AppDbContext context)
        {
            _context = context;
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
            Dlcs = game.Dlcs != null ? game.Dlcs.Select(d => new GameDto
            {
                Id = d.Id,
                Title = d.Title,
                Description = d.Description,
                ShortDescription = d.ShortDescription,
                PriceInNanoTons = d.PriceInNanoTons,
                DiscountPercentage = d.DiscountPercentage,
                ServerArchivePath = d.ServerArchivePath,
                OwnerId = d.OwnerId,
                DownloadCount = d.DownloadCount,
                AverageRating = d.AverageRating,
                ReviewsCount = d.ReviewsCount,
                IsDlc = d.IsDlc,
                ParentGameId = d.ParentGameId,
                Genres = d.Genres ?? new List<string>(),
                Platforms = d.Platforms ?? new List<string>(),
                Features = d.Features ?? new List<string>(),
                Tags = d.Tags ?? new List<string>(),
                Version = d.Version,
                SizeInBytes = d.SizeInBytes,
                IsPublished = d.IsPublished,
                HeaderImageUrl = d.HeaderImageUrl,
                CoverImageUrl = d.CoverImageUrl,
                ScreenshotUrls = d.ScreenshotUrls ?? new List<string>(),
                TrailerUrl = d.TrailerUrl,
                CreatedAt = d.CreatedAt,
                UpdatedAt = d.UpdatedAt
            }).ToList() : new List<GameDto>(),
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

        [HttpGet]
        public async Task<ActionResult<IEnumerable<GameDto>>> GetGames(
            [FromQuery] string? search,
            [FromQuery] string? genre,
            [FromQuery] bool? isDlc,
            [FromQuery] bool? isDiscounted,
            [FromQuery] bool? isFree,
            [FromQuery] long? minPrice,
            [FromQuery] long? maxPrice,
            [FromQuery] string? platform,
            [FromQuery] string? feature,
            [FromQuery] string? tag,
            [FromQuery] string? sortBy = "relevance")
        {
            var query = _context.Games
                .Include(g => g.Owner)
                .Include(g => g.ParentGame)
                .Include(g => g.Dlcs)
                .AsNoTracking()
                .Where(g => g.IsPublished);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                query = query.Where(g =>
                    g.Title.ToLower().Contains(s) ||
                    (g.ShortDescription != null && g.ShortDescription.ToLower().Contains(s)) ||
                    g.Description.ToLower().Contains(s) ||
                    g.Tags.Any(t => t.ToLower().Contains(s)) ||
                    g.Genres.Any(gen => gen.ToLower().Contains(s)));
            }

            if (!string.IsNullOrWhiteSpace(genre) && genre != "All Games" && genre != "Все")
            {
                var gLower = genre.Trim().ToLower();
                query = query.Where(g => g.Genres.Any(gen => gen.ToLower() == gLower) || g.Tags.Any(t => t.ToLower() == gLower));
            }

            if (isDlc.HasValue)
            {
                query = query.Where(g => g.IsDlc == isDlc.Value);
            }

            if (isDiscounted == true)
            {
                query = query.Where(g => g.DiscountPercentage > 0);
            }

            if (isFree == true)
            {
                query = query.Where(g => g.PriceInNanoTons == 0 || g.DiscountPercentage == 100);
            }

            if (minPrice.HasValue)
            {
                query = query.Where(g => (g.PriceInNanoTons * (100 - g.DiscountPercentage) / 100) >= minPrice.Value);
            }
            if (maxPrice.HasValue)
            {
                query = query.Where(g => (g.PriceInNanoTons * (100 - g.DiscountPercentage) / 100) <= maxPrice.Value);
            }

            if (!string.IsNullOrWhiteSpace(platform))
            {
                var pLower = platform.Trim().ToLower();
                query = query.Where(g => g.Platforms.Any(p => p.ToLower() == pLower));
            }

            if (!string.IsNullOrWhiteSpace(feature))
            {
                var fLower = feature.Trim().ToLower();
                query = query.Where(g => g.Features.Any(f => f.ToLower() == fLower));
            }

            if (!string.IsNullOrWhiteSpace(tag))
            {
                var tLower = tag.Trim().ToLower();
                query = query.Where(g => g.Tags.Any(t => t.ToLower().Contains(tLower)));
            }

            query = sortBy?.ToLowerInvariant() switch
            {
                "discount" or "discounts" => query.OrderByDescending(g => g.DiscountPercentage).ThenBy(g => g.PriceInNanoTons),
                "price_asc" => query.OrderBy(g => g.PriceInNanoTons * (100 - g.DiscountPercentage) / 100),
                "price_desc" => query.OrderByDescending(g => g.PriceInNanoTons * (100 - g.DiscountPercentage) / 100),
                "rating" => query.OrderByDescending(g => g.AverageRating).ThenByDescending(g => g.ReviewsCount),
                "newest" => query.OrderByDescending(g => g.CreatedAt),
                "downloads" or "popular" => query.OrderByDescending(g => g.DownloadCount),
                "relevance" or _ => query.OrderByDescending(g => g.DownloadCount).ThenByDescending(g => g.CreatedAt)
            };

            var games = await query.ToListAsync();

            return Ok(games.Select(MapToGameDto));
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<GameDto>> GetGameById(Guid id)
        {
            var game = await _context.Games
                .Include(g => g.Owner)
                .Include(g => g.ParentGame)
                .Include(g => g.Dlcs)
                .FirstOrDefaultAsync(g => g.Id == id && g.IsPublished);

            if (game == null)
            {
                return NotFound(new { message = $"Игра с ID '{id}' не найдена" });
            }

            return Ok(MapToGameDto(game));
        }

        [HttpGet("genres")]
        public ActionResult<IEnumerable<string>> GetGenres()
        {
            var genres = Enum.GetNames(typeof(GameGenre)).ToList();
            return Ok(genres);
        }

        [HttpPost("upload-image")]
        public async Task<ActionResult<object>> UploadGameImage(IFormFile? file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "Файл не передан или пустой." });
            }

            if (file.Length > 20 * 1024 * 1024)
            {
                return BadRequest(new { message = "Размер файла превышает 20 МБ." });
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif", ".svg" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                return BadRequest(new { message = "Недопустимый формат файла. Разрешены форматы: .jpg, .jpeg, .png, .webp, .gif, .svg" });
            }

            var webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var imagesFolder = Path.Combine(webRootPath, "game_images");

            if (!Directory.Exists(imagesFolder))
            {
                Directory.CreateDirectory(imagesFolder);
            }

            var uniqueFileName = $"{Guid.NewGuid():N}{extension}";
            var filePath = Path.Combine(imagesFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var relativeUrl = $"/game_images/{uniqueFileName}";

            return Ok(new
            {
                url = relativeUrl,
                fileName = uniqueFileName,
                size = file.Length,
                message = "Изображение успешно загружено"
            });
        }
    }
}
