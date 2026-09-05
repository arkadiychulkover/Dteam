using DteamBackend.Data;
using DteamBackend.Interfaces;
using DteamBackend.Models;
using DteamBackend.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Text.Json;

namespace DteamBackend.Services
{
    public class InitDataService : IInitDataService
    {
        private readonly AppDbContext? _context;
        private readonly ILogger<InitDataService>? _logger;
        private readonly IConfiguration? _configuration;

        public InitDataService()
        {
        }

        public InitDataService(AppDbContext context, ILogger<InitDataService> logger, IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task InitializeAsync(AppDbContext context)
        {
            if (await context.Users.AnyAsync())
            {
                return;
            }

            PasswordHasher.CreatePasswordHash("admin123321", out string passwordHash, out string passwordSalt);

            var adminEthAddress = _configuration?["Ethereum:PublicKey"] ?? "0xf39fd6e51aad88f6f4ce6ab8827279cfffb92266";

            var adminUser = new Duser
            {
                Id = Guid.NewGuid(),
                Email = "adim@gmail.com",
                Username = "adim",
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                WalletAddress = "EQB_v1zX3L1f2M9zX_SampleAdminTonWalletAddress_777",
                HardhatAddress = adminEthAddress,
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
                Features = new List<string> { "SinglePlayer", "SteamAchievements", "FullControllerSupport", "SteamCloud", "SteamTradingCards" },
                SupportedLanguages = new List<GameLanguageSupport>
                {
                    new() { Language = "Українська", Interface = true, FullAudio = false, Subtitles = true },
                    new() { Language = "English", Interface = true, FullAudio = true, Subtitles = true },
                    new() { Language = "Deutsch", Interface = true, FullAudio = true, Subtitles = true },
                    new() { Language = "Français", Interface = true, FullAudio = true, Subtitles = true },
                    new() { Language = "Polski", Interface = true, FullAudio = true, Subtitles = true },
                    new() { Language = "Español", Interface = true, FullAudio = true, Subtitles = true },
                    new() { Language = "日本語", Interface = true, FullAudio = true, Subtitles = true }
                },
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

            // Seed demo friends from the mockup
            var demoUsersData = new (string email, string username, string avatar, UserStatus status)[]
            {
                ("mrszubarikessa@dteam.io", "MrsZubarikessa", "https://images.unsplash.com/photo-1544551763-46a013bb70d5?w=500&auto=format&fit=crop&q=80", UserStatus.Online),
                ("firephoenix@dteam.io", "FirePhoenix", "https://images.unsplash.com/photo-1566492031773-4f4e44671857?w=500&auto=format&fit=crop&q=80", UserStatus.Online),
                ("dragonslayer@dteam.io", "DragonSlayer", "https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=500&auto=format&fit=crop&q=80", UserStatus.Offline),
                ("titancrusher@dteam.io", "TitanCrusher", "https://images.unsplash.com/photo-1500648767791-00dcc994a43e?w=500&auto=format&fit=crop&q=80", UserStatus.Online),
                ("blazingarrow@dteam.io", "BlazingArrow", "https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?w=500&auto=format&fit=crop&q=80", UserStatus.Offline),
                ("sinichka@dteam.io", "sinichka_bez_egg", "https://images.unsplash.com/photo-1570295999919-56ceb5ecca61?w=500&auto=format&fit=crop&q=80", UserStatus.Online),
                ("silentassassin@dteam.io", "SilentAssassin", "https://images.unsplash.com/photo-1527980965255-d3b416303d12?w=500&auto=format&fit=crop&q=80", UserStatus.Online),
                ("lunarmage@dteam.io", "LunarMage", "https://images.unsplash.com/photo-1580489944761-15a19d654956?w=500&auto=format&fit=crop&q=80", UserStatus.Offline)
            };

            // Hardhat accounts #1 to #8 for demo friends
            var hardhatDemoAccounts = new[]
            {
                "0x70997970C51812dc3A010C7d01b50e0d17dc79C8",
                "0x3C44CdDdB6a900fa2b585dd299e03d12FA4293BC",
                "0x90F79bf6EB2c4f870365E785982E1f101E93b906",
                "0x15d34AAf54267DB7D7c367839AAf71A00a2C6A65",
                "0x9965507D1a55bcC2695C58ba16FB37d819B0A4df",
                "0x976EA74026E726554dB657fA54763abd0C3a0aa9",
                "0x14dC79964da2C08b23698B3D3cc7Ca32193d9955",
                "0x23618e81E3f5cdF7f54C3d65f7FBc0aBf5B21E8f"
            };

            var friendUsers = new List<Duser>();
            for (int i = 0; i < demoUsersData.Length; i++)
            {
                var item = demoUsersData[i];
                var user = await context.Users.FirstOrDefaultAsync(u => u.Email == item.email || u.Username == item.username);
                if (user == null)
                {
                    user = new Duser
                    {
                        Id = Guid.NewGuid(),
                        Email = item.email,
                        Username = item.username,
                        PasswordHash = passwordHash,
                        PasswordSalt = passwordSalt,
                        WalletAddress = $"EQB_{item.username.ToLowerInvariant()}_wallet_addr",
                        HardhatAddress = i < hardhatDemoAccounts.Length ? hardhatDemoAccounts[i] : null,
                        BalanceInNanoTons = 10_000_000_000,
                        CreatedAt = DateTime.UtcNow.AddDays(-30),
                        Status = item.status,
                        AvatarUrl = item.avatar,
                        Bio = $"Gaming enthusiast — {item.username}"
                    };
                    await context.Users.AddAsync(user);
                }
                friendUsers.Add(user);
            }
            await context.SaveChangesAsync();

            // Establish friendships with admin user
            foreach (var friend in friendUsers)
            {
                bool friendship1 = await context.UserFriends.AnyAsync(uf => uf.UserId == adminUser.Id && uf.FriendId == friend.Id);
                if (!friendship1)
                {
                    context.UserFriends.Add(new UserFriend
                    {
                        UserId = adminUser.Id,
                        FriendId = friend.Id,
                        CreatedAt = DateTime.UtcNow.AddDays(-10)
                    });
                }

                bool friendship2 = await context.UserFriends.AnyAsync(uf => uf.UserId == friend.Id && uf.FriendId == adminUser.Id);
                if (!friendship2)
                {
                    context.UserFriends.Add(new UserFriend
                    {
                        UserId = friend.Id,
                        FriendId = adminUser.Id,
                        CreatedAt = DateTime.UtcNow.AddDays(-10)
                    });
                }
            }
            await context.SaveChangesAsync();

            // Seed demo messages if none exist
            if (!await context.ChatMessages.AnyAsync())
            {
                var zubarikessa = friendUsers.First(u => u.Username == "MrsZubarikessa");
                var firePhoenix = friendUsers.First(u => u.Username == "FirePhoenix");
                var dragonSlayer = friendUsers.First(u => u.Username == "DragonSlayer");
                var titanCrusher = friendUsers.First(u => u.Username == "TitanCrusher");
                var sinichka = friendUsers.First(u => u.Username == "sinichka_bez_egg");
                var silentAssassin = friendUsers.First(u => u.Username == "SilentAssassin");

                var messages = new List<ChatMessage>
                {
                    // 1. MrsZubarikessa conversation
                    new()
                    {
                        Id = Guid.NewGuid(),
                        ClientMessageId = Guid.NewGuid().ToString("N"),
                        SenderId = zubarikessa.Id,
                        ReceiverId = adminUser.Id,
                        Content = "Привіт! Як справи?",
                        Type = ChatMessageType.Text,
                        Status = MessageDeliveryStatus.Read,
                        CreatedAt = DateTimeOffset.UtcNow.AddHours(-2),
                        ReadAt = DateTimeOffset.UtcNow.AddHours(-1)
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        ClientMessageId = Guid.NewGuid().ToString("N"),
                        SenderId = zubarikessa.Id,
                        ReceiverId = adminUser.Id,
                        Content = "Я чув про гарне місце неподалік нашого міста. Там є озеро і ліс. Що думаєш?",
                        Type = ChatMessageType.Text,
                        Status = MessageDeliveryStatus.Read,
                        CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-45),
                        ReadAt = DateTimeOffset.UtcNow.AddMinutes(-40)
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        ClientMessageId = Guid.NewGuid().ToString("N"),
                        SenderId = zubarikessa.Id,
                        ReceiverId = adminUser.Id,
                        Content = "Це ідея! Я візьму на себе напої. Коли тобі зручно виїхати?",
                        Type = ChatMessageType.Text,
                        Status = MessageDeliveryStatus.Read,
                        CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-30),
                        ReadAt = DateTimeOffset.UtcNow.AddMinutes(-28)
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        ClientMessageId = Guid.NewGuid().ToString("N"),
                        SenderId = adminUser.Id,
                        ReceiverId = zubarikessa.Id,
                        Content = "Нещодавно почав працювати над новим проєктом на роботі. А в тебе які новини?",
                        Type = ChatMessageType.Text,
                        Status = MessageDeliveryStatus.Read,
                        CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-20),
                        ReadAt = DateTimeOffset.UtcNow.AddMinutes(-18)
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        ClientMessageId = Guid.NewGuid().ToString("N"),
                        SenderId = zubarikessa.Id,
                        ReceiverId = adminUser.Id,
                        Content = "Йо, ти де пропав? Давно тебе не було видно.",
                        Type = ChatMessageType.Text,
                        Status = MessageDeliveryStatus.Sent,
                        CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-5)
                    },

                    // 2. FirePhoenix conversation
                    new()
                    {
                        Id = Guid.NewGuid(),
                        ClientMessageId = Guid.NewGuid().ToString("N"),
                        SenderId = firePhoenix.Id,
                        ReceiverId = adminUser.Id,
                        Content = "Чи можеш допомогти з одним питанням по грі?",
                        Type = ChatMessageType.Text,
                        Status = MessageDeliveryStatus.Sent,
                        CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-15)
                    },

                    // 3. DragonSlayer conversation
                    new()
                    {
                        Id = Guid.NewGuid(),
                        ClientMessageId = Guid.NewGuid().ToString("N"),
                        SenderId = adminUser.Id,
                        ReceiverId = dragonSlayer.Id,
                        Content = "Хочеш разом пограти в гру сьогодні ввечері?",
                        Type = ChatMessageType.Text,
                        Status = MessageDeliveryStatus.Read,
                        CreatedAt = DateTimeOffset.UtcNow.AddHours(-1)
                    },

                    // 4. TitanCrusher conversation
                    new()
                    {
                        Id = Guid.NewGuid(),
                        ClientMessageId = Guid.NewGuid().ToString("N"),
                        SenderId = titanCrusher.Id,
                        ReceiverId = adminUser.Id,
                        Content = "Планую невеличку вечірку, ти з нами?",
                        Type = ChatMessageType.Text,
                        Status = MessageDeliveryStatus.Sent,
                        CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-25)
                    },

                    // 5. Sinichka conversation
                    new()
                    {
                        Id = Guid.NewGuid(),
                        ClientMessageId = Guid.NewGuid().ToString("N"),
                        SenderId = sinichka.Id,
                        ReceiverId = adminUser.Id,
                        Content = "Маєш час на швидкий дзвінок?",
                        Type = ChatMessageType.Text,
                        Status = MessageDeliveryStatus.Sent,
                        CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-50)
                    },

                    // 6. SilentAssassin conversation
                    new()
                    {
                        Id = Guid.NewGuid(),
                        ClientMessageId = Guid.NewGuid().ToString("N"),
                        SenderId = silentAssassin.Id,
                        ReceiverId = adminUser.Id,
                        Content = "Як просувається твій проєкт?",
                        Type = ChatMessageType.Text,
                        Status = MessageDeliveryStatus.Sent,
                        CreatedAt = DateTimeOffset.UtcNow.AddHours(-3)
                    }
                };

                await context.ChatMessages.AddRangeAsync(messages);
                await context.SaveChangesAsync();
            }
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

                                // Обнуляем GameGuidId для постов, чьи игры не существуют в БД
                                var referencedGameIds = data.Posts
                                    .Where(p => p.GameGuidId.HasValue)
                                    .Select(p => p.GameGuidId!.Value)
                                    .Distinct()
                                    .ToList();

                                var existingGameIds = await context.Games
                                    .Where(g => referencedGameIds.Contains(g.Id))
                                    .Select(g => g.Id)
                                    .ToListAsync();

                                var existingSet = new HashSet<Guid>(existingGameIds);
                                foreach (var p in data.Posts)
                                {
                                    if (p.GameGuidId.HasValue && !existingSet.Contains(p.GameGuidId.Value))
                                    {
                                        p.GameGuidId = null;
                                    }
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

        public async Task EnsureChatSchemaAsync(AppDbContext context)
        {
            try
            {
                await context.Database.ExecuteSqlRawAsync(@"
                    CREATE TABLE IF NOT EXISTS ""ChatMessages"" (
                        ""Id"" TEXT NOT NULL CONSTRAINT ""PK_ChatMessages"" PRIMARY KEY,
                        ""ClientMessageId"" TEXT NULL,
                        ""SenderId"" TEXT NOT NULL,
                        ""ReceiverId"" TEXT NOT NULL,
                        ""Content"" TEXT NULL,
                        ""Type"" INTEGER NOT NULL,
                        ""Status"" INTEGER NOT NULL,
                        ""StorageKey"" TEXT NULL,
                        ""OriginalFileName"" TEXT NULL,
                        ""ContentType"" TEXT NULL,
                        ""FileSize"" INTEGER NULL,
                        ""Duration"" INTEGER NULL,
                        ""CreatedAt"" INTEGER NOT NULL,
                        ""ReadAt"" INTEGER NULL,
                        ""IsDeletedForSender"" INTEGER NOT NULL DEFAULT 0,
                        ""IsDeletedForReceiver"" INTEGER NOT NULL DEFAULT 0,
                        CONSTRAINT ""FK_ChatMessages_Users_ReceiverId"" FOREIGN KEY (""ReceiverId"") REFERENCES ""Users"" (""Id"") ON DELETE RESTRICT,
                        CONSTRAINT ""FK_ChatMessages_Users_SenderId"" FOREIGN KEY (""SenderId"") REFERENCES ""Users"" (""Id"") ON DELETE RESTRICT
                    );

                    CREATE UNIQUE INDEX IF NOT EXISTS ""IX_ChatMessages_SenderId_ClientMessageId"" ON ""ChatMessages"" (""SenderId"", ""ClientMessageId"");
                    CREATE INDEX IF NOT EXISTS ""IX_ChatMessages_ReceiverId_SenderId_CreatedAt"" ON ""ChatMessages"" (""ReceiverId"", ""SenderId"", ""CreatedAt"");
                    CREATE INDEX IF NOT EXISTS ""IX_ChatMessages_ReceiverId_Status_CreatedAt"" ON ""ChatMessages"" (""ReceiverId"", ""Status"", ""CreatedAt"");
                    CREATE INDEX IF NOT EXISTS ""IX_ChatMessages_SenderId_ReceiverId_CreatedAt"" ON ""ChatMessages"" (""SenderId"", ""ReceiverId"", ""CreatedAt"");

                    CREATE TABLE IF NOT EXISTS ""ChatUploads"" (
                        ""Id"" TEXT NOT NULL CONSTRAINT ""PK_ChatUploads"" PRIMARY KEY,
                        ""UserId"" TEXT NOT NULL,
                        ""StorageKey"" TEXT NOT NULL,
                        ""OriginalFileName"" TEXT NOT NULL,
                        ""ContentType"" TEXT NOT NULL,
                        ""FileSize"" INTEGER NOT NULL,
                        ""Duration"" INTEGER NULL,
                        ""IsCommitted"" INTEGER NOT NULL DEFAULT 0,
                        ""MessageId"" TEXT NULL,
                        ""CreatedAt"" INTEGER NOT NULL,
                        CONSTRAINT ""FK_ChatUploads_ChatMessages_MessageId"" FOREIGN KEY (""MessageId"") REFERENCES ""ChatMessages"" (""Id"") ON DELETE SET NULL,
                        CONSTRAINT ""FK_ChatUploads_Users_UserId"" FOREIGN KEY (""UserId"") REFERENCES ""Users"" (""Id"") ON DELETE CASCADE
                    );

                    CREATE INDEX IF NOT EXISTS ""IX_ChatUploads_MessageId"" ON ""ChatUploads"" (""MessageId"");
                    CREATE INDEX IF NOT EXISTS ""IX_ChatUploads_UserId"" ON ""ChatUploads"" (""UserId"");
                ");

                _logger?.LogInformation("[InitData] ChatMessages and ChatUploads schema successfully ensured.");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "[InitData] Error ensuring Chat schema in SQLite database.");
            }
        }

        public async Task EnsureActivitySchemaAsync(AppDbContext context)
        {
            try
            {
                await context.Database.ExecuteSqlRawAsync(@"
                    CREATE TABLE IF NOT EXISTS ""UserActivities"" (
                        ""Id"" TEXT NOT NULL CONSTRAINT ""PK_UserActivities"" PRIMARY KEY,
                        ""UserId"" TEXT NOT NULL,
                        ""Type"" INTEGER NOT NULL,
                        ""Title"" TEXT NOT NULL,
                        ""Description"" TEXT NULL,
                        ""Details"" TEXT NULL,
                        ""RelatedEntityId"" TEXT NULL,
                        ""ImageUrl"" TEXT NULL,
                        ""CreatedAt"" TEXT NOT NULL,
                        CONSTRAINT ""FK_UserActivities_Users_UserId"" FOREIGN KEY (""UserId"") REFERENCES ""Users"" (""Id"") ON DELETE CASCADE
                    );

                    CREATE INDEX IF NOT EXISTS ""IX_UserActivities_UserId"" ON ""UserActivities"" (""UserId"");
                    CREATE INDEX IF NOT EXISTS ""IX_UserActivities_CreatedAt"" ON ""UserActivities"" (""CreatedAt"");
                    CREATE INDEX IF NOT EXISTS ""IX_UserActivities_UserId_CreatedAt"" ON ""UserActivities"" (""UserId"", ""CreatedAt"");
                ");

                try
                {
                    await context.Database.ExecuteSqlRawAsync(@"ALTER TABLE ""Games"" ADD COLUMN ""SupportedLanguages"" TEXT NULL;");
                }
                catch { /* Column already exists or database created fresh */ }

                try
                {
                    await context.Database.ExecuteSqlRawAsync(@"UPDATE ""Games"" SET ""SupportedLanguages"" = '[]' WHERE ""SupportedLanguages"" IS NULL;");
                }
                catch { }

                _logger?.LogInformation("[InitData] UserActivities and Games schema successfully ensured.");

                if (!await context.UserActivities.AnyAsync())
                {
                    var user = await context.Users.FirstOrDefaultAsync();
                    var game = await context.Games.FirstOrDefaultAsync();

                    if (user != null)
                    {
                        var sampleActivities = new List<UserActivity>();

                        if (game != null)
                        {
                            sampleActivities.Add(new UserActivity
                            {
                                Id = Guid.NewGuid(),
                                UserId = user.Id,
                                Type = UserActivityType.GamePurchased,
                                Title = $"Придбав(ла) гру {game.Title}",
                                Description = game.ShortDescription ?? game.Description,
                                Details = JsonSerializer.Serialize(new { gameId = game.Id, gameTitle = game.Title, price = game.PriceInNanoTons }),
                                RelatedEntityId = game.Id,
                                ImageUrl = game.CoverImageUrl ?? game.HeaderImageUrl,
                                CreatedAt = DateTime.UtcNow.AddMinutes(-25)
                            });

                            sampleActivities.Add(new UserActivity
                            {
                                Id = Guid.NewGuid(),
                                UserId = user.Id,
                                Type = UserActivityType.GamePublished,
                                Title = $"Опубліковано нову гру {game.Title}",
                                Description = game.ShortDescription,
                                Details = JsonSerializer.Serialize(new { gameId = game.Id, gameTitle = game.Title }),
                                RelatedEntityId = game.Id,
                                ImageUrl = game.CoverImageUrl ?? game.HeaderImageUrl,
                                CreatedAt = DateTime.UtcNow.AddHours(-3)
                            });
                        }

                        sampleActivities.Add(new UserActivity
                        {
                            Id = Guid.NewGuid(),
                            UserId = user.Id,
                            Type = UserActivityType.BalanceDeposited,
                            Title = "Поповнив(ла) баланс на 10 TON",
                            Description = "Успішне зарахування коштів у мережі TON (TX: 3a9f1b2c4d5e...)",
                            Details = JsonSerializer.Serialize(new { amount = 10m, txhHash = "3a9f1b2c4d5e6f7a8b9c0d1e2f3a4b5c6d7e8f9a" }),
                            CreatedAt = DateTime.UtcNow.AddDays(-1)
                        });

                        await context.UserActivities.AddRangeAsync(sampleActivities);
                        await context.SaveChangesAsync();
                        _logger?.LogInformation($"[InitData] Seeded {sampleActivities.Count} sample activities.");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "[InitData] Error ensuring UserActivities schema in SQLite database.");
            }
        }

        public async Task EnsureUserOnlineTrackingSchemaAsync(AppDbContext context)
        {
            try
            {
                var connection = context.Database.GetDbConnection();
                await connection.OpenAsync();

                using var cmd = connection.CreateCommand();
                cmd.CommandText = "PRAGMA table_info('Users');";
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

                if (!columns.Contains("LastConnectedAt"))
                {
                    await context.Database.ExecuteSqlRawAsync("ALTER TABLE \"Users\" ADD COLUMN \"LastConnectedAt\" TEXT NULL;");
                    _logger?.LogInformation("[InitData] Added LastConnectedAt column to Users table.");
                }

                if (!columns.Contains("LastDisconnectedAt"))
                {
                    await context.Database.ExecuteSqlRawAsync("ALTER TABLE \"Users\" ADD COLUMN \"LastDisconnectedAt\" TEXT NULL;");
                    _logger?.LogInformation("[InitData] Added LastDisconnectedAt column to Users table.");
                }

                if (!columns.Contains("TotalTimeSpentSeconds"))
                {
                    await context.Database.ExecuteSqlRawAsync("ALTER TABLE \"Users\" ADD COLUMN \"TotalTimeSpentSeconds\" INTEGER NOT NULL DEFAULT 0;");
                    _logger?.LogInformation("[InitData] Added TotalTimeSpentSeconds column to Users table.");
                }

                if (!columns.Contains("TimeRewardNftsMintedCount"))
                {
                    await context.Database.ExecuteSqlRawAsync("ALTER TABLE \"Users\" ADD COLUMN \"TimeRewardNftsMintedCount\" INTEGER NOT NULL DEFAULT 0;");
                    _logger?.LogInformation("[InitData] Added TimeRewardNftsMintedCount column to Users table.");
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "[InitData] Error ensuring user online tracking schema in SQLite database.");
            }
        }

        public async Task EnsureTasteVectorSchemaAsync(AppDbContext context)
        {
            try
            {
                var connection = context.Database.GetDbConnection();
                await connection.OpenAsync();

                // Check Users table
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "PRAGMA table_info('Users');";
                    var userCols = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var colName = reader["name"]?.ToString();
                            if (!string.IsNullOrEmpty(colName)) userCols.Add(colName);
                        }
                    }

                    if (!userCols.Contains("TasteVectorJson"))
                    {
                        var defaultBase = JsonSerializer.Serialize(TasteCategories.Baseline()).Replace("'", "''");
                        var sql = "ALTER TABLE \"Users\" ADD COLUMN \"TasteVectorJson\" TEXT NOT NULL DEFAULT '" + defaultBase + "';";
                        await context.Database.ExecuteSqlRawAsync(sql);
                        _logger?.LogInformation("[InitData] Added TasteVectorJson column to Users table.");
                    }
                }

                // Check Games table
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "PRAGMA table_info('Games');";
                    var gameCols = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var colName = reader["name"]?.ToString();
                            if (!string.IsNullOrEmpty(colName)) gameCols.Add(colName);
                        }
                    }

                    if (!gameCols.Contains("TasteVectorJson"))
                    {
                        var defaultEmpty = JsonSerializer.Serialize(TasteCategories.Empty()).Replace("'", "''");
                        var sql = "ALTER TABLE \"Games\" ADD COLUMN \"TasteVectorJson\" TEXT NOT NULL DEFAULT '" + defaultEmpty + "';";
                        await context.Database.ExecuteSqlRawAsync(sql);
                        _logger?.LogInformation("[InitData] Added TasteVectorJson column to Games table.");
                    }
                }

                // Recalculate taste vectors for games that have zero or uninitialized vectors
                var games = await context.Games.ToListAsync();
                var changed = false;
                foreach (var game in games)
                {
                    if (game.TasteVector == null || game.TasteVector.Length != TasteCategories.Length || game.TasteVector.All(v => Math.Abs(v) < 1e-6f))
                    {
                        game.RecalculateTasteVector();
                        changed = true;
                    }
                }

                if (changed)
                {
                    await context.SaveChangesAsync();
                    _logger?.LogInformation("[InitData] Recalculated taste vectors for games in database.");
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "[InitData] Error ensuring TasteVector schema in SQLite database.");
            }
        }
    }
}
