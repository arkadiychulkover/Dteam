using System.Security.Claims;
using DteamBackend.Data;
using DteamBackend.Models;
using DteamBackend.Models.DTO;
using DteamBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DteamBackend.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<AdminController> _logger;

        public AdminController(AppDbContext context, ILogger<AdminController> logger)
        {
            _context = context;
            _logger = logger;
        }

        private static UserDto MapToUserDto(Duser user) => new()
        {
            Id = user.Id,
            Email = user.Email,
            Username = user.Username,
            WalletAddress = user.WalletAddress,
            BalanceInNanoTons = user.BalanceInNanoTons,
            TotalEarningsInNanoTons = user.TotalEarningsInNanoTons,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            LastLoginAt = user.LastLoginAt,
            IsInFamily = user.IsInFamily,
            FamilyOwnerId = user.FamilyOwnerId,
            IsAdmin = user.IsAdmin,
            IsBanned = user.IsBanned,
            Status = user.Status,
            AvatarUrl = user.AvatarUrl,
            Bio = user.Bio
        };

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

        [HttpGet("users")]
        [ProducesResponseType(typeof(IEnumerable<UserDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsers()
        {
            var users = await _context.Users
                .AsNoTracking()
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();

            return Ok(users.Select(MapToUserDto));
        }

        [HttpGet("users/{id:guid}")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserDto>> GetUserById(Guid id)
        {
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound(new { message = $"Пользователь с ID '{id}' не найден" });
            }

            return Ok(MapToUserDto(user));
        }

        [HttpPost("users")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<UserDto>> CreateUser([FromBody] CreateUserDto dto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
            {
                return BadRequest(new { message = $"Пользователь с email '{dto.Email}' уже существует" });
            }

            if (await _context.Users.AnyAsync(u => u.Username == dto.Username))
            {
                return BadRequest(new { message = $"Пользователь с username '{dto.Username}' уже существует" });
            }

            PasswordHasher.CreatePasswordHash(dto.Password, out string hash, out string salt);

            var user = new Duser
            {
                Id = Guid.NewGuid(),
                Email = dto.Email.Trim().ToLowerInvariant(),
                Username = dto.Username.Trim(),
                PasswordHash = hash,
                PasswordSalt = salt,
                WalletAddress = dto.WalletAddress,
                BalanceInNanoTons = dto.BalanceInNanoTons,
                TotalEarningsInNanoTons = 0,
                CreatedAt = DateTime.UtcNow,
                IsAdmin = dto.IsAdmin,
                IsBanned = false,
                Status = dto.Status,
                AvatarUrl = dto.AvatarUrl,
                Bio = dto.Bio
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, MapToUserDto(user));
        }

        [HttpPut("users/{id:guid}")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserDto>> UpdateUser(Guid id, [FromBody] UpdateUserDto dto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound(new { message = $"Пользователь с ID '{id}' не найден" });
            }

            if (!string.IsNullOrWhiteSpace(dto.Email) && dto.Email != user.Email)
            {
                if (await _context.Users.AnyAsync(u => u.Email == dto.Email && u.Id != id))
                {
                    return BadRequest(new { message = $"Пользователь с email '{dto.Email}' уже существует" });
                }
                user.Email = dto.Email.Trim().ToLowerInvariant();
            }

            if (!string.IsNullOrWhiteSpace(dto.Username) && dto.Username != user.Username)
            {
                if (await _context.Users.AnyAsync(u => u.Username == dto.Username && u.Id != id))
                {
                    return BadRequest(new { message = $"Пользователь с username '{dto.Username}' уже существует" });
                }
                user.Username = dto.Username.Trim();
            }

            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                PasswordHasher.CreatePasswordHash(dto.Password, out string hash, out string salt);
                user.PasswordHash = hash;
                user.PasswordSalt = salt;
            }

            if (dto.WalletAddress != null) user.WalletAddress = dto.WalletAddress;
            if (dto.IsAdmin.HasValue) user.IsAdmin = dto.IsAdmin.Value;
            if (dto.IsBanned.HasValue) user.IsBanned = dto.IsBanned.Value;
            if (dto.Status.HasValue) user.Status = dto.Status.Value;
            if (dto.BalanceInNanoTons.HasValue) user.BalanceInNanoTons = dto.BalanceInNanoTons.Value;
            if (dto.AvatarUrl != null) user.AvatarUrl = dto.AvatarUrl;
            if (dto.Bio != null) user.Bio = dto.Bio;

            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(MapToUserDto(user));
        }

        [HttpDelete("users/{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound(new { message = $"Пользователь с ID '{id}' не найден" });
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Пользователь '{user.Username}' успешно удален" });
        }

        [HttpGet("games")]
        [ProducesResponseType(typeof(IEnumerable<GameDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<GameDto>>> GetAllGames()
        {
            var games = await _context.Games
                .Include(g => g.Owner)
                .Include(g => g.ParentGame)
                .Include(g => g.Dlcs)
                .AsNoTracking()
                .OrderByDescending(g => g.CreatedAt)
                .ToListAsync();

            return Ok(games.Select(MapToGameDto));
        }

        [HttpGet("games/{id:guid}")]
        [ProducesResponseType(typeof(GameDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GameDto>> GetGameById(Guid id)
        {
            var game = await _context.Games
                .Include(g => g.Owner)
                .Include(g => g.ParentGame)
                .Include(g => g.Dlcs)
                .AsNoTracking()
                .FirstOrDefaultAsync(g => g.Id == id);

            if (game == null)
            {
                return NotFound(new { message = $"Игра с ID '{id}' не найдена" });
            }

            return Ok(MapToGameDto(game));
        }

        [HttpPost("games")]
        [ProducesResponseType(typeof(GameDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<GameDto>> CreateGame([FromBody] CreateGameDto dto)
        {
            var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                                  ?? User.FindFirst("sub")?.Value;
            Guid.TryParse(currentUserIdClaim, out var currentAdminId);

            Guid ownerId = dto.OwnerId ?? currentAdminId;

            var ownerExists = await _context.Users.AnyAsync(u => u.Id == ownerId);
            if (!ownerExists)
            {
                return BadRequest(new { message = $"Владелец игры с ID '{ownerId}' не найден в базе данных" });
            }

            var game = new Game
            {
                Id = Guid.NewGuid(),
                Title = dto.Title.Trim(),
                Description = dto.Description,
                ShortDescription = dto.ShortDescription,
                PriceInNanoTons = dto.PriceInNanoTons,
                DiscountPercentage = dto.DiscountPercentage,
                ServerArchivePath = dto.ServerArchivePath,
                OwnerId = ownerId,
                DownloadCount = 0,
                AverageRating = 0.0,
                ReviewsCount = 0,
                IsDlc = dto.IsDlc,
                ParentGameId = dto.ParentGameId,
                Genres = dto.Genres ?? new List<string>(),
                Platforms = dto.Platforms ?? new List<string> { "Windows" },
                Features = dto.Features ?? new List<string>(),
                Tags = dto.Tags ?? new List<string>(),
                Version = string.IsNullOrWhiteSpace(dto.Version) ? "1.0.0" : dto.Version,
                SizeInBytes = dto.SizeInBytes,
                IsPublished = dto.IsPublished,
                HeaderImageUrl = dto.HeaderImageUrl,
                CoverImageUrl = dto.CoverImageUrl,
                ScreenshotUrls = dto.ScreenshotUrls ?? new List<string>(),
                TrailerUrl = dto.TrailerUrl,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Games.AddAsync(game);
            await _context.SaveChangesAsync();

            await _context.Entry(game).Reference(g => g.Owner).LoadAsync();
            if (game.ParentGameId.HasValue)
            {
                await _context.Entry(game).Reference(g => g.ParentGame).LoadAsync();
            }

            return CreatedAtAction(nameof(GetGameById), new { id = game.Id }, MapToGameDto(game));
        }

        [HttpPut("games/{id:guid}")]
        [ProducesResponseType(typeof(GameDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GameDto>> UpdateGame(Guid id, [FromBody] UpdateGameDto dto)
        {
            var game = await _context.Games
                .Include(g => g.Owner)
                .Include(g => g.ParentGame)
                .Include(g => g.Dlcs)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (game == null)
            {
                return NotFound(new { message = $"Игра с ID '{id}' не найдена" });
            }

            if (!string.IsNullOrWhiteSpace(dto.Title)) game.Title = dto.Title.Trim();
            if (dto.Description != null) game.Description = dto.Description;
            if (dto.ShortDescription != null) game.ShortDescription = dto.ShortDescription;
            if (dto.PriceInNanoTons.HasValue) game.PriceInNanoTons = dto.PriceInNanoTons.Value;
            if (dto.DiscountPercentage.HasValue) game.DiscountPercentage = dto.DiscountPercentage.Value;
            if (!string.IsNullOrWhiteSpace(dto.ServerArchivePath)) game.ServerArchivePath = dto.ServerArchivePath;
            if (dto.OwnerId.HasValue)
            {
                var ownerExists = await _context.Users.AnyAsync(u => u.Id == dto.OwnerId.Value);
                if (!ownerExists)
                {
                    return BadRequest(new { message = $"Владелец игры с ID '{dto.OwnerId.Value}' не найден" });
                }
                game.OwnerId = dto.OwnerId.Value;
            }
            if (dto.IsDlc.HasValue) game.IsDlc = dto.IsDlc.Value;
            if (dto.ParentGameId.HasValue) game.ParentGameId = dto.ParentGameId.Value == Guid.Empty ? null : dto.ParentGameId.Value;
            if (dto.Genres != null) game.Genres = dto.Genres;
            if (dto.Platforms != null) game.Platforms = dto.Platforms;
            if (dto.Features != null) game.Features = dto.Features;
            if (dto.Tags != null) game.Tags = dto.Tags;
            if (!string.IsNullOrWhiteSpace(dto.Version)) game.Version = dto.Version;
            if (dto.SizeInBytes.HasValue) game.SizeInBytes = dto.SizeInBytes.Value;
            if (dto.IsPublished.HasValue) game.IsPublished = dto.IsPublished.Value;
            if (dto.HeaderImageUrl != null) game.HeaderImageUrl = dto.HeaderImageUrl;
            if (dto.CoverImageUrl != null) game.CoverImageUrl = dto.CoverImageUrl;
            if (dto.ScreenshotUrls != null) game.ScreenshotUrls = dto.ScreenshotUrls;
            if (dto.TrailerUrl != null) game.TrailerUrl = dto.TrailerUrl;

            game.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(MapToGameDto(game));
        }

        [HttpDelete("games/{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteGame(Guid id)
        {
            var game = await _context.Games.FindAsync(id);
            if (game == null)
            {
                return NotFound(new { message = $"Игра с ID '{id}' не найдена" });
            }

            _context.Games.Remove(game);
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Игра '{game.Title}' успешно удалена" });
        }
    }
}

