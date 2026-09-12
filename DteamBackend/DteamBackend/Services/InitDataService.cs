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

        public async Task EnsureAllSchemasAsync(AppDbContext context)
        {
            try
            {
                await EnsureNotificationAndSettingsSchemaAsync(context);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "[InitData] Error in EnsureNotificationAndSettingsSchemaAsync");
            }

            try
            {
                await EnsureCollectionSchemaAsync(context);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "[InitData] Error in EnsureCollectionSchemaAsync");
            }

            try
            {
                await EnsureUserOnlineTrackingSchemaAsync(context);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "[InitData] Error in EnsureUserOnlineTrackingSchemaAsync");
            }

            try
            {
                await EnsureTasteVectorSchemaAsync(context);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "[InitData] Error in EnsureTasteVectorSchemaAsync");
            }

            try
            {
                await EnsureReviewSchemaAsync(context);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "[InitData] Error in EnsureReviewSchemaAsync");
            }

            try
            {
                await EnsureChatSchemaAsync(context);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "[InitData] Error in EnsureChatSchemaAsync");
            }

            try
            {
                await EnsureActivitySchemaAsync(context);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "[InitData] Error in EnsureActivitySchemaAsync");
            }

            try
            {
                await EnsureCommunityDataAsync(context);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "[InitData] Error in EnsureCommunityDataAsync");
            }

            try
            {
                await EnsureGamesSeededAsync(context);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "[InitData] Error in EnsureGamesSeededAsync");
            }
        }

        public async Task InitializeAsync(AppDbContext context)
        {
            if (!await context.Users.AnyAsync())
            {

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

            await context.SaveChangesAsync();

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

            await EnsureGamesSeededAsync(context);
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
                if (!context.Database.IsSqlite()) return;

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
                catch {  }

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
                if (!context.Database.IsSqlite()) return;
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
                if (!context.Database.IsSqlite()) return;
                var connection = context.Database.GetDbConnection();
                await connection.OpenAsync();

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

        public async Task EnsureNotificationAndSettingsSchemaAsync(AppDbContext context)
        {
            try
            {
                if (!context.Database.IsSqlite()) return;

                try { await context.Database.ExecuteSqlRawAsync("ALTER TABLE \"Users\" ADD COLUMN \"PreferredLanguage\" TEXT NOT NULL DEFAULT 'uk';"); } catch { }
                try { await context.Database.ExecuteSqlRawAsync("ALTER TABLE \"Users\" ADD COLUMN \"IsDeleted\" INTEGER NOT NULL DEFAULT 0;"); } catch { }
                try { await context.Database.ExecuteSqlRawAsync("ALTER TABLE \"Users\" ADD COLUMN \"DeletedAt\" TEXT NULL;"); } catch { }

                var connection = context.Database.GetDbConnection();
                if (connection.State != System.Data.ConnectionState.Open)
                {
                    await connection.OpenAsync();
                }

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

                    if (!userCols.Contains("PreferredLanguage"))
                    {
                        await context.Database.ExecuteSqlRawAsync("ALTER TABLE \"Users\" ADD COLUMN \"PreferredLanguage\" TEXT NOT NULL DEFAULT 'uk';");
                        _logger?.LogInformation("[InitData] Added PreferredLanguage column to Users table.");
                    }

                    if (!userCols.Contains("IsDeleted"))
                    {
                        await context.Database.ExecuteSqlRawAsync("ALTER TABLE \"Users\" ADD COLUMN \"IsDeleted\" INTEGER NOT NULL DEFAULT 0;");
                        _logger?.LogInformation("[InitData] Added IsDeleted column to Users table.");
                    }

                    if (!userCols.Contains("DeletedAt"))
                    {
                        await context.Database.ExecuteSqlRawAsync("ALTER TABLE \"Users\" ADD COLUMN \"DeletedAt\" TEXT NULL;");
                        _logger?.LogInformation("[InitData] Added DeletedAt column to Users table.");
                    }
                }

                await context.Database.ExecuteSqlRawAsync(@"
                    CREATE TABLE IF NOT EXISTS ""UserNotificationPreferences"" (
                        ""UserId"" TEXT NOT NULL CONSTRAINT ""PK_UserNotificationPreferences"" PRIMARY KEY,
                        ""NotifyBigSales"" INTEGER NOT NULL DEFAULT 1,
                        ""NotifyWishlistDiscounts"" INTEGER NOT NULL DEFAULT 1,
                        ""NotifyProfileComments"" INTEGER NOT NULL DEFAULT 1,
                        ""NotifyFriendRequests"" INTEGER NOT NULL DEFAULT 1,
                        ""NotifyFriendRequestAccepted"" INTEGER NOT NULL DEFAULT 1,
                        ""NotifyFriendRequestDeclined"" INTEGER NOT NULL DEFAULT 1,
                        ""ChatNotificationsEnabled"" INTEGER NOT NULL DEFAULT 1,
                        ""ChatSoundEnabled"" INTEGER NOT NULL DEFAULT 1,
                        ""UpdatedAt"" TEXT NOT NULL,
                        CONSTRAINT ""FK_UserNotificationPreferences_Users_UserId"" FOREIGN KEY (""UserId"") REFERENCES ""Users"" (""Id"") ON DELETE CASCADE
                    );
                ");

                await context.Database.ExecuteSqlRawAsync(@"
                    CREATE TABLE IF NOT EXISTS ""WalletTransactions"" (
                        ""Id"" TEXT NOT NULL CONSTRAINT ""PK_WalletTransactions"" PRIMARY KEY,
                        ""UserId"" TEXT NOT NULL,
                        ""Type"" INTEGER NOT NULL,
                        ""Status"" INTEGER NOT NULL DEFAULT 1,
                        ""AmountInNanoTons"" INTEGER NOT NULL,
                        ""Title"" TEXT NOT NULL,
                        ""Currency"" TEXT NOT NULL DEFAULT 'TON',
                        ""CreatedAt"" TEXT NOT NULL,
                        ""ReferenceId"" TEXT NULL,
                        ""Metadata"" TEXT NULL,
                        CONSTRAINT ""FK_WalletTransactions_Users_UserId"" FOREIGN KEY (""UserId"") REFERENCES ""Users"" (""Id"") ON DELETE RESTRICT
                    );
                    CREATE INDEX IF NOT EXISTS ""IX_WalletTransactions_UserId"" ON ""WalletTransactions"" (""UserId"");
                    CREATE INDEX IF NOT EXISTS ""IX_WalletTransactions_CreatedAt"" ON ""WalletTransactions"" (""CreatedAt"");
                ");

                await context.Database.ExecuteSqlRawAsync(@"
                    CREATE TABLE IF NOT EXISTS ""Notifications"" (
                        ""Id"" TEXT NOT NULL CONSTRAINT ""PK_Notifications"" PRIMARY KEY,
                        ""UserId"" TEXT NOT NULL,
                        ""ActorUserId"" TEXT NULL,
                        ""Type"" TEXT NOT NULL,
                        ""EntityType"" TEXT NULL,
                        ""EntityId"" TEXT NULL,
                        ""EventId"" TEXT NULL,
                        ""Title"" TEXT NOT NULL,
                        ""Message"" TEXT NOT NULL,
                        ""DataJson"" TEXT NULL,
                        ""IsRead"" INTEGER NOT NULL DEFAULT 0,
                        ""CreatedAt"" TEXT NOT NULL,
                        ""ReadAt"" TEXT NULL,
                        ""IsDeleted"" INTEGER NOT NULL DEFAULT 0,
                        ""DeletedAt"" TEXT NULL,
                        CONSTRAINT ""FK_Notifications_Users_UserId"" FOREIGN KEY (""UserId"") REFERENCES ""Users"" (""Id"") ON DELETE CASCADE,
                        CONSTRAINT ""FK_Notifications_Users_ActorUserId"" FOREIGN KEY (""ActorUserId"") REFERENCES ""Users"" (""Id"") ON DELETE SET NULL
                    );
                    CREATE INDEX IF NOT EXISTS ""IX_Notifications_UserId_IsDeleted_CreatedAt"" ON ""Notifications"" (""UserId"", ""IsDeleted"", ""CreatedAt"");
                    CREATE INDEX IF NOT EXISTS ""IX_Notifications_UserId_IsDeleted_IsRead"" ON ""Notifications"" (""UserId"", ""IsDeleted"", ""IsRead"");
                ");

                _logger?.LogInformation("[InitData] Notifications and Settings schema successfully ensured.");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "[InitData] Error ensuring Notification and Settings schema in database.");
            }
        }

        public async Task EnsureCollectionSchemaAsync(AppDbContext context)
        {
            try
            {
                await context.Database.ExecuteSqlRawAsync(@"
                    CREATE TABLE IF NOT EXISTS ""GameCollections"" (
                        ""Id"" TEXT NOT NULL CONSTRAINT ""PK_GameCollections"" PRIMARY KEY,
                        ""UserId"" TEXT NOT NULL,
                        ""Name"" TEXT NOT NULL,
                        ""Description"" TEXT NULL,
                        ""CreatedAt"" TEXT NOT NULL,
                        ""UpdatedAt"" TEXT NOT NULL,
                        CONSTRAINT ""FK_GameCollections_Users_UserId"" FOREIGN KEY (""UserId"") REFERENCES ""Users"" (""Id"") ON DELETE CASCADE
                    );
                    CREATE INDEX IF NOT EXISTS ""IX_GameCollections_UserId"" ON ""GameCollections"" (""UserId"");
                    CREATE INDEX IF NOT EXISTS ""IX_GameCollections_UserId_Name"" ON ""GameCollections"" (""UserId"", ""Name"");

                    CREATE TABLE IF NOT EXISTS ""GameCollectionItems"" (
                        ""CollectionId"" TEXT NOT NULL,
                        ""GameId"" TEXT NOT NULL,
                        ""AddedAt"" TEXT NOT NULL,
                        CONSTRAINT ""PK_GameCollectionItems"" PRIMARY KEY (""CollectionId"", ""GameId""),
                        CONSTRAINT ""FK_GameCollectionItems_GameCollections_CollectionId"" FOREIGN KEY (""CollectionId"") REFERENCES ""GameCollections"" (""Id"") ON DELETE CASCADE,
                        CONSTRAINT ""FK_GameCollectionItems_Games_GameId"" FOREIGN KEY (""GameId"") REFERENCES ""Games"" (""Id"") ON DELETE CASCADE
                    );
                    CREATE INDEX IF NOT EXISTS ""IX_GameCollectionItems_GameId"" ON ""GameCollectionItems"" (""GameId"");
                ");

                _logger?.LogInformation("[InitData] Game collections schema successfully ensured.");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "[InitData] Error ensuring Game collections schema in database.");
            }
        }

        public async Task EnsureGamesSeededAsync(AppDbContext context)
        {
            try
            {
                var adminUser = await context.Users.FirstOrDefaultAsync(u => u.IsAdmin)
                    ?? await context.Users.FirstOrDefaultAsync();

                if (adminUser == null)
                {
                    return;
                }

                var existingGames = await context.Games.ToListAsync();
                var gamesByTitle = existingGames.ToDictionary(g => g.Title, g => g, StringComparer.OrdinalIgnoreCase);

                var officialMainTitles = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    "Cyberpunk 2077",
                    "The Witcher 3: Wild Hunt",
                    "Elden Ring",
                    "DOOM Eternal",
                    "Sid Meier's Civilization VI",
                    "Forza Horizon 5",
                    "Resident Evil 4 Remake",
                    "Baldur's Gate 3",
                    "Dead Cells",
                    "Subnautica"
                };

                var officialDlcTitles = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    "Cyberpunk 2077: Phantom Liberty",
                    "Cyberpunk 2077: REDmod",
                    "The Witcher 3: Hearts of Stone",
                    "The Witcher 3: Blood and Wine",
                    "Elden Ring: Shadow of the Erdtree",
                    "Elden Ring: Colosseum & Soundtrack",
                    "DOOM Eternal: The Ancient Gods - Part One",
                    "DOOM Eternal: The Ancient Gods - Part Two",
                    "Civilization VI: Gathering Storm",
                    "Civilization VI: Rise and Fall",
                    "Forza Horizon 5: Hot Wheels",
                    "Forza Horizon 5: Rally Adventure",
                    "Resident Evil 4: Separate Ways",
                    "Resident Evil 4: Extra DLC Pack",
                    "Baldur's Gate 3: Digital Deluxe Upgrade",
                    "Baldur's Gate 3: Adventure Companion Pack",
                    "Dead Cells: Return to Castlevania",
                    "Dead Cells: The Bad Seed",
                    "Subnautica: Below Zero",
                    "Subnautica: Deep Sea Pack"
                };

                var unwantedGames = existingGames
                    .Where(g => !officialMainTitles.Contains(g.Title) && !officialDlcTitles.Contains(g.Title))
                    .ToList();

                if (unwantedGames.Count > 0)
                {
                    var unwantedIds = unwantedGames.Select(g => g.Id).ToList();

                    var reviews = await context.Reviews.Where(r => unwantedIds.Contains(r.GameId)).ToListAsync();
                    if (reviews.Count > 0) context.Reviews.RemoveRange(reviews);

                    var carts = await context.UserCartItems.Where(c => unwantedIds.Contains(c.GameId)).ToListAsync();
                    if (carts.Count > 0) context.UserCartItems.RemoveRange(carts);

                    var wishes = await context.UserWishlists.Where(w => unwantedIds.Contains(w.GameId)).ToListAsync();
                    if (wishes.Count > 0) context.UserWishlists.RemoveRange(wishes);

                    var userGames = await context.UserGames.Where(ug => unwantedIds.Contains(ug.GameId)).ToListAsync();
                    if (userGames.Count > 0) context.UserGames.RemoveRange(userGames);

                    var collectionItems = await context.GameCollectionItems.Where(ci => unwantedIds.Contains(ci.GameId)).ToListAsync();
                    if (collectionItems.Count > 0) context.GameCollectionItems.RemoveRange(collectionItems);

                    context.Games.RemoveRange(unwantedGames);
                    await context.SaveChangesAsync();

                    foreach (var g in unwantedGames)
                    {
                        gamesByTitle.Remove(g.Title);
                    }
                }

                var defaultLanguages = new List<GameLanguageSupport>
                {
                    new() { Language = "Українська", Interface = true, FullAudio = false, Subtitles = true },
                    new() { Language = "English", Interface = true, FullAudio = true, Subtitles = true },
                    new() { Language = "Deutsch", Interface = true, FullAudio = true, Subtitles = true },
                    new() { Language = "Français", Interface = true, FullAudio = true, Subtitles = true },
                    new() { Language = "Polski", Interface = true, FullAudio = true, Subtitles = true }
                };

                var gamesToSave = new List<Game>();
                var reviewsToSave = new List<Review>();

                Game GetOrAddMainGame(
                    string title,
                    string description,
                    string shortDescription,
                    long price,
                    int discount,
                    string archivePath,
                    int downloads,
                    double rating,
                    int reviewsCount,
                    List<string> genres,
                    List<string> platforms,
                    List<string> features,
                    List<string> tags,
                    string version,
                    long sizeBytes,
                    string headerImg,
                    string coverImg,
                    List<string> screenshots,
                    string trailerUrl,
                    string reviewText)
                {
                    if (gamesByTitle.TryGetValue(title, out var existing))
                    {
                        existing.CoverImageUrl = coverImg;
                        existing.HeaderImageUrl = headerImg;
                        existing.ScreenshotUrls = screenshots;
                        existing.PriceInNanoTons = price;
                        existing.DiscountPercentage = discount;
                        existing.AverageRating = rating;
                        existing.ReviewsCount = reviewsCount;
                        existing.DownloadCount = downloads;
                        existing.Genres = genres;
                        existing.Platforms = platforms;
                        existing.Features = features;
                        existing.Tags = tags;
                        existing.Description = description;
                        existing.ShortDescription = shortDescription;
                        existing.IsDlc = false;
                        existing.ParentGameId = null;
                        existing.IsPublished = true;
                        existing.Version = version;
                        existing.SizeInBytes = sizeBytes;
                        existing.TrailerUrl = trailerUrl;
                        existing.RecalculateTasteVector();
                        context.Games.Update(existing);
                        return existing;
                    }

                    var g = new Game
                    {
                        Id = Guid.NewGuid(),
                        Title = title,
                        Description = description,
                        ShortDescription = shortDescription,
                        PriceInNanoTons = price,
                        DiscountPercentage = discount,
                        ServerArchivePath = archivePath,
                        OwnerId = adminUser.Id,
                        DownloadCount = downloads,
                        AverageRating = rating,
                        ReviewsCount = reviewsCount,
                        IsDlc = false,
                        ParentGameId = null,
                        Genres = genres,
                        Platforms = platforms,
                        Features = features,
                        SupportedLanguages = defaultLanguages,
                        Tags = tags,
                        Version = version,
                        SizeInBytes = sizeBytes,
                        IsPublished = true,
                        HeaderImageUrl = headerImg,
                        CoverImageUrl = coverImg,
                        ScreenshotUrls = screenshots,
                        TrailerUrl = trailerUrl,
                        CreatedAt = DateTime.UtcNow
                    };
                    g.RecalculateTasteVector();
                    context.Games.Add(g);
                    gamesByTitle[g.Title] = g;
                    gamesToSave.Add(g);

                    if (!string.IsNullOrWhiteSpace(reviewText))
                    {
                        reviewsToSave.Add(new Review
                        {
                            Id = Guid.NewGuid(),
                            UserId = adminUser.Id,
                            GameId = g.Id,
                            Rating = (int)Math.Round(rating),
                            Content = reviewText,
                            IsRecommended = true,
                            PlayTimeHoursAtReview = 24.5,
                            CreatedAt = DateTime.UtcNow
                        });
                    }

                    return g;
                }

                void GetOrAddDlc(
                    Guid parentGameId,
                    string title,
                    string description,
                    string shortDescription,
                    long price,
                    int discount,
                    string archivePath,
                    int downloads,
                    double rating,
                    int reviewsCount,
                    List<string> genres,
                    List<string> platforms,
                    string headerImg,
                    string coverImg)
                {
                    if (gamesByTitle.TryGetValue(title, out var existing))
                    {
                        existing.ParentGameId = parentGameId;
                        existing.CoverImageUrl = coverImg;
                        existing.HeaderImageUrl = headerImg;
                        existing.ScreenshotUrls = new List<string> { coverImg, headerImg };
                        existing.PriceInNanoTons = price;
                        existing.DiscountPercentage = discount;
                        existing.AverageRating = rating;
                        existing.ReviewsCount = reviewsCount;
                        existing.DownloadCount = downloads;
                        existing.Genres = genres;
                        existing.Platforms = platforms;
                        existing.Description = description;
                        existing.ShortDescription = shortDescription;
                        existing.IsDlc = true;
                        existing.IsPublished = true;
                        existing.RecalculateTasteVector();
                        context.Games.Update(existing);
                        return;
                    }

                    var dlc = new Game
                    {
                        Id = Guid.NewGuid(),
                        Title = title,
                        Description = description,
                        ShortDescription = shortDescription,
                        PriceInNanoTons = price,
                        DiscountPercentage = discount,
                        ServerArchivePath = archivePath,
                        OwnerId = adminUser.Id,
                        DownloadCount = downloads,
                        AverageRating = rating,
                        ReviewsCount = reviewsCount,
                        IsDlc = true,
                        ParentGameId = parentGameId,
                        Genres = genres,
                        Platforms = platforms,
                        SupportedLanguages = defaultLanguages,
                        IsPublished = true,
                        HeaderImageUrl = headerImg,
                        CoverImageUrl = coverImg,
                        ScreenshotUrls = new List<string> { coverImg, headerImg },
                        CreatedAt = DateTime.UtcNow
                    };
                    dlc.RecalculateTasteVector();
                    context.Games.Add(dlc);
                    gamesByTitle[dlc.Title] = dlc;
                    gamesToSave.Add(dlc);
                }

                var cp2077 = GetOrAddMainGame(
                    "Cyberpunk 2077",
                    "Cyberpunk 2077 — пригодницький бойовик і рольова гра з відкритим світом. Дія відбувається у темному майбутньому Найт-Сіті, небезпечного мегаполіса, одержимого владою, гламуром і ненаситною модифікацією тіла. Грайте за найманця V у пошуках унікального імплантату безсмертя.",
                    "Пригодницький рольовий екшн у відкритому світі майбутнього з глибоким сюжетом.",
                    5_000_000_000,
                    20,
                    "/storage/games/cyberpunk-2077.zip",
                    14200,
                    4.9,
                    4,
                    new List<string> { "Action", "RPG", "Cyberpunk", "SciFi" },
                    new List<string> { "Windows", "MacOS" },
                    new List<string> { "SinglePlayer", "Achievements", "FullControllerSupport", "CloudSaves" },
                    new List<string> { "шутер", "екшн", "кіберпанк", "відкритий світ", "майбутнє", "рольова гра" },
                    "2.1.0",
                    70L * 1024 * 1024 * 1024,
                    "https://images.unsplash.com/photo-1542751371-adc38448a05e?w=1200&auto=format&fit=crop&q=80",
                    "https://images.unsplash.com/photo-1618005182384-a83a8bd57fbe?w=800&auto=format&fit=crop&q=80",
                    new List<string>
                    {
                        "https://images.unsplash.com/photo-1550745165-9bc0b252726f?w=800&auto=format&fit=crop&q=80",
                        "https://images.unsplash.com/photo-1511512578047-dfb367046420?w=800&auto=format&fit=crop&q=80"
                    },
                    "https://www.youtube.com/watch?v=dQw4w9WgXcQ",
                    "Чудова гра! Неймовірна атмосфера, графіка та сюжет тримають у напрузі до останньої хвилини. Рекомендую всім!"
                );
                GetOrAddDlc(cp2077.Id, "Cyberpunk 2077: Phantom Liberty", "Масштабне сюжетне доповнення у жанрі шпигунського трилера до Cyberpunk 2077.", "Шпигунський трилер у Найт-Сіті.", 2_500_000_000, 0, "/storage/games/cyberpunk-phantom-liberty.zip", 11000, 5.0, 2, new List<string> { "Action", "RPG", "Cyberpunk", "DLC" }, new List<string> { "Windows", "MacOS" }, "https://images.unsplash.com/photo-1508739773434-c26b3d09e071?w=1200&auto=format&fit=crop&q=80", "https://images.unsplash.com/photo-1579783900882-c0d3dad7b119?w=800&auto=format&fit=crop&q=80");
                GetOrAddDlc(cp2077.Id, "Cyberpunk 2077: REDmod", "Безкоштовний інструмент для створення та завантаження модифікацій.", "Інструмент модифікацій.", 0, 0, "/storage/games/cyberpunk-redmod.zip", 8000, 5.0, 1, new List<string> { "DLC", "Modding" }, new List<string> { "Windows" }, "https://images.unsplash.com/photo-1550745165-9bc0b252726f?w=1200&auto=format&fit=crop&q=80", "https://images.unsplash.com/photo-1511512578047-dfb367046420?w=800&auto=format&fit=crop&q=80");

                var witcher3 = GetOrAddMainGame(
                    "The Witcher 3: Wild Hunt",
                    "Ви Ґеральт із Рівії, найманий убивця чудовиськ. Перед вами спустошений війною, заповнений монстрами континент, який ви можете досліджувати за своїм бажанням. Ваш поточний контракт? Знайти Цірі — Дитя Приреченості, живу зброю, яка може змінити форму світу.",
                    "Сюжетна рольова гра у відкритому фентезійному всесвіті з незабутніми пригодами.",
                    4_000_000_000,
                    70,
                    "/storage/games/witcher3.zip",
                    38500,
                    4.95,
                    12,
                    new List<string> { "RPG", "Fantasy", "Action", "Adventure" },
                    new List<string> { "Windows", "MacOS" },
                    new List<string> { "SinglePlayer", "Achievements", "FullControllerSupport", "CloudSaves" },
                    new List<string> { "рольова гра", "відкритий світ", "фентезі", "магія", "мечі", "шедевр", "сюжет" },
                    "4.04",
                    50L * 1024 * 1024 * 1024,
                    "https://images.unsplash.com/photo-1518709268805-4e9042af9f23?w=1200&auto=format&fit=crop&q=80",
                    "https://images.unsplash.com/photo-1579373903781-fd5c0c30c4cd?w=800&auto=format&fit=crop&q=80",
                    new List<string>
                    {
                        "https://images.unsplash.com/photo-1538481199705-c710c4e965fc?w=800&auto=format&fit=crop&q=80",
                        "https://images.unsplash.com/photo-1511512578047-dfb367046420?w=800&auto=format&fit=crop&q=80"
                    },
                    "https://www.youtube.com/watch?v=c0i88t0Kacs",
                    "Одна з найкращих RPG в історії! Неймовірний сюжет, живі персонажі та прекрасна музика."
                );
                GetOrAddDlc(witcher3.Id, "The Witcher 3: Hearts of Stone", "Знову станьте Ґеральтом із Рівії, щоб виконати контракт для безжального капітана бандитів Ольґерда фон Еверека.", "Перше велике сюжетне доповнення для гри The Witcher 3.", 1_200_000_000, 30, "/storage/games/witcher3-hos.zip", 14000, 4.9, 3, new List<string> { "RPG", "Fantasy", "DLC" }, new List<string> { "Windows", "MacOS" }, "https://images.unsplash.com/photo-1514533450685-4493e01d1fdc?w=1200&auto=format&fit=crop&q=80", "https://images.unsplash.com/photo-1563089145-599997674d42?w=800&auto=format&fit=crop&q=80");
                GetOrAddDlc(witcher3.Id, "The Witcher 3: Blood and Wine", "Вирушайте до далекої та залитої сонцем землі Туссент, де безтурботне життя ховає страшну та криваву таємницю.", "Масштабне доповнення на 30+ годин у новому регіоні Туссент.", 1_800_000_000, 30, "/storage/games/witcher3-baw.zip", 19000, 5.0, 5, new List<string> { "RPG", "Fantasy", "Adventure", "DLC" }, new List<string> { "Windows", "MacOS" }, "https://images.unsplash.com/photo-1506744038136-46273834b3fb?w=1200&auto=format&fit=crop&q=80", "https://images.unsplash.com/photo-1534447677768-be436bb09401?w=800&auto=format&fit=crop&q=80");

                var eldenRing = GetOrAddMainGame(
                    "Elden Ring",
                    "Золотий Порядок був порушений. Повстаньте, згаслі, і нехай благодать веде вас, щоб здобути силу Кільця Елден і стати Повелителем Елдену у Межизем'ї. Неперевершений шедевр від Hidetaka Miyazaki та George R. R. Martin.",
                    "Епічна рольова екшн-гра у величній фентезійній реальності Межизем'я.",
                    6_000_000_000,
                    0,
                    "/storage/games/elden-ring.zip",
                    52000,
                    4.9,
                    16,
                    new List<string> { "RPG", "Fantasy", "Action" },
                    new List<string> { "Windows" },
                    new List<string> { "SinglePlayer", "MultiPlayer", "CoOp", "Achievements", "FullControllerSupport" },
                    new List<string> { "souls-like", "відкритий світ", "темне фентезі", "складний екшн", "магія", "боси" },
                    "1.12",
                    60L * 1024 * 1024 * 1024,
                    "https://images.unsplash.com/photo-1538481199705-c710c4e965fc?w=1200&auto=format&fit=crop&q=80",
                    "https://images.unsplash.com/photo-1563089145-599997674d42?w=800&auto=format&fit=crop&q=80",
                    new List<string>
                    {
                        "https://images.unsplash.com/photo-1514533450685-4493e01d1fdc?w=800&auto=format&fit=crop&q=80",
                        "https://images.unsplash.com/photo-1518709268805-4e9042af9f23?w=800&auto=format&fit=crop&q=80"
                    },
                    "https://www.youtube.com/watch?v=E3Huy2cdih0",
                    "Неймовірний відкритий світ і незабутні битви з босами. Справжнє мистецтво геймдизайну."
                );
                GetOrAddDlc(eldenRing.Id, "Elden Ring: Shadow of the Erdtree", "Вирушайте в Землі Тіні слідами Мікелли та розкрийте темні таємниці світового порядку.", "Найбільше сюжетне доповнення для Elden Ring.", 3_800_000_000, 0, "/storage/games/elden-ring-erdtree.zip", 22000, 4.8, 6, new List<string> { "RPG", "Fantasy", "Action", "DLC" }, new List<string> { "Windows" }, "https://images.unsplash.com/photo-1519681393784-d120267933ba?w=1200&auto=format&fit=crop&q=80", "https://images.unsplash.com/photo-1518709268805-4e9042af9f23?w=800&auto=format&fit=crop&q=80");
                GetOrAddDlc(eldenRing.Id, "Elden Ring: Colosseum & Soundtrack", "Офіційне доповнення із PvP-аренами Колізеїв та цифровим саундтреком.", "PvP-арени Колізеїв та офіційний саундтрек.", 800_000_000, 0, "/storage/games/elden-ring-colosseum.zip", 14000, 4.8, 3, new List<string> { "RPG", "Action", "DLC" }, new List<string> { "Windows" }, "https://images.unsplash.com/photo-1511671782779-c97d3d27a1d4?w=1200&auto=format&fit=crop&q=80", "https://images.unsplash.com/photo-1470225620780-dba8ba36b745?w=800&auto=format&fit=crop&q=80");

                var doom = GetOrAddMainGame(
                    "DOOM Eternal",
                    "Армії пекла заполонили Землю. Станьте Катом Року та зупиніть демонічне вторгнення у всіх вимірах. Відчуйте абсолютну швидкість і силу у передовому шутері від першої особи.",
                    "Неперевершений динамічний шутер від id Software з потужним саундтреком.",
                    3_500_000_000,
                    67,
                    "/storage/games/doom-eternal.zip",
                    29000,
                    4.85,
                    8,
                    new List<string> { "Shooter", "Action", "Horror", "SciFi" },
                    new List<string> { "Windows" },
                    new List<string> { "SinglePlayer", "MultiPlayer", "Achievements", "FullControllerSupport" },
                    new List<string> { "шутер", "швидкий бій", "демони", "фантастика", "кров", "важкий метал", "адреналін" },
                    "6.66",
                    80L * 1024 * 1024 * 1024,
                    "https://images.unsplash.com/photo-1509198397868-475647b2a1e5?w=1200&auto=format&fit=crop&q=80",
                    "https://images.unsplash.com/photo-1563245372-f21724e3856d?w=800&auto=format&fit=crop&q=80",
                    new List<string> { "https://images.unsplash.com/photo-1550745165-9bc0b252726f?w=800&auto=format&fit=crop&q=80" },
                    "https://www.youtube.com/watch?v=_NOJ2b_k2yo",
                    "Безперервний адреналіновий екшн і неймовірна музика Міка Гордона. Чистий драйв!"
                );
                GetOrAddDlc(doom.Id, "DOOM Eternal: The Ancient Gods - Part One", "Ваша війна проти демонів триває. Здолайте нові загрози у найвищих небесних сферах та безоднях.", "Перше сюжетне доповнення найвищої складності.", 1_800_000_000, 50, "/storage/games/doom-ancient-gods-1.zip", 9500, 4.7, 2, new List<string> { "Shooter", "Action", "SciFi", "DLC" }, new List<string> { "Windows" }, "https://images.unsplash.com/photo-1518709268805-4e9042af9f23?w=1200&auto=format&fit=crop&q=80", "https://images.unsplash.com/photo-1509198397868-475647b2a1e5?w=800&auto=format&fit=crop&q=80");
                GetOrAddDlc(doom.Id, "DOOM Eternal: The Ancient Gods - Part Two", "Кульмінація саги про Ката Року. Штурмуйте цитадель Темного Володаря в Іммфорі.", "Фінальне епічне протистояння з владикою Пекла.", 1_800_000_000, 50, "/storage/games/doom-ancient-gods-2.zip", 8900, 4.75, 2, new List<string> { "Shooter", "Action", "SciFi", "DLC" }, new List<string> { "Windows" }, "https://images.unsplash.com/photo-1579783900882-c0d3dad7b119?w=1200&auto=format&fit=crop&q=80", "https://images.unsplash.com/photo-1550745165-9bc0b252726f?w=800&auto=format&fit=crop&q=80");

                var civ6 = GetOrAddMainGame(
                    "Sid Meier's Civilization VI",
                    "Будуйте імперію, яка витримає випробування часом. Досліджуйте нові землі, відкривайте технології, перемагайте ворогів та ведіть свій народ від кам'яного віку до космічної ери.",
                    "Легендарна покрокова глобальна стратегія світового рівня.",
                    3_000_000_000,
                    85,
                    "/storage/games/civ6.zip",
                    41000,
                    4.7,
                    9,
                    new List<string> { "Strategy", "Simulation" },
                    new List<string> { "Windows", "MacOS", "Linux" },
                    new List<string> { "SinglePlayer", "MultiPlayer", "CoOp", "CloudSaves" },
                    new List<string> { "стратегія", "покрокова", "історія", "дипломатія", "тактика", "симулятор імперії" },
                    "1.0.12",
                    25L * 1024 * 1024 * 1024,
                    "https://images.unsplash.com/photo-1618172193763-c511deb635ca?w=1200&auto=format&fit=crop&q=80",
                    "https://images.unsplash.com/photo-1511512578047-dfb367046420?w=800&auto=format&fit=crop&q=80",
                    new List<string> { "https://images.unsplash.com/photo-1542751371-adc38448a05e?w=800&auto=format&fit=crop&q=80" },
                    "https://www.youtube.com/watch?v=5KdE0p2joJw",
                    "Ще один хід — і вже ранок! Чудова покрокова стратегія для довгих вечорів."
                );
                GetOrAddDlc(civ6.Id, "Civilization VI: Gathering Storm", "Світ стає живим: кліматичні зміни, стихійні лиха, енергетичні ресурси та Світовий конгрес.", "Глобальне доповнення з новими механіками екології та дипломатії.", 1_500_000_000, 70, "/storage/games/civ6-gs.zip", 16000, 4.8, 3, new List<string> { "Strategy", "Simulation", "DLC" }, new List<string> { "Windows", "MacOS", "Linux" }, "https://images.unsplash.com/photo-1527489377706-5bf97e608852?w=1200&auto=format&fit=crop&q=80", "https://images.unsplash.com/photo-1534447677768-be436bb09401?w=800&auto=format&fit=crop&q=80");
                GetOrAddDlc(civ6.Id, "Civilization VI: Rise and Fall", "Епохи розквіту й темряви, вірність міст та нова система губернаторів.", "Доповнення про піднесення та занепад цивілізацій.", 1_200_000_000, 70, "/storage/games/civ6-rf.zip", 14000, 4.7, 2, new List<string> { "Strategy", "Simulation", "DLC" }, new List<string> { "Windows", "MacOS", "Linux" }, "https://images.unsplash.com/photo-1451187580459-43490279c0fa?w=1200&auto=format&fit=crop&q=80", "https://images.unsplash.com/photo-1506744038136-46273834b3fb?w=800&auto=format&fit=crop&q=80");

                var fh5 = GetOrAddMainGame(
                    "Forza Horizon 5",
                    "Ваша неперевершена пригода Horizon починається! Досліджуйте яскраві пейзажі Мексики у безперервному русі на сотнях найкращих автомобілів світу.",
                    "Найкращий відкритий світ перегонів сучасності з фотореалістичною графікою.",
                    5_500_000_000,
                    35,
                    "/storage/games/fh5.zip",
                    47000,
                    4.8,
                    10,
                    new List<string> { "Racing", "Sports", "Simulation" },
                    new List<string> { "Windows" },
                    new List<string> { "SinglePlayer", "MultiPlayer", "CoOp", "FullControllerSupport" },
                    new List<string> { "перегони", "спорт", "відкритий світ", "автомобілі", "мультиплеєр", "швидкість" },
                    "1.650",
                    110L * 1024 * 1024 * 1024,
                    "https://images.unsplash.com/photo-1503376780353-7e6692767b70?w=1200&auto=format&fit=crop&q=80",
                    "https://images.unsplash.com/photo-1503376780353-7e6692767b70?w=800&auto=format&fit=crop&q=80",
                    new List<string> { "https://images.unsplash.com/photo-1568605117036-5fe5e7bab0b7?w=800&auto=format&fit=crop&q=80" },
                    "https://www.youtube.com/watch?v=FYH9n37B7Yw",
                    "Приголомшлива графіка і фізика керування авто. Найкраща автогонка десятиліття."
                );
                GetOrAddDlc(fh5.Id, "Forza Horizon 5: Hot Wheels", "Злітайте у небеса на запаморочливих помаранчевих треках Hot Wheels у хмарах над Мексикою.", "Шалені петлі та екстремальні треки Hot Wheels.", 1_800_000_000, 20, "/storage/games/fh5-hotwheels.zip", 18000, 4.85, 4, new List<string> { "Racing", "Sports", "DLC" }, new List<string> { "Windows" }, "https://images.unsplash.com/photo-1511919884226-fd3cad34687c?w=1200&auto=format&fit=crop&q=80", "https://images.unsplash.com/photo-1542282088-72c9c27ed0cd?w=800&auto=format&fit=crop&q=80");
                GetOrAddDlc(fh5.Id, "Forza Horizon 5: Rally Adventure", "Підкорюйте бездоріжжя Сьєрра-Нуева на спеціально підготовлених ралійних болідах.", "Справжнє ралі з ґрунтовими трасами та пилом.", 1_800_000_000, 20, "/storage/games/fh5-rally.zip", 13000, 4.7, 2, new List<string> { "Racing", "Sports", "DLC" }, new List<string> { "Windows" }, "https://images.unsplash.com/photo-1568605117036-5fe5e7bab0b7?w=1200&auto=format&fit=crop&q=80", "https://images.unsplash.com/photo-1492144534655-ae79c964c9d7?w=800&auto=format&fit=crop&q=80");

                var re4 = GetOrAddMainGame(
                    "Resident Evil 4 Remake",
                    "Виживання — це лише перший крок. Агент Леон С. Кеннеді вирушає до глухого європейського селища, щоб урятувати викрадену доньку президента, але стикається з жахом культу Лос-Ілюмінадос.",
                    "Переосмислення легендарного шедевра жахів на виживання від Capcom.",
                    4_500_000_000,
                    25,
                    "/storage/games/re4-remake.zip",
                    36000,
                    4.9,
                    11,
                    new List<string> { "Horror", "Action", "Adventure" },
                    new List<string> { "Windows", "MacOS" },
                    new List<string> { "SinglePlayer", "Achievements", "FullControllerSupport" },
                    new List<string> { "жахи", "виживання", "зомбі", "трилер", "екшн", "кінематографічно", "напруга" },
                    "1.10",
                    65L * 1024 * 1024 * 1024,
                    "https://images.unsplash.com/photo-1509198397868-475647b2a1e5?w=1200&auto=format&fit=crop&q=80",
                    "https://images.unsplash.com/photo-1518709268805-4e9042af9f23?w=800&auto=format&fit=crop&q=80",
                    new List<string> { "https://images.unsplash.com/photo-1500648767791-00dcc994a43e?w=800&auto=format&fit=crop&q=80" },
                    "https://www.youtube.com/watch?v=j5Ic2z3_xP8",
                    "Ідеальний ремейк. Атмосфера жаху та напружені перестрілки тримають до фінальних титрів."
                );
                GetOrAddDlc(re4.Id, "Resident Evil 4: Separate Ways", "Дізнайтеся інший бік історії очима загадкової Ади Вонг під час її таємної місії.", "Сюжетна кампанія Ади Вонг із крюком-кішкою.", 1_200_000_000, 0, "/storage/games/re4-separate-ways.zip", 15000, 4.95, 3, new List<string> { "Horror", "Action", "DLC" }, new List<string> { "Windows", "MacOS" }, "https://images.unsplash.com/photo-1579783900882-c0d3dad7b119?w=1200&auto=format&fit=crop&q=80", "https://images.unsplash.com/photo-1508739773434-c26b3d09e071?w=800&auto=format&fit=crop&q=80");
                GetOrAddDlc(re4.Id, "Resident Evil 4: Extra DLC Pack", "Комплект спеціальних костюмів, фільтрів та ексклюзивної зброї для Леона та Ешлі.", "Набір ексклюзивного спорядження та стилів.", 700_000_000, 0, "/storage/games/re4-extra-dlc.zip", 12000, 4.8, 2, new List<string> { "Horror", "Action", "DLC" }, new List<string> { "Windows", "MacOS" }, "https://images.unsplash.com/photo-1500648767791-00dcc994a43e?w=1200&auto=format&fit=crop&q=80", "https://images.unsplash.com/photo-1563245372-f21724e3856d?w=800&auto=format&fit=crop&q=80");

                var bg3 = GetOrAddMainGame(
                    "Baldur's Gate 3",
                    "Зберіть загін і поверніться у Забуті Королівства. Вас заразили личинкою іллітіда, яка наділяє вас неймовірною силою, але вимагає страшної ціни. Обирайте між добром і злом у найкращій RPG десятиліття.",
                    "Грандіозна кінематографічна рольова гра нового покоління за системою D&D.",
                    6_000_000_000,
                    15,
                    "/storage/games/bg3.zip",
                    75000,
                    4.98,
                    20,
                    new List<string> { "RPG", "Fantasy", "Strategy", "Adventure" },
                    new List<string> { "Windows", "MacOS" },
                    new List<string> { "SinglePlayer", "MultiPlayer", "CoOp", "Achievements", "FullControllerSupport" },
                    new List<string> { "D&D", "рольова гра", "магія", "покрокові бої", "вибір має значення", "кооператив", "шедевр" },
                    "4.1.1",
                    125L * 1024 * 1024 * 1024,
                    "https://images.unsplash.com/photo-1563089145-599997674d42?w=1200&auto=format&fit=crop&q=80",
                    "https://images.unsplash.com/photo-1579783900882-c0d3dad7b119?w=800&auto=format&fit=crop&q=80",
                    new List<string> { "https://images.unsplash.com/photo-1538481199705-c710c4e965fc?w=800&auto=format&fit=crop&q=80" },
                    "https://www.youtube.com/watch?v=1T22wN1BIzU",
                    "Абсолютна свобода дій та неймовірно прописані супутники. Гра року!"
                );
                GetOrAddDlc(bg3.Id, "Baldur's Gate 3: Digital Deluxe Upgrade", "Унікальні бардичні пісні, артефакти з всесвіту Divinity, цифровий артбук та саундтрек.", "Колекційний набір бонусних матеріалів.", 1_000_000_000, 0, "/storage/games/bg3-deluxe.zip", 21000, 4.9, 3, new List<string> { "RPG", "Fantasy", "DLC" }, new List<string> { "Windows", "MacOS" }, "https://images.unsplash.com/photo-1518709268805-4e9042af9f23?w=1200&auto=format&fit=crop&q=80", "https://images.unsplash.com/photo-1538481199705-c710c4e965fc?w=800&auto=format&fit=crop&q=80");
                GetOrAddDlc(bg3.Id, "Baldur's Gate 3: Adventure Companion Pack", "Додаткові косметичні набори, ігрові кубики та унікальні діалогові портрети.", "Набір кастомізації персонажів та кубиків.", 600_000_000, 0, "/storage/games/bg3-companion.zip", 16000, 4.85, 2, new List<string> { "RPG", "Fantasy", "DLC" }, new List<string> { "Windows", "MacOS" }, "https://images.unsplash.com/photo-1514533450685-4493e01d1fdc?w=1200&auto=format&fit=crop&q=80", "https://images.unsplash.com/photo-1579373903781-fd5c0c30c4cd?w=800&auto=format&fit=crop&q=80");

                var deadCells = GetOrAddMainGame(
                    "Dead Cells",
                    "Dead Cells — це roguelite-метроїдванія у мінливому замку. Відсутність контрольних точок: убивайте, помирайте, вчіться на помилках і повторюйте знову в шаленому ритмі.",
                    "Динамічний екшн-платформер із безкінечною реіграбельністю.",
                    2_000_000_000,
                    40,
                    "/storage/games/dead-cells.zip",
                    33000,
                    4.85,
                    7,
                    new List<string> { "Indie", "Action", "Fantasy" },
                    new List<string> { "Windows", "MacOS", "Linux" },
                    new List<string> { "SinglePlayer", "Achievements", "FullControllerSupport" },
                    new List<string> { "рогалик", "метроїдванія", "піксель-арт", "складний бій", "зброя", "швидкий темп" },
                    "35.0",
                    5L * 1024 * 1024 * 1024,
                    "https://images.unsplash.com/photo-1550745165-9bc0b252726f?w=1200&auto=format&fit=crop&q=80",
                    "https://images.unsplash.com/photo-1618005182384-a83a8bd57fbe?w=800&auto=format&fit=crop&q=80",
                    new List<string> { "https://images.unsplash.com/photo-1607604276583-eef5d076aa5f?w=800&auto=format&fit=crop&q=80" },
                    "https://www.youtube.com/watch?v=rvXZQn44AAU",
                    "Неймовірна динаміка та різноманіття зброї. Відірватися неможливо."
                );
                GetOrAddDlc(deadCells.Id, "Dead Cells: Return to Castlevania", "Легендарний кросовер: бийтеся пліч-о-пліч з Алукардом і Ріхтером Белмонтом у залах замку Дракули.", "Масштабне доповнення за мотивами всесвіту Castlevania.", 900_000_000, 20, "/storage/games/dead-cells-castlevania.zip", 17000, 4.95, 4, new List<string> { "Indie", "Action", "Fantasy", "DLC" }, new List<string> { "Windows", "MacOS", "Linux" }, "https://images.unsplash.com/photo-1607604276583-eef5d076aa5f?w=1200&auto=format&fit=crop&q=80", "https://images.unsplash.com/photo-1563245372-f21724e3856d?w=800&auto=format&fit=crop&q=80");
                GetOrAddDlc(deadCells.Id, "Dead Cells: The Bad Seed", "Досліджуйте розслаблене Дендропарк, боріться з новими мутантами у болотах та киньте виклик Матері Кліщів.", "Доповнення з новими біомами та зброєю.", 700_000_000, 20, "/storage/games/dead-cells-bad-seed.zip", 13000, 4.8, 2, new List<string> { "Indie", "Action", "DLC" }, new List<string> { "Windows", "MacOS", "Linux" }, "https://images.unsplash.com/photo-1518709268805-4e9042af9f23?w=1200&auto=format&fit=crop&q=80", "https://images.unsplash.com/photo-1511512578047-dfb367046420?w=800&auto=format&fit=crop&q=80");

                var subnautica = GetOrAddMainGame(
                    "Subnautica",
                    "Здійсніть вимушену посадку на невідомій океанічній планеті 4546B. Створюйте спорядження, будуйте підводні бази та субмарини, щоб досліджувати коралові рифи та глибоководні розломи.",
                    "Захоплива підводна пригода на виживання в чужому інопланетному океані.",
                    2_800_000_000,
                    50,
                    "/storage/games/subnautica.zip",
                    39000,
                    4.85,
                    10,
                    new List<string> { "Adventure", "SciFi", "Simulation" },
                    new List<string> { "Windows", "MacOS" },
                    new List<string> { "SinglePlayer", "Achievements", "FullControllerSupport", "VR" },
                    new List<string> { "під водою", "виживання", "крафтинг", "океан", "інопланетяни", "дослідження", "Sci-Fi" },
                    "2.0.1",
                    20L * 1024 * 1024 * 1024,
                    "https://images.unsplash.com/photo-1682687220063-4742bd7fd538?w=1200&auto=format&fit=crop&q=80",
                    "https://images.unsplash.com/photo-1544551763-46a013bb70d5?w=800&auto=format&fit=crop&q=80",
                    new List<string> { "https://images.unsplash.com/photo-1550745165-9bc0b252726f?w=800&auto=format&fit=crop&q=80" },
                    "https://www.youtube.com/watch?v=Rz2SNm8VguE",
                    "Неймовірне відчуття першовідкривача та легкий страх глибини. Чудовий підводний світ!"
                );
                GetOrAddDlc(subnautica.Id, "Subnautica: Below Zero", "Зануртеся в крижану експедицію на арктичному регіоні планети 4546B.", "Окрема арктична історія у всесвіті Subnautica.", 2_400_000_000, 30, "/storage/games/subnautica-bz.zip", 21000, 4.7, 5, new List<string> { "Adventure", "SciFi", "Simulation", "DLC" }, new List<string> { "Windows", "MacOS" }, "https://images.unsplash.com/photo-1483921020237-2ff51e8e4b22?w=1200&auto=format&fit=crop&q=80", "https://images.unsplash.com/photo-1517824806704-9040b037703b?w=800&auto=format&fit=crop&q=80");
                GetOrAddDlc(subnautica.Id, "Subnautica: Deep Sea Pack", "Додаткові креслення підводних модулів та екзокостюмів для глибоководних занурень.", "Набір глибоководного спорядження та креслень.", 900_000_000, 0, "/storage/games/subnautica-deepsea.zip", 14000, 4.8, 3, new List<string> { "Adventure", "SciFi", "DLC" }, new List<string> { "Windows", "MacOS" }, "https://images.unsplash.com/photo-1544551763-46a013bb70d5?w=1200&auto=format&fit=crop&q=80", "https://images.unsplash.com/photo-1550745165-9bc0b252726f?w=800&auto=format&fit=crop&q=80");

                if (reviewsToSave.Count > 0)
                {
                    await context.Reviews.AddRangeAsync(reviewsToSave);
                }

                await context.SaveChangesAsync();
                _logger?.LogInformation($"[InitData] Seeded/verified {gamesByTitle.Count} games and DLCs with recalculated recommendation vectors.");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "[InitData] Error ensuring seeded games and DLCs.");
            }
        }
    }
}
