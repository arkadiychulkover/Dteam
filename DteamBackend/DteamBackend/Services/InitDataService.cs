using DteamBackend.Data;
using DteamBackend.Interfaces;
using DteamBackend.Models;
using DteamBackend.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace DteamBackend.Services
{
    public class InitDataService : IInitDataService
    {
        private readonly AppDbContext? _context;
        private readonly ILogger<InitDataService>? _logger;

        public InitDataService()
        {
        }

        public InitDataService(AppDbContext context, ILogger<InitDataService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task InitializeAsync(AppDbContext context)
        {
            if (await context.Users.AnyAsync())
            {
                return;
            }

            PasswordHasher.CreatePasswordHash("admin123321", out string passwordHash, out string passwordSalt);

            var adminUser = new Duser
            {
                Id = Guid.NewGuid(),
                Email = "adim@gmail.com",
                Username = "adim",
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                WalletAddress = "EQB_v1zX3L1f2M9zX_SampleAdminTonWalletAddress_777",
                BalanceInNanoTons = 100_000_000_000,
                TotalEarningsInNanoTons = 0,
                CreatedAt = DateTime.UtcNow,
                IsAdmin = true,
                IsBanned = false,
                Status = UserStatus.Online,
                AvatarUrl = "https://images.unsplash.com/photo-1534528741775-53994a69daeb?w=500&auto=format&fit=crop&q=60",
                Bio = "Dteam System Administrator"
            };

            await context.Users.AddAsync(adminUser);

            var games = new List<Game>
            {
                new Game
                {
                    Id = Guid.NewGuid(),
                    Title = "Cyberpunk Odyssey 2077",
                    Description = "A next-generation sci-fi RPG set in a dystopian futuristic metropolis. Experience cutting-edge graphics, deep narrative, high-stakes tactical combat, and seamless decentralized trading.",
                    ShortDescription = "High-octane cyberpunk action RPG with next-gen raytracing and Web3 trading.",
                    PriceInNanoTons = 5_000_000_000,
                    DiscountPercentage = 40,
                    ServerArchivePath = "/storage/games/cyberpunk-odyssey-v1.0.0.zip",
                    OwnerId = adminUser.Id,
                    DownloadCount = 14200,
                    AverageRating = 4.8,
                    ReviewsCount = 389,
                    IsDlc = false,
                    ParentGameId = null,
                    Genres = new List<string> { "Action", "RPG", "Cyberpunk", "SciFi" },
                    Platforms = new List<string> { "Windows", "MacOS", "Linux" },
                    Features = new List<string> { "SinglePlayer", "CloudSaves", "Achievements" },
                    Tags = new List<string> { "Open World", "Story Rich", "Ray Tracing", "Cyberpunk" },
                    Version = "1.0.0",
                    SizeInBytes = 25L * 1024 * 1024 * 1024,
                    IsPublished = true,
                    HeaderImageUrl = "https://images.unsplash.com/photo-1542751371-adc38448a05e?w=1200&auto=format&fit=crop&q=80",
                    CoverImageUrl = "https://images.unsplash.com/photo-1618005182384-a83a8bd57fbe?w=800&auto=format&fit=crop&q=80",
                    ScreenshotUrls = new List<string>
                    {
                        "https://images.unsplash.com/photo-1550745165-9bc0b252726f?w=800&auto=format&fit=crop&q=80",
                        "https://images.unsplash.com/photo-1511512578047-dfb367046420?w=800&auto=format&fit=crop&q=80"
                    },
                    TrailerUrl = "https://www.youtube.com/watch?v=dQw4w9WgXcQ",
                    CreatedAt = DateTime.UtcNow
                }
            };

            await context.Games.AddRangeAsync(games);
            await context.SaveChangesAsync();
        }

        public async Task InitializeAsync()
        {
            if (_context != null)
            {
                await InitializeAsync(_context);
            }
        }
    }
}
