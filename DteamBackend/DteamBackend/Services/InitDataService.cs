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
            try
            {
                await context.Database.ExecuteSqlRawAsync(@"
                    DROP TABLE IF EXISTS ""ChatUploads"";
                    DROP TABLE IF EXISTS ""ChatMessages"";

                    CREATE TABLE IF NOT EXISTS ""ChatMessages"" (
                        ""Id"" TEXT NOT NULL CONSTRAINT ""PK_ChatMessages"" PRIMARY KEY,
                        ""ClientMessageId"" TEXT NOT NULL,
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
                        ""IsDeletedForSender"" INTEGER NOT NULL,
                        ""IsDeletedForReceiver"" INTEGER NOT NULL,
                        CONSTRAINT ""FK_ChatMessages_Users_SenderId"" FOREIGN KEY (""SenderId"") REFERENCES ""Users"" (""Id"") ON DELETE RESTRICT,
                        CONSTRAINT ""FK_ChatMessages_Users_ReceiverId"" FOREIGN KEY (""ReceiverId"") REFERENCES ""Users"" (""Id"") ON DELETE RESTRICT
                    );

                    CREATE TABLE IF NOT EXISTS ""ChatUploads"" (
                        ""Id"" TEXT NOT NULL CONSTRAINT ""PK_ChatUploads"" PRIMARY KEY,
                        ""UserId"" TEXT NOT NULL,
                        ""MessageId"" TEXT NULL,
                        ""StorageKey"" TEXT NOT NULL,
                        ""OriginalFileName"" TEXT NOT NULL,
                        ""ContentType"" TEXT NOT NULL,
                        ""FileSize"" INTEGER NOT NULL,
                        ""Duration"" INTEGER NULL,
                        ""CreatedAt"" INTEGER NOT NULL,
                        ""IsCommitted"" INTEGER NOT NULL,
                        CONSTRAINT ""FK_ChatUploads_Users_UserId"" FOREIGN KEY (""UserId"") REFERENCES ""Users"" (""Id"") ON DELETE CASCADE,
                        CONSTRAINT ""FK_ChatUploads_ChatMessages_MessageId"" FOREIGN KEY (""MessageId"") REFERENCES ""ChatMessages"" (""Id"") ON DELETE SET NULL
                    );

                    CREATE UNIQUE INDEX IF NOT EXISTS ""IX_ChatMessages_SenderId_ClientMessageId"" ON ""ChatMessages"" (""SenderId"", ""ClientMessageId"");
                    CREATE INDEX IF NOT EXISTS ""IX_ChatMessages_SenderId_ReceiverId_CreatedAt"" ON ""ChatMessages"" (""SenderId"", ""ReceiverId"", ""CreatedAt"");
                    CREATE INDEX IF NOT EXISTS ""IX_ChatMessages_ReceiverId_SenderId_CreatedAt"" ON ""ChatMessages"" (""ReceiverId"", ""SenderId"", ""CreatedAt"");
                    CREATE INDEX IF NOT EXISTS ""IX_ChatMessages_ReceiverId_Status_CreatedAt"" ON ""ChatMessages"" (""ReceiverId"", ""Status"", ""CreatedAt"");
                    CREATE INDEX IF NOT EXISTS ""IX_ChatUploads_UserId"" ON ""ChatUploads"" (""UserId"");
                ");
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Error ensuring chat tables exist.");
            }

            PasswordHasher.CreatePasswordHash("admin123321", out string passwordHash, out string passwordSalt);

            var adminUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "adim@gmail.com");
            if (adminUser == null)
            {
                adminUser = new Duser
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
                await context.SaveChangesAsync();
            }

            if (!await context.Games.AnyAsync())
            {
                var mainGameId = Guid.NewGuid();
                var mainGame = new Game
                {
                    Id = mainGameId,
                    Title = "Cyberpunk 2077",
                    Description = "Cyberpunk 2077 — пригодницький бойовик і рольова гра з відкритим світом. Дія відбувається у темному майбутньому Найт-Сіті, небезпечного мегаполіса, одержимого владою, гламуром і ненаситною модифікацією тіла.",
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

                await context.Games.AddAsync(mainGame);
                await context.SaveChangesAsync();
            }

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

            var friendUsers = new List<Duser>();
            foreach (var item in demoUsersData)
            {
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
                        Status = FriendshipStatus.Accepted,
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
                        Status = FriendshipStatus.Accepted,
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
                    // 1. MrsZubarikessa conversation (with text, photo, file, voice)
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
    }
}
