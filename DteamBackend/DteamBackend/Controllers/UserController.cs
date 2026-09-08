using System.Security.Claims;
using DteamBackend.Data;
using DteamBackend.Models;
using DteamBackend.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

using DteamBackend.Services;
using DteamBackend.Models.DTO;

namespace DteamBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IAccountService _accountService;

        public UserController(AppDbContext context, IAccountService accountService)
        {
            _context = context;
            _accountService = accountService;
        }

        public class UpdateProfileRequest
        {
            [MaxLength(1000, ErrorMessage = "Біографія не може перевищувати 1000 символів.")]
            public string? Bio { get; set; }

            [MaxLength(500, ErrorMessage = "URL аватарки не може перевищувати 500 символів.")]
            public string? AvatarUrl { get; set; }

            [MaxLength(500, ErrorMessage = "URL банера не може перевищувати 500 символів.")]
            public string? BannerUrl { get; set; }
        }

        [HttpPut("me")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateProfileRequest dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                           ?? User.FindFirst("sub")?.Value;

            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { message = "Користувач не авторизований." });
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return NotFound(new { message = "Користувача не знайдено." });
            }

            if (dto.Bio != null)
            {
                user.Bio = dto.Bio.Trim().Length > 0 ? dto.Bio.Trim() : null;
            }
            if (!string.IsNullOrWhiteSpace(dto.AvatarUrl))
            {
                user.AvatarUrl = dto.AvatarUrl.Trim();
            }
            if (dto.BannerUrl != null)
            {
                user.BannerUrl = dto.BannerUrl.Trim().Length > 0 ? dto.BannerUrl.Trim() : null;
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                id = user.Id,
                username = user.Username,
                avatarUrl = user.AvatarUrl,
                bio = user.Bio,
                bannerUrl = user.BannerUrl
            });
        }

        [HttpGet("me/reviews")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetMyReviews()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                           ?? User.FindFirst("sub")?.Value;

            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { message = "Користувач не авторизований." });
            }

            var reviews = await _context.Reviews
                .Include(r => r.Game)
                .AsNoTracking()
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new
                {
                    id = r.Id,
                    gameId = r.GameId,
                    gameTitle = r.Game.Title,
                    gameCoverImageUrl = r.Game.CoverImageUrl,
                    rating = r.Rating,
                    content = r.Content,
                    isRecommended = r.IsRecommended,
                    playTimeHoursAtReview = r.PlayTimeHoursAtReview,
                    createdAt = r.CreatedAt,
                    updatedAt = r.UpdatedAt
                })
                .ToListAsync();

            return Ok(reviews);
        }

        [HttpGet("is-banned")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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

        [HttpGet("{userId:guid}/profile")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPublicProfile(Guid userId)
        {
            var user = await _context.Users
                .AsNoTracking()
                .Where(u => u.Id == userId)
                .Select(u => new
                {
                    u.Id,
                    u.Username,
                    u.AvatarUrl,
                    u.BannerUrl,
                    u.Bio,
                    u.Status,
                    u.IsInFamily,
                    u.IsAdmin,
                    u.CreatedAt,
                    u.HardhatAddress,
                    u.WalletAddress
                })
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound(new { message = $"Користувача з ID '{userId}' не знайдено." });
            }

            var friendsCount = await _context.Users
                .Where(u => u.Id == userId)
                .Select(u => u.Friends.Count)
                .FirstOrDefaultAsync();

            var publishedGames = await _context.Games
                .AsNoTracking()
                .Where(g => g.OwnerId == userId && g.IsPublished)
                .OrderByDescending(g => g.CreatedAt)
                .Select(g => new
                {
                    id = g.Id,
                    title = g.Title,
                    coverImageUrl = g.CoverImageUrl ?? g.HeaderImageUrl ?? "",
                    priceInNanoTons = g.PriceInNanoTons,
                    discountPercentage = g.DiscountPercentage
                })
                .ToListAsync();

            var libraryGames = await _context.UserGames
                .Include(ug => ug.Game)
                .AsNoTracking()
                .Where(ug => ug.UserId == userId && ug.Game != null)
                .OrderByDescending(ug => ug.PurchasedAt)
                .Select(ug => new
                {
                    id = ug.Game.Id,
                    title = ug.Game.Title,
                    coverImageUrl = ug.Game.CoverImageUrl ?? ug.Game.HeaderImageUrl ?? "",
                    priceInNanoTons = ug.Game.PriceInNanoTons,
                    discountPercentage = ug.Game.DiscountPercentage,
                    isDlc = ug.Game.IsDlc
                })
                .ToListAsync();

            var gamesCount = libraryGames.Count + publishedGames.Count;

            Guid? viewerId = null;
            var viewerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                             ?? User.FindFirst("sub")?.Value;
            if (Guid.TryParse(viewerIdClaim, out var parsedViewerId))
            {
                viewerId = parsedViewerId;
            }

            string friendshipStatus = "none";
            bool isIncomingRequest = false;
            if (viewerId.HasValue && viewerId.Value != userId)
            {
                var areFriends = await _context.UserFriends
                    .AnyAsync(uf => (uf.UserId == viewerId.Value && uf.FriendId == userId) ||
                                   (uf.UserId == userId && uf.FriendId == viewerId.Value));

                if (areFriends)
                {
                    friendshipStatus = "friends";
                }
                else
                {
                    var pendingReq = await _context.FriendRequests
                        .FirstOrDefaultAsync(fr =>
                            ((fr.SenderId == viewerId.Value && fr.ReceiverId == userId) ||
                             (fr.SenderId == userId && fr.ReceiverId == viewerId.Value)) &&
                            fr.Status == FriendRequestStatus.Pending);

                    if (pendingReq != null)
                    {
                        friendshipStatus = "pending";
                        isIncomingRequest = pendingReq.ReceiverId == viewerId.Value;
                    }
                }
            }

            return Ok(new
            {
                id = user.Id,
                username = user.Username,
                avatarUrl = user.AvatarUrl,
                bannerUrl = user.BannerUrl,
                bio = user.Bio,
                status = (int)user.Status,
                isInFamily = user.IsInFamily,
                isAdmin = user.IsAdmin,
                createdAt = user.CreatedAt,
                friendsCount,
                gamesCount,
                publishedGames,
                libraryGames,
                isOwnProfile = viewerId.HasValue && viewerId.Value == userId,
                friendshipStatus,
                isIncomingRequest,
                hardhatAddress = user.HardhatAddress,
                walletAddress = user.WalletAddress
            });
        }

        [HttpGet("{userId:guid}/friends")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPublicFriends(Guid userId)
        {
            var friends = await _context.Users
                .AsNoTracking()
                .Where(u => u.Id == userId)
                .SelectMany(u => u.Friends)
                .Select(f => new
                {
                    id = f.Id,
                    username = f.Username,
                    avatarUrl = f.AvatarUrl,
                    status = (int)f.Status
                })
                .ToListAsync();

            return Ok(friends);
        }

        [HttpGet("library")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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
                .ToListAsync();

            var result = userGames.Select(ug => new
            {
                userId = ug.UserId,
                gameId = ug.GameId,
                purchasedAt = ug.PurchasedAt,
                playTimeMinutes = ug.PlayTimeMinutes,
                lastPlayedAt = ug.LastPlayedAt,
                isFavorite = ug.IsFavorite,
                game = MapToGameDto(ug.Game)
            });

            return Ok(result);
        }

        [HttpPost("library/{gameId}/favorite")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ToggleFavorite(Guid gameId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                           ?? User.FindFirst("sub")?.Value;

            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { message = "Користувач не авторизований." });
            }

            var userGame = await _context.UserGames
                .FirstOrDefaultAsync(ug => ug.UserId == userId && ug.GameId == gameId);

            if (userGame == null)
            {
                return NotFound(new { message = "Гру не знайдено у бібліотеці користувача." });
            }

            userGame.IsFavorite = !userGame.IsFavorite;
            await _context.SaveChangesAsync();

            return Ok(new { isFavorite = userGame.IsFavorite });
        }

        private static object MapToGameDto(Game game) => new
        {
            id = game.Id,
            title = game.Title,
            description = game.Description,
            shortDescription = game.ShortDescription,
            priceInNanoTons = game.PriceInNanoTons,
            discountPercentage = game.DiscountPercentage,
            ownerId = game.OwnerId,
            ownerUsername = game.Owner?.Username,
            downloadCount = game.DownloadCount,
            averageRating = game.AverageRating,
            reviewsCount = game.ReviewsCount,
            isDlc = game.IsDlc,
            parentGameId = game.ParentGameId,
            genres = game.Genres ?? new List<string>(),
            platforms = game.Platforms ?? new List<string>(),
            features = game.Features ?? new List<string>(),
            tags = game.Tags ?? new List<string>(),
            version = game.Version,
            sizeInBytes = game.SizeInBytes,
            isPublished = game.IsPublished,
            headerImageUrl = game.HeaderImageUrl,
            coverImageUrl = game.CoverImageUrl,
            screenshotUrls = game.ScreenshotUrls ?? new List<string>(),
            trailerUrl = game.TrailerUrl,
            createdAt = game.CreatedAt,
            updatedAt = game.UpdatedAt
        };

        [HttpGet("settings")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUserSettings()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                           ?? User.FindFirst("sub")?.Value;

            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { message = "Користувач не авторизований." });
            }

            var user = await _context.Users
                .Include(u => u.NotificationPreferences)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return NotFound(new { message = "Користувача не знайдено." });
            }

            var prefs = user.NotificationPreferences;
            var response = new SettingsResponseDto
            {
                Profile = new UserProfileSettingsDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    Bio = user.Bio,
                    AvatarUrl = user.AvatarUrl,
                    BannerUrl = user.BannerUrl,
                    PreferredLanguage = user.PreferredLanguage ?? "uk",
                    CreatedAt = user.CreatedAt
                },
                Preferences = new NotificationPreferencesDto
                {
                    NotifyBigSales = prefs?.NotifyBigSales ?? true,
                    NotifyWishlistDiscounts = prefs?.NotifyWishlistDiscounts ?? true,
                    NotifyProfileComments = prefs?.NotifyProfileComments ?? true,
                    NotifyFriendRequests = prefs?.NotifyFriendRequests ?? true,
                    NotifyFriendRequestAccepted = prefs?.NotifyFriendRequestAccepted ?? true,
                    NotifyFriendRequestDeclined = prefs?.NotifyFriendRequestDeclined ?? true,
                    ChatNotificationsEnabled = prefs?.ChatNotificationsEnabled ?? true,
                    ChatSoundEnabled = prefs?.ChatSoundEnabled ?? true
                },
                WalletSummary = new WalletSummaryDto
                {
                    BalanceInNanoTons = user.BalanceInNanoTons,
                    FormattedBalance = user.BalanceInNanoTons / 1_000_000_000m,
                    Currency = "TON"
                }
            };

            return Ok(response);
        }

        [HttpPut("settings/general")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateGeneralSettings([FromBody] UpdateGeneralSettingsDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                           ?? User.FindFirst("sub")?.Value;

            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { message = "Користувач не авторизований." });
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return NotFound(new { message = "Користувача не знайдено." });
            }

            var cleanUsername = dto.Username.Trim();
            var cleanEmail = dto.Email.Trim().ToLowerInvariant();

            // Check username uniqueness (excluding current user)
            var usernameTaken = await _context.Users
                .AnyAsync(u => u.Id != userId && u.Username.ToLower() == cleanUsername.ToLower());
            if (usernameTaken)
            {
                return BadRequest(new { message = "Користувач з таким нікнеймом вже існує." });
            }

            // Check email uniqueness (excluding current user)
            var emailTaken = await _context.Users
                .AnyAsync(u => u.Id != userId && u.Email.ToLower() == cleanEmail);
            if (emailTaken)
            {
                return BadRequest(new { message = "Користувач з такою електронною поштою вже існує." });
            }

            user.Username = cleanUsername;
            user.Email = cleanEmail;
            user.Bio = string.IsNullOrWhiteSpace(dto.Bio) ? null : dto.Bio.Trim();
            user.AvatarUrl = string.IsNullOrWhiteSpace(dto.AvatarUrl) ? null : dto.AvatarUrl.Trim();
            user.BannerUrl = string.IsNullOrWhiteSpace(dto.BannerUrl) ? null : dto.BannerUrl.Trim();
            if (!string.IsNullOrWhiteSpace(dto.PreferredLanguage))
            {
                user.PreferredLanguage = dto.PreferredLanguage.Trim();
            }
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new UserProfileSettingsDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Bio = user.Bio,
                AvatarUrl = user.AvatarUrl,
                BannerUrl = user.BannerUrl,
                PreferredLanguage = user.PreferredLanguage,
                CreatedAt = user.CreatedAt
            });
        }

        [HttpPut("settings/notifications")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateNotificationSettings([FromBody] NotificationPreferencesDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                           ?? User.FindFirst("sub")?.Value;

            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { message = "Користувач не авторизований." });
            }

            var prefs = await _context.UserNotificationPreferences.FirstOrDefaultAsync(p => p.UserId == userId);
            if (prefs == null)
            {
                prefs = new UserNotificationPreferences
                {
                    UserId = userId
                };
                await _context.UserNotificationPreferences.AddAsync(prefs);
            }

            prefs.NotifyBigSales = dto.NotifyBigSales;
            prefs.NotifyWishlistDiscounts = dto.NotifyWishlistDiscounts;
            prefs.NotifyProfileComments = dto.NotifyProfileComments;
            prefs.NotifyFriendRequests = dto.NotifyFriendRequests;
            prefs.NotifyFriendRequestAccepted = dto.NotifyFriendRequestAccepted;
            prefs.NotifyFriendRequestDeclined = dto.NotifyFriendRequestDeclined;
            prefs.ChatNotificationsEnabled = dto.ChatNotificationsEnabled;
            prefs.ChatSoundEnabled = dto.ChatSoundEnabled;
            prefs.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(dto);
        }

        [HttpDelete("me")]
        [HttpPost("delete-account")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeleteAccount([FromBody] DeleteAccountDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                           ?? User.FindFirst("sub")?.Value;

            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { message = "Користувач не авторизований." });
            }

            var (success, error) = await _accountService.DeleteAccountAsync(userId, dto);
            if (!success)
            {
                return BadRequest(new { message = error });
            }

            return Ok(new { message = "Ваш акаунт було успішно видалено." });
        }
    }
}
