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
        [ProducesResponseType(typeof(IEnumerable<GameDto>), StatusCodes.Status200OK)]
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
        [ProducesResponseType(typeof(GameDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GameDto>> GetGameById(Guid id)
        {
            var game = await _context.Games
                .Include(g => g.Owner)
                .Include(g => g.ParentGame)
                .Include(g => g.Dlcs)
                .AsNoTracking()
                .FirstOrDefaultAsync(g => g.Id == id && g.IsPublished);

            if (game == null)
            {
                return NotFound(new { message = $"Игра с ID '{id}' не найдена" });
            }

            return Ok(MapToGameDto(game));
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                           ?? User.FindFirst("sub")?.Value;

            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }

        [HttpGet("{id:guid}/reviews")]
        [ProducesResponseType(typeof(IEnumerable<ReviewDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ReviewDto>>> GetGameReviews(Guid id)
        {
            var currentUserId = GetCurrentUserId();
            var currentUserIdStr = currentUserId != Guid.Empty ? currentUserId.ToString() : null;

            var allReviews = await _context.Reviews
                .Include(r => r.User)
                .AsNoTracking()
                .Where(r => r.GameId == id)
                .OrderBy(r => r.CreatedAt)
                .ToListAsync();

            var reviewMap = allReviews.ToDictionary(r => r.Id);

            var repliesByRootId = new Dictionary<Guid, List<Review>>();
            var topLevelReviews = allReviews.Where(r => !r.ParentReviewId.HasValue).OrderByDescending(r => r.CreatedAt).ToList();

            foreach (var top in topLevelReviews)
            {
                repliesByRootId[top.Id] = new List<Review>();
            }

            foreach (var reply in allReviews.Where(r => r.ParentReviewId.HasValue))
            {
                var curr = reply;
                while (curr.ParentReviewId.HasValue && reviewMap.ContainsKey(curr.ParentReviewId.Value))
                {
                    curr = reviewMap[curr.ParentReviewId.Value];
                }
                if (repliesByRootId.ContainsKey(curr.Id))
                {
                    repliesByRootId[curr.Id].Add(reply);
                }
            }

            var result = topLevelReviews.Select(r =>
            {
                var replies = repliesByRootId.GetValueOrDefault(r.Id, new List<Review>());
                return new ReviewDto
                {
                    Id = r.Id,
                    ParentReviewId = r.ParentReviewId,
                    UserId = r.UserId,
                    Username = r.User != null ? r.User.Username : "Анонім",
                    UserAvatarUrl = r.User != null ? r.User.AvatarUrl : null,
                    GameId = r.GameId,
                    Rating = r.Rating,
                    Content = r.Content,
                    IsRecommended = r.IsRecommended,
                    PlayTimeHoursAtReview = r.PlayTimeHoursAtReview,
                    LikesCount = r.LikedByUsers?.Count ?? r.LikesCount,
                    IsLiked = currentUserIdStr != null && (r.LikedByUsers?.Contains(currentUserIdStr) ?? false),
                    RepliesCount = replies.Count,
                    Replies = replies.OrderBy(rp => rp.CreatedAt).Select(rp =>
                    {
                        ReviewParentInfoDto? parentInfo = null;
                        if (rp.ParentReviewId.HasValue && reviewMap.TryGetValue(rp.ParentReviewId.Value, out var pReview))
                        {
                            parentInfo = new ReviewParentInfoDto
                            {
                                Id = pReview.Id,
                                UserId = pReview.UserId,
                                Username = pReview.User != null ? pReview.User.Username : "Користувач",
                                UserAvatarUrl = pReview.User?.AvatarUrl,
                                Content = pReview.Content
                            };
                        }

                        return new ReviewDto
                        {
                            Id = rp.Id,
                            ParentReviewId = rp.ParentReviewId,
                            UserId = rp.UserId,
                            Username = rp.User != null ? rp.User.Username : "Анонім",
                            UserAvatarUrl = rp.User != null ? rp.User.AvatarUrl : null,
                            GameId = rp.GameId,
                            Rating = rp.Rating,
                            Content = rp.Content,
                            IsRecommended = rp.IsRecommended,
                            PlayTimeHoursAtReview = rp.PlayTimeHoursAtReview,
                            LikesCount = rp.LikedByUsers?.Count ?? rp.LikesCount,
                            IsLiked = currentUserIdStr != null && (rp.LikedByUsers?.Contains(currentUserIdStr) ?? false),
                            ParentReview = parentInfo,
                            CreatedAt = rp.CreatedAt,
                            UpdatedAt = rp.UpdatedAt
                        };
                    }).ToList(),
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt
                };
            }).ToList();

            return Ok(result);
        }

        [HttpGet("{id:guid}/reviews/{reviewId:guid}")]
        [ProducesResponseType(typeof(ReviewDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ReviewDto>> GetReviewDetails(Guid id, Guid reviewId)
        {
            var currentUserId = GetCurrentUserId();
            var currentUserIdStr = currentUserId != Guid.Empty ? currentUserId.ToString() : null;

            var allReviews = await _context.Reviews
                .Include(r => r.User)
                .AsNoTracking()
                .Where(r => r.GameId == id)
                .OrderBy(r => r.CreatedAt)
                .ToListAsync();

            var reviewMap = allReviews.ToDictionary(r => r.Id);
            if (!reviewMap.TryGetValue(reviewId, out var targetReview))
            {
                return NotFound(new { message = "Відгук не знайдено." });
            }

            var threadReplies = new List<Review>();
            foreach (var r in allReviews.Where(r => r.ParentReviewId.HasValue))
            {
                var curr = r;
                while (curr.ParentReviewId.HasValue && reviewMap.ContainsKey(curr.ParentReviewId.Value))
                {
                    if (curr.ParentReviewId.Value == reviewId)
                    {
                        threadReplies.Add(r);
                        break;
                    }
                    curr = reviewMap[curr.ParentReviewId.Value];
                }
            }

            var dto = new ReviewDto
            {
                Id = targetReview.Id,
                ParentReviewId = targetReview.ParentReviewId,
                UserId = targetReview.UserId,
                Username = targetReview.User != null ? targetReview.User.Username : "Анонім",
                UserAvatarUrl = targetReview.User != null ? targetReview.User.AvatarUrl : null,
                GameId = targetReview.GameId,
                Rating = targetReview.Rating,
                Content = targetReview.Content,
                IsRecommended = targetReview.IsRecommended,
                PlayTimeHoursAtReview = targetReview.PlayTimeHoursAtReview,
                LikesCount = targetReview.LikedByUsers?.Count ?? targetReview.LikesCount,
                IsLiked = currentUserIdStr != null && (targetReview.LikedByUsers?.Contains(currentUserIdStr) ?? false),
                RepliesCount = threadReplies.Count,
                Replies = threadReplies.OrderBy(rp => rp.CreatedAt).Select(rp =>
                {
                    ReviewParentInfoDto? parentInfo = null;
                    if (rp.ParentReviewId.HasValue && reviewMap.TryGetValue(rp.ParentReviewId.Value, out var pReview))
                    {
                        parentInfo = new ReviewParentInfoDto
                        {
                            Id = pReview.Id,
                            UserId = pReview.UserId,
                            Username = pReview.User != null ? pReview.User.Username : "Користувач",
                            UserAvatarUrl = pReview.User?.AvatarUrl,
                            Content = pReview.Content
                        };
                    }

                    return new ReviewDto
                    {
                        Id = rp.Id,
                        ParentReviewId = rp.ParentReviewId,
                        UserId = rp.UserId,
                        Username = rp.User != null ? rp.User.Username : "Анонім",
                        UserAvatarUrl = rp.User != null ? rp.User.AvatarUrl : null,
                        GameId = rp.GameId,
                        Rating = rp.Rating,
                        Content = rp.Content,
                        IsRecommended = rp.IsRecommended,
                        PlayTimeHoursAtReview = rp.PlayTimeHoursAtReview,
                        LikesCount = rp.LikedByUsers?.Count ?? rp.LikesCount,
                        IsLiked = currentUserIdStr != null && (rp.LikedByUsers?.Contains(currentUserIdStr) ?? false),
                        ParentReview = parentInfo,
                        CreatedAt = rp.CreatedAt,
                        UpdatedAt = rp.UpdatedAt
                    };
                }).ToList(),
                CreatedAt = targetReview.CreatedAt,
                UpdatedAt = targetReview.UpdatedAt
            };

            return Ok(dto);
        }

        [Authorize]
        [HttpPost("{id:guid}/reviews")]
        [ProducesResponseType(typeof(ReviewDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ReviewDto>> PostReview(Guid id, [FromBody] CreateReviewDto dto)
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized(new { message = "Потрібна авторизація для публікації рецензії або коментаря." });
            }

            var game = await _context.Games.FirstOrDefaultAsync(g => g.Id == id && g.IsPublished);
            if (game == null)
            {
                return NotFound(new { message = "Гру не знайдено." });
            }

            Review savedReview;

            if (dto.ParentReviewId.HasValue && dto.ParentReviewId.Value != Guid.Empty)
            {
                var parentReview = await _context.Reviews
                    .Include(r => r.User)
                    .FirstOrDefaultAsync(r => r.Id == dto.ParentReviewId.Value && r.GameId == id);

                if (parentReview == null)
                {
                    return NotFound(new { message = "Батьківський відгук або коментар не знайдено." });
                }

                var newReply = new Review
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    GameId = id,
                    ParentReviewId = parentReview.Id,
                    Rating = parentReview.Rating,
                    Content = dto.Content.Trim(),
                    IsRecommended = parentReview.IsRecommended,
                    PlayTimeHoursAtReview = 0,
                    CreatedAt = DateTime.UtcNow,
                    LikesCount = 0,
                    LikedByUsers = new List<string>()
                };

                await _context.Reviews.AddAsync(newReply);
                await _context.SaveChangesAsync();

                savedReview = (await _context.Reviews
                    .Include(r => r.User)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(r => r.Id == newReply.Id))!;

                return Ok(new ReviewDto
                {
                    Id = savedReview.Id,
                    ParentReviewId = savedReview.ParentReviewId,
                    UserId = savedReview.UserId,
                    Username = savedReview.User?.Username ?? "Користувач",
                    UserAvatarUrl = savedReview.User?.AvatarUrl,
                    GameId = savedReview.GameId,
                    Rating = savedReview.Rating,
                    Content = savedReview.Content,
                    IsRecommended = savedReview.IsRecommended,
                    PlayTimeHoursAtReview = savedReview.PlayTimeHoursAtReview,
                    LikesCount = 0,
                    IsLiked = false,
                    RepliesCount = 0,
                    ParentReview = new ReviewParentInfoDto
                    {
                        Id = parentReview.Id,
                        UserId = parentReview.UserId,
                        Username = parentReview.User?.Username ?? "Користувач",
                        UserAvatarUrl = parentReview.User?.AvatarUrl,
                        Content = parentReview.Content
                    },
                    CreatedAt = savedReview.CreatedAt,
                    UpdatedAt = savedReview.UpdatedAt
                });
            }

            // Top-level review
            var existingReview = await _context.Reviews.FirstOrDefaultAsync(r => r.UserId == userId && r.GameId == id && !r.ParentReviewId.HasValue);
            if (existingReview != null)
            {
                existingReview.Rating = Math.Clamp(dto.Rating, 1, 5);
                existingReview.Content = dto.Content.Trim();
                existingReview.IsRecommended = dto.IsRecommended;
                existingReview.UpdatedAt = DateTime.UtcNow;
                savedReview = existingReview;
            }
            else
            {
                var newReview = new Review
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    GameId = id,
                    ParentReviewId = null,
                    Rating = Math.Clamp(dto.Rating, 1, 5),
                    Content = dto.Content.Trim(),
                    IsRecommended = dto.IsRecommended,
                    PlayTimeHoursAtReview = 0,
                    CreatedAt = DateTime.UtcNow,
                    LikesCount = 0,
                    LikedByUsers = new List<string>()
                };
                await _context.Reviews.AddAsync(newReview);
                savedReview = newReview;
            }

            await _context.SaveChangesAsync();

            // Calculate average rating and review count only based on top-level reviews
            var topReviews = await _context.Reviews.Where(r => r.GameId == id && !r.ParentReviewId.HasValue).ToListAsync();
            game.ReviewsCount = topReviews.Count;
            game.AverageRating = topReviews.Count > 0 ? Math.Round(topReviews.Average(r => (double)r.Rating), 1) : 5.0;
            await _context.SaveChangesAsync();

            var reloaded = await _context.Reviews
                .Include(r => r.User)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == savedReview.Id);

            var userIdStr = userId.ToString();
            return Ok(new ReviewDto
            {
                Id = reloaded!.Id,
                ParentReviewId = reloaded.ParentReviewId,
                UserId = reloaded.UserId,
                Username = reloaded.User?.Username ?? "Користувач",
                UserAvatarUrl = reloaded.User?.AvatarUrl,
                GameId = reloaded.GameId,
                Rating = reloaded.Rating,
                Content = reloaded.Content,
                IsRecommended = reloaded.IsRecommended,
                PlayTimeHoursAtReview = reloaded.PlayTimeHoursAtReview,
                LikesCount = reloaded.LikedByUsers?.Count ?? reloaded.LikesCount,
                IsLiked = reloaded.LikedByUsers?.Contains(userIdStr) ?? false,
                RepliesCount = await _context.Reviews.CountAsync(r => r.ParentReviewId == reloaded.Id),
                CreatedAt = reloaded.CreatedAt,
                UpdatedAt = reloaded.UpdatedAt
            });
        }

        [Authorize]
        [HttpPost("{id:guid}/reviews/{reviewId:guid}/like")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ToggleLikeReview(Guid id, Guid reviewId)
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized(new { message = "Потрібна авторизація." });
            }

            var review = await _context.Reviews.FirstOrDefaultAsync(r => r.Id == reviewId && r.GameId == id);
            if (review == null)
            {
                return NotFound(new { message = "Відгук або коментар не знайдено." });
            }

            var userIdStr = userId.ToString();
            bool liked;
            if (review.LikedByUsers.Contains(userIdStr))
            {
                review.LikedByUsers.Remove(userIdStr);
                liked = false;
            }
            else
            {
                review.LikedByUsers.Add(userIdStr);
                liked = true;
            }

            review.LikesCount = review.LikedByUsers.Count;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                liked,
                likesCount = review.LikesCount
            });
        }

        [HttpGet("{id:guid}/dlcs")]
        [ProducesResponseType(typeof(IEnumerable<GameDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<GameDto>>> GetGameDlcs(Guid id)
        {
            var dlcs = await _context.Games
                .Include(g => g.Owner)
                .AsNoTracking()
                .Where(g => g.IsDlc && g.ParentGameId == id && g.IsPublished)
                .OrderBy(g => g.PriceInNanoTons)
                .ToListAsync();

            return Ok(dlcs.Select(MapToGameDto));
        }

        [HttpGet("genres")]
        [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<string>> GetGenres()
        {
            var genres = Enum.GetNames(typeof(GameGenre)).ToList();
            return Ok(genres);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("upload-image")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
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

