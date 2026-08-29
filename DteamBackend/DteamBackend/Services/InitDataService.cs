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

            var mainGameId = Guid.NewGuid();
            var mainGame = new Game
            {
                Id = mainGameId,
                Title = "Cyberpunk 2077",
                Description = "Cyberpunk 2077 — пригодницький бойовик і рольова гра з відкритим світом. Дія відбувається у темному майбутньому Найт-Сіті, небезпечного мегаполіса, одержимого владою, гламуром і ненаситною модифікацією тіла. Грайте за найманця V у пошуках унікального імплантату безсмертя.",
                ShortDescription = "Пригодницький рольовий екшн у відкритому світі майбутнього з глибоким сюжетом.",
                PriceInNanoTons = 5_000_000_000,
                DiscountPercentage = 20,
                ServerArchivePath = "/storage/games/cyberpunk-2077.zip",
                OwnerId = adminUser.Id,
                DownloadCount = 14200,
                AverageRating = 4.9,
                ReviewsCount = 4,
                IsDlc = false,
                ParentGameId = null,
                Genres = new List<string> { "Action", "RPG", "Cyberpunk", "Open World" },
                Platforms = new List<string> { "Windows", "MacOS" },
                Features = new List<string> { "SinglePlayer", "CloudSaves", "Achievements" },
                Tags = new List<string> { "шутер", "екшн", "кіберпанк", "відкритий світ", "майбутнє", "рольова гра" },
                Version = "2.1.0",
                SizeInBytes = 70L * 1024 * 1024 * 1024,
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
            };

            var dlc1 = new Game
            {
                Id = Guid.NewGuid(),
                Title = "Cyberpunk 2077: Bonus Content",
                Description = "Офіційний додатковий контент та шпалери для Cyberpunk 2077.",
                ShortDescription = "Офіційний бонусний контент.",
                PriceInNanoTons = 0,
                DiscountPercentage = 0,
                ServerArchivePath = "/storage/games/cyberpunk-bonus.zip",
                OwnerId = adminUser.Id,
                DownloadCount = 5000,
                AverageRating = 5.0,
                ReviewsCount = 1,
                IsDlc = true,
                ParentGameId = mainGameId,
                Genres = new List<string> { "DLC" },
                Platforms = new List<string> { "Windows" },
                IsPublished = true,
                CreatedAt = DateTime.UtcNow
            };

            var dlc2 = new Game
            {
                Id = Guid.NewGuid(),
                Title = "Cyberpunk 2077: REDmod",
                Description = "Безкоштовний інструмент для створення та завантаження модифікацій.",
                ShortDescription = "Інструмент модифікацій.",
                PriceInNanoTons = 0,
                DiscountPercentage = 0,
                ServerArchivePath = "/storage/games/cyberpunk-redmod.zip",
                OwnerId = adminUser.Id,
                DownloadCount = 8000,
                AverageRating = 5.0,
                ReviewsCount = 1,
                IsDlc = true,
                ParentGameId = mainGameId,
                Genres = new List<string> { "DLC", "Modding" },
                Platforms = new List<string> { "Windows" },
                IsPublished = true,
                CreatedAt = DateTime.UtcNow
            };

            var dlc3 = new Game
            {
                Id = Guid.NewGuid(),
                Title = "Cyberpunk 2077: Phantom Liberty",
                Description = "Масштабне сюжетне доповнення у жанрі шпигунського трилера до Cyberpunk 2077.",
                ShortDescription = "Шпигунський трилер у Найт-Сіті.",
                PriceInNanoTons = 2_500_000_000,
                DiscountPercentage = 0,
                ServerArchivePath = "/storage/games/cyberpunk-phantom-liberty.zip",
                OwnerId = adminUser.Id,
                DownloadCount = 11000,
                AverageRating = 5.0,
                ReviewsCount = 2,
                IsDlc = true,
                ParentGameId = mainGameId,
                Genres = new List<string> { "Action", "RPG", "DLC" },
                Platforms = new List<string> { "Windows", "MacOS" },
                IsPublished = true,
                CreatedAt = DateTime.UtcNow
            };

            await context.Games.AddRangeAsync(new[] { mainGame, dlc1, dlc2, dlc3 });

            var sampleReview = new Review
            {
                Id = Guid.NewGuid(),
                UserId = adminUser.Id,
                GameId = mainGameId,
                Rating = 5,
                Content = "Чудова гра! Неймовірна атмосфера, графіка та сюжет тримають у напрузі до останньої хвилини. Рекомендую всім!",
                IsRecommended = true,
                PlayTimeHoursAtReview = 48.5,
                CreatedAt = DateTime.UtcNow
            };

            await context.Reviews.AddAsync(sampleReview);

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

