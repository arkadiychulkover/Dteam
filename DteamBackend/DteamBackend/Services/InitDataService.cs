using DteamBackend.Data;
using DteamBackend.Interfaces;
using DteamBackend.Models;
using DteamBackend.Models.Enums;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Text.Json;

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

        private class CommunitySeedJsonStore
        {
            public List<CommunityPost> Posts { get; set; } = new();
            public List<CommunityComment> Comments { get; set; } = new();
        }

        public async Task EnsureCommunityDataAsync(AppDbContext context)
        {
            try
            {
                await context.Database.ExecuteSqlRawAsync(@"
                    CREATE TABLE IF NOT EXISTS ""CommunityPosts"" (
                        ""Id"" TEXT NOT NULL CONSTRAINT ""PK_CommunityPosts"" PRIMARY KEY,
                        ""GameId"" TEXT NOT NULL,
                        ""GameTitle"" TEXT NULL,
                        ""GameBannerUrl"" TEXT NULL,
                        ""GameGuidId"" TEXT NULL,
                        ""AuthorId"" TEXT NOT NULL DEFAULT '',
                        ""AuthorUsername"" TEXT NOT NULL DEFAULT '',
                        ""AuthorAvatarUrl"" TEXT NOT NULL DEFAULT '',
                        ""CreatedAt"" TEXT NOT NULL,
                        ""Category"" TEXT NOT NULL DEFAULT 'forum',
                        ""Title"" TEXT NOT NULL DEFAULT '',
                        ""Content"" TEXT NOT NULL DEFAULT '',
                        ""MediaType"" TEXT NOT NULL DEFAULT 'none',
                        ""MediaUrl"" TEXT NOT NULL DEFAULT '',
                        ""MediaThumbnailUrl"" TEXT NOT NULL DEFAULT '',
                        ""LikedByUsers"" TEXT NOT NULL DEFAULT '[]'
                    );
                    CREATE INDEX IF NOT EXISTS ""IX_CommunityPosts_GameId"" ON ""CommunityPosts"" (""GameId"");
                    CREATE INDEX IF NOT EXISTS ""IX_CommunityPosts_Category"" ON ""CommunityPosts"" (""Category"");
                    CREATE INDEX IF NOT EXISTS ""IX_CommunityPosts_CreatedAt"" ON ""CommunityPosts"" (""CreatedAt"");

                    CREATE TABLE IF NOT EXISTS ""CommunityComments"" (
                        ""Id"" TEXT NOT NULL CONSTRAINT ""PK_CommunityComments"" PRIMARY KEY,
                        ""PostId"" TEXT NOT NULL,
                        ""ParentCommentId"" TEXT NULL,
                        ""AuthorId"" TEXT NOT NULL DEFAULT '',
                        ""AuthorUsername"" TEXT NOT NULL DEFAULT '',
                        ""AuthorAvatarUrl"" TEXT NOT NULL DEFAULT '',
                        ""CreatedAt"" TEXT NOT NULL,
                        ""Content"" TEXT NOT NULL DEFAULT '',
                        ""LikesCount"" INTEGER NOT NULL DEFAULT 0,
                        ""LikedByUsers"" TEXT NOT NULL DEFAULT '[]'
                    );
                    CREATE INDEX IF NOT EXISTS ""IX_CommunityComments_PostId"" ON ""CommunityComments"" (""PostId"");
                ");

                if (!await context.CommunityPosts.AnyAsync())
                {
                    var searchPaths = new[]
                    {
                        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "community_data.json"),
                        Path.Combine(Directory.GetCurrentDirectory(), "community_data.json")
                    };
                    var jsonPath = searchPaths.FirstOrDefault(File.Exists);
                    if (jsonPath != null)
                    {
                        var json = await File.ReadAllTextAsync(jsonPath);
                        var data = JsonSerializer.Deserialize<CommunitySeedJsonStore>(json, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                        if (data != null)
                        {
                            if (data.Posts != null && data.Posts.Count > 0)
                            {
                                foreach (var p in data.Posts)
                                {
                                    if (Guid.TryParse(p.GameId, out var gId))
                                    {
                                        p.GameGuidId = gId;
                                    }
                                    if (p.Author == null) p.Author = new AuthorDto();
                                    if (p.Media == null) p.Media = new PostMedia();
                                    if (p.LikedByUsers == null) p.LikedByUsers = new List<string>();
                                }
                                await context.CommunityPosts.AddRangeAsync(data.Posts);
                            }

                            if (data.Comments != null && data.Comments.Count > 0)
                            {
                                var flattened = new List<CommunityComment>();
                                foreach (var c in data.Comments)
                                {
                                    if (c.Author == null) c.Author = new AuthorDto();
                                    if (c.LikedByUsers == null) c.LikedByUsers = new List<string>();
                                    flattened.Add(c);

                                    if (c.Replies != null && c.Replies.Count > 0)
                                    {
                                        foreach (var r in c.Replies)
                                        {
                                            r.ParentCommentId = c.Id;
                                            if (r.Author == null) r.Author = new AuthorDto();
                                            if (r.LikedByUsers == null) r.LikedByUsers = new List<string>();
                                            flattened.Add(r);
                                        }
                                    }
                                }
                                await context.CommunityComments.AddRangeAsync(flattened);
                            }

                            await context.SaveChangesAsync();
                            _logger?.LogInformation($"[InitData] Migrated {data.Posts?.Count ?? 0} community posts and {data.Comments?.Count ?? 0} comments into database.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "[InitData] Error ensuring community data in SQLite database.");
            }
        }

        public async Task EnsureReviewSchemaAsync(AppDbContext context)
        {
            try
            {
                // Check existing columns in Reviews table
                var connection = context.Database.GetDbConnection();
                await connection.OpenAsync();

                using var cmd = connection.CreateCommand();
                cmd.CommandText = "PRAGMA table_info('Reviews');";
                var columns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var colName = reader["name"]?.ToString();
                        if (!string.IsNullOrEmpty(colName))
                        {
                            columns.Add(colName);
                        }
                    }
                }

                if (!columns.Contains("ParentReviewId"))
                {
                    await context.Database.ExecuteSqlRawAsync("ALTER TABLE \"Reviews\" ADD COLUMN \"ParentReviewId\" TEXT NULL;");
                    _logger?.LogInformation("[InitData] Added ParentReviewId column to Reviews table.");
                }

                if (!columns.Contains("LikesCount"))
                {
                    await context.Database.ExecuteSqlRawAsync("ALTER TABLE \"Reviews\" ADD COLUMN \"LikesCount\" INTEGER NOT NULL DEFAULT 0;");
                    _logger?.LogInformation("[InitData] Added LikesCount column to Reviews table.");
                }

                if (!columns.Contains("LikedByUsers"))
                {
                    await context.Database.ExecuteSqlRawAsync("ALTER TABLE \"Reviews\" ADD COLUMN \"LikedByUsers\" TEXT NOT NULL DEFAULT '[]';");
                    _logger?.LogInformation("[InitData] Added LikedByUsers column to Reviews table.");
                }

                // Update index on Reviews table to allow multiple comments by same user on game
                await context.Database.ExecuteSqlRawAsync(@"
                    DROP INDEX IF EXISTS ""IX_Reviews_UserId_GameId"";
                    CREATE UNIQUE INDEX IF NOT EXISTS ""IX_Reviews_UserId_GameId"" ON ""Reviews"" (""UserId"", ""GameId"") WHERE ""ParentReviewId"" IS NULL;
                    CREATE INDEX IF NOT EXISTS ""IX_Reviews_ParentReviewId"" ON ""Reviews"" (""ParentReviewId"");
                ");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "[InitData] Error ensuring Reviews schema in SQLite database.");
            }
        }
    }
}
