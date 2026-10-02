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
                await EnsureDefaultAdminsAsync(context);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "[InitData] Error in EnsureDefaultAdminsAsync");
            }

            try
            {
                await EnsureDemoUsersAsync(context);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "[InitData] Error in EnsureDemoUsersAsync");
            }

            try
            {
                await EnsureGamesSeededAsync(context);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "[InitData] Error in EnsureGamesSeededAsync");
            }

            try
            {
                await EnsureGameReviewsAndCommentsAsync(context);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "[InitData] Error in EnsureGameReviewsAndCommentsAsync");
            }

            try
            {
                await EnsureCommunityDataAsync(context);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "[InitData] Error in EnsureCommunityDataAsync");
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

            var secondAdminUser = new Duser
            {
                Id = Guid.NewGuid(),
                Email = "admin@gmail.com",
                Username = "admin",
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                WalletAddress = "EQB_v2Admin_SampleTonWalletAddress_888",
                HardhatAddress = "0x70997970C51812dc3A010C7d01b50e0d17dc79C8",
                BalanceInNanoTons = 100_000_000_000,
                TotalEarningsInNanoTons = 0,
                CreatedAt = DateTime.UtcNow,
                IsAdmin = true,
                IsBanned = false,
                Status = UserStatus.Online,
                AvatarUrl = "https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?w=500&auto=format&fit=crop&q=80",
                Bio = "Dteam Lead Administrator"
            };

            await context.Users.AddAsync(secondAdminUser);

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

            await EnsureDefaultAdminsAsync(context);
            await EnsureDemoUsersAsync(context);
            await EnsureGamesSeededAsync(context);
            await EnsureGameReviewsAndCommentsAsync(context);
            await EnsureCommunityDataAsync(context);
        }

        public async Task InitializeAsync()
        {
            if (_context != null)
            {
                await InitializeAsync(_context);
            }
        }

        public async Task EnsureDemoUsersAsync(AppDbContext context)
        {
            try
            {
                PasswordHasher.CreatePasswordHash("admin123321", out string passwordHash, out string passwordSalt);

                var adminUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "adim@gmail.com" || u.Username == "adim")
                    ?? await context.Users.FirstOrDefaultAsync(u => u.IsAdmin);

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

                if (adminUser != null)
                {
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
                }

                _logger?.LogInformation("[InitData] Demo users verified and friendships ensured.");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "[InitData] Error ensuring demo users.");
            }
        }

        private class SeedCommentReplyDef
        {
            public string Username { get; set; } = string.Empty;
            public string Content { get; set; } = string.Empty;
            public int HoursAfter { get; set; } = 4;
            public int LikesCount { get; set; } = 2;
        }

        private class SeedReviewDef
        {
            public string Username { get; set; } = string.Empty;
            public int Rating { get; set; } = 5;
            public bool IsRecommended { get; set; } = true;
            public double PlayTimeHours { get; set; } = 25.0;
            public string Content { get; set; } = string.Empty;
            public int DaysAgo { get; set; } = 10;
            public List<string> UpvotedByUsernames { get; set; } = new();
            public List<SeedCommentReplyDef> Replies { get; set; } = new();
        }

        public async Task EnsureGameReviewsAndCommentsAsync(AppDbContext context)
        {
            try
            {
                var users = await context.Users.ToListAsync();
                var usersByName = users.ToDictionary(u => u.Username, u => u, StringComparer.OrdinalIgnoreCase);

                var games = await context.Games.Where(g => !g.IsDlc).ToListAsync();
                var gamesByTitle = games.ToDictionary(g => g.Title, g => g, StringComparer.OrdinalIgnoreCase);

                var reviewData = new Dictionary<string, List<SeedReviewDef>>(StringComparer.OrdinalIgnoreCase)
                {
                    ["Cyberpunk 2077"] = new()
                    {
                        new SeedReviewDef
                        {
                            Username = "MrsZubarikessa",
                            Rating = 5,
                            IsRecommended = true,
                            PlayTimeHours = 84.5,
                            DaysAgo = 18,
                            Content = "Після патчу 2.0 та доповнення Phantom Liberty це одна з найкращих ігор у моєму житті! Бойова система стала динамічною, імпланти дають відчуття справжнього кіборга, а сюжет Ідріса Ельби та Сойки просто розриває серце.",
                            UpvotedByUsernames = new() { "DragonSlayer", "FirePhoenix", "SilentAssassin" },
                            Replies = new()
                            {
                                new() { Username = "DragonSlayer", Content = "Повністю згоден щодо Сойки, фінал в аеропорту — один із найсильніших моментів в історії CDPR!", HoursAfter = 6, LikesCount = 4 },
                                new() { Username = "SilentAssassin", Content = "А білд через сандевістан з катаною взагалі не залишає ворогам шансів. Кайф!", HoursAfter = 14, LikesCount = 2 }
                            }
                        },
                        new SeedReviewDef
                        {
                            Username = "FirePhoenix",
                            Rating = 5,
                            IsRecommended = true,
                            PlayTimeHours = 67.2,
                            DaysAgo = 12,
                            Content = "Неймовірне занурення у світ майбутнього. Дизайн Найт-Сіті настільки деталізований, що я провів перші 20 годин просто гуляючи провулками та слухаючи розмови мешканців.",
                            UpvotedByUsernames = new() { "TitanCrusher", "BlazingArrow" },
                            Replies = new()
                            {
                                new() { Username = "TitanCrusher", Content = "Також обожнюю без швидких переміщень їздити на мотоциклі під дощем під радіо Night FM.", HoursAfter = 8, LikesCount = 3 }
                            }
                        },
                        new SeedReviewDef
                        {
                            Username = "BlazingArrow",
                            Rating = 5,
                            IsRecommended = true,
                            PlayTimeHours = 52.0,
                            DaysAgo = 7,
                            Content = "Шедевр кіберпанк-естетики. Сюжет тримає в напрузі, музика геніальна, а графіка з трасуванням променів випереджає свій час на роки вперед.",
                            UpvotedByUsernames = new() { "sinichka_bez_egg", "MrsZubarikessa" }
                        },
                        new SeedReviewDef
                        {
                            Username = "sinichka_bez_egg",
                            Rating = 4,
                            IsRecommended = true,
                            PlayTimeHours = 39.8,
                            DaysAgo = 3,
                            Content = "Чудова гра, але вимагає потужного заліза для повної краси. На моєму сетапі все літає, оптимізація в останніх патчах нарешті на висоті.",
                            UpvotedByUsernames = new() { "FirePhoenix" }
                        }
                    },

                    ["The Witcher 3: Wild Hunt"] = new()
                    {
                        new SeedReviewDef
                        {
                            Username = "DragonSlayer",
                            Rating = 5,
                            IsRecommended = true,
                            PlayTimeHours = 145.0,
                            DaysAgo = 25,
                            Content = "Легендарна гра, яка встановила новий стандарт для всіх RPG. Кожен другорядний квест має душу, несподівані повороти та глибокі моральні дилеми.",
                            UpvotedByUsernames = new() { "LunarMage", "MrsZubarikessa", "SilentAssassin" },
                            Replies = new()
                            {
                                new() { Username = "LunarMage", Content = "Історія Кривавого Барона та квести на Скелліге досі стоять перед очима!", HoursAfter = 5, LikesCount = 5 }
                            }
                        },
                        new SeedReviewDef
                        {
                            Username = "MrsZubarikessa",
                            Rating = 5,
                            IsRecommended = true,
                            PlayTimeHours = 120.0,
                            DaysAgo = 16,
                            Content = "Пройшла базову гру і обидва DLC — 'Кров і вино' взагалі як окрема повноцінна гра. Туссент чарівний, а саундтрек слухаю на повторі вже кілька років.",
                            UpvotedByUsernames = new() { "FirePhoenix", "DragonSlayer" },
                            Replies = new()
                            {
                                new() { Username = "FirePhoenix", Content = "Музика гурту Percival в боях — це чиста магія. Атмосфера слов'янського фольклору неповторна.", HoursAfter = 9, LikesCount = 3 }
                            }
                        },
                        new SeedReviewDef
                        {
                            Username = "SilentAssassin",
                            Rating = 5,
                            IsRecommended = true,
                            PlayTimeHours = 95.3,
                            DaysAgo = 9,
                            Content = "Бойова система з мечами, знаками й еліксирами дарує справжнє відчуття професійного мисливця на монстрів. Некст-ген патч зробив гру ще гарнішою.",
                            UpvotedByUsernames = new() { "TitanCrusher" }
                        },
                        new SeedReviewDef
                        {
                            Username = "TitanCrusher",
                            Rating = 5,
                            IsRecommended = true,
                            PlayTimeHours = 110.0,
                            DaysAgo = 4,
                            Content = "Найкраща фентезійна пригода, яка коли-небудь виходила у світі відеоігор. Ґеральт — назавжди в серці!",
                            UpvotedByUsernames = new() { "DragonSlayer", "LunarMage" }
                        }
                    },

                    ["Elden Ring"] = new()
                    {
                        new SeedReviewDef
                        {
                            Username = "TitanCrusher",
                            Rating = 5,
                            IsRecommended = true,
                            PlayTimeHours = 135.0,
                            DaysAgo = 22,
                            Content = "FromSoftware перевершили самі себе. Масштаб світу Межизем'я вражає: сотні босів, таємні підземелля, небезпека за кожним кроком. Складність висока, але задоволення від перемоги не зрівняється ні з чим.",
                            UpvotedByUsernames = new() { "DragonSlayer", "LunarMage" },
                            Replies = new()
                            {
                                new() { Username = "DragonSlayer", Content = "О так, битва з Раданом та Маленією запам'ятається на все життя!", HoursAfter = 4, LikesCount = 6 },
                                new() { Username = "TitanCrusher", Content = "А доповнення Shadow of the Erdtree взагалі підняло планку челенджу ще вище.", HoursAfter = 12, LikesCount = 3 }
                            }
                        },
                        new SeedReviewDef
                        {
                            Username = "LunarMage",
                            Rating = 5,
                            IsRecommended = true,
                            PlayTimeHours = 92.4,
                            DaysAgo = 14,
                            Content = "Грав магом через Місячну катану та чари Карії. Візуально заклинання виглядають просто чарівно. Дуже варіативна та гнучка система прокачки.",
                            UpvotedByUsernames = new() { "SilentAssassin", "FirePhoenix" }
                        },
                        new SeedReviewDef
                        {
                            Username = "SilentAssassin",
                            Rating = 5,
                            IsRecommended = true,
                            PlayTimeHours = 78.0,
                            DaysAgo = 8,
                            Content = "Неймовірний левел-дизайн замків та підземель. Кожен куточок нагороджує допитливого гравця секретними проходами чи унікальною зброєю.",
                            UpvotedByUsernames = new() { "sinichka_bez_egg" },
                            Replies = new()
                            {
                                new() { Username = "sinichka_bez_egg", Content = "Підтримую, дослідження тут найкраще серед усіх soulslike-ігор, мапа не перевантажена маркерами.", HoursAfter = 7, LikesCount = 2 }
                            }
                        },
                        new SeedReviewDef
                        {
                            Username = "DragonSlayer",
                            Rating = 5,
                            IsRecommended = true,
                            PlayTimeHours = 125.0,
                            DaysAgo = 2,
                            Content = "Справжній епос. Атмосфера таємничості, музика в боях з босами та лор Джорджа Мартіна роблять цю гру вічною класикою.",
                            UpvotedByUsernames = new() { "TitanCrusher", "MrsZubarikessa" }
                        }
                    },

                    ["DOOM Eternal"] = new()
                    {
                        new SeedReviewDef
                        {
                            Username = "FirePhoenix",
                            Rating = 5,
                            IsRecommended = true,
                            PlayTimeHours = 45.0,
                            DaysAgo = 20,
                            Content = "Чистий адреналін та геймплейний танець смерті! Гра не прощає помилок: треба постійно рухатися, міняти гармати, розпилювати патрони та палити броню. Саундтрек Міка Гордона — вогонь!",
                            UpvotedByUsernames = new() { "TitanCrusher", "SilentAssassin" },
                            Replies = new()
                            {
                                new() { Username = "TitanCrusher", Content = "Дробовик із гаком — найкращий винахід id Software! Влітаєш у натовп і розриваєш усіх на шматки.", HoursAfter = 5, LikesCount = 4 }
                            }
                        },
                        new SeedReviewDef
                        {
                            Username = "BlazingArrow",
                            Rating = 5,
                            IsRecommended = true,
                            PlayTimeHours = 38.5,
                            DaysAgo = 11,
                            Content = "Ультра-динамічний та красивий шутер від першої особи. Оптимізація ідеальна, картинка соковита, кожен рівень вимагає максимальної концентрації.",
                            UpvotedByUsernames = new() { "FirePhoenix" }
                        },
                        new SeedReviewDef
                        {
                            Username = "sinichka_bez_egg",
                            Rating = 4,
                            IsRecommended = true,
                            PlayTimeHours = 29.0,
                            DaysAgo = 5,
                            Content = "Спочатку було важко звикнути до постійної нестачі набоїв і потреби постійно пиляти ворогів бензопилою, але коли вловлюєш ритм — відірватися неможливо.",
                            UpvotedByUsernames = new() { "SilentAssassin" },
                            Replies = new()
                            {
                                new() { Username = "SilentAssassin", Content = "Так, це скоріше швидкісна головоломка у стилі метал-концерту, ніж просто шутер.", HoursAfter = 6, LikesCount = 2 }
                            }
                        }
                    },

                    ["Sid Meier's Civilization VI"] = new()
                    {
                        new SeedReviewDef
                        {
                            Username = "LunarMage",
                            Rating = 5,
                            IsRecommended = true,
                            PlayTimeHours = 220.0,
                            DaysAgo = 28,
                            Content = "Синдром 'ще одного ходу' у всій красі. Починаєш ввечері, а закінчуєш на світанку з запуском марсіанської місії. Доповнення Gathering Storm додало неймовірні механіки клімату.",
                            UpvotedByUsernames = new() { "MrsZubarikessa", "DragonSlayer" },
                            Replies = new()
                            {
                                new() { Username = "MrsZubarikessa", Content = "Культурна перемога та шпигунство — моє улюблене! Обожнюю переманювати чужі міста лояльністю.", HoursAfter = 4, LikesCount = 5 }
                            }
                        },
                        new SeedReviewDef
                        {
                            Username = "MrsZubarikessa",
                            Rating = 5,
                            IsRecommended = true,
                            PlayTimeHours = 105.0,
                            DaysAgo = 15,
                            Content = "Граємо вечорами з друзями в мультиплеєрі. Дипломатія, війна, союзи та зради — кращої покрокової стратегії годі й шукати.",
                            UpvotedByUsernames = new() { "LunarMage", "BlazingArrow" }
                        },
                        new SeedReviewDef
                        {
                            Username = "BlazingArrow",
                            Rating = 4,
                            IsRecommended = true,
                            PlayTimeHours = 80.0,
                            DaysAgo = 6,
                            Content = "Величезний вибір цивілізацій та лідерів. Система спеціалізації районів міст робить кожне планування мегаполіса цікавою головоломкою.",
                            UpvotedByUsernames = new() { "FirePhoenix" }
                        }
                    },

                    ["Forza Horizon 5"] = new()
                    {
                        new SeedReviewDef
                        {
                            Username = "BlazingArrow",
                            Rating = 5,
                            IsRecommended = true,
                            PlayTimeHours = 88.0,
                            DaysAgo = 21,
                            Content = "Найкраща гоночна аркада на сьогодні. Мексика виглядає приголомшливо: джунглі, пустелі, вулкан та пляжі. Фізика авто приємна як на геймпаді, так і на кермі.",
                            UpvotedByUsernames = new() { "FirePhoenix", "sinichka_bez_egg" },
                            Replies = new()
                            {
                                new() { Username = "FirePhoenix", Content = "Звук вихлопу та деталізація кабіни кожного суперкара на найвищому рівні. Граю щодня!", HoursAfter = 5, LikesCount = 3 }
                            }
                        },
                        new SeedReviewDef
                        {
                            Username = "sinichka_bez_egg",
                            Rating = 5,
                            IsRecommended = true,
                            PlayTimeHours = 54.0,
                            DaysAgo = 13,
                            Content = "Величезний вибір авто, щотижневі фестивальні події та чудовий режим фотографії. Завжди є чим зайнятися.",
                            UpvotedByUsernames = new() { "TitanCrusher" }
                        },
                        new SeedReviewDef
                        {
                            Username = "TitanCrusher",
                            Rating = 5,
                            IsRecommended = true,
                            PlayTimeHours = 42.0,
                            DaysAgo = 4,
                            Content = "Доповнення Hot Wheels з помаранчевими петлями у хмарах — це просто вибух веселощів. Раджу всім фанатам швидкості!",
                            UpvotedByUsernames = new() { "BlazingArrow", "DragonSlayer" },
                            Replies = new()
                            {
                                new() { Username = "DragonSlayer", Content = "Швидкість там шалена, гравітація відчувається просто бомбезно.", HoursAfter = 3, LikesCount = 2 }
                            }
                        }
                    },

                    ["Resident Evil 4 Remake"] = new()
                    {
                        new SeedReviewDef
                        {
                            Username = "SilentAssassin",
                            Rating = 5,
                            IsRecommended = true,
                            PlayTimeHours = 48.0,
                            DaysAgo = 19,
                            Content = "Ідеальний ремейк легендарного горору. Леон відчувається живим та спритним бійцем, боївка з ножем та парируванням бензопил — просто кайф, а атмосфера селища пробирає до кісток.",
                            UpvotedByUsernames = new() { "DragonSlayer", "sinichka_bez_egg" },
                            Replies = new()
                            {
                                new() { Username = "DragonSlayer", Content = "А сцена в замку з Салазаром взагалі поставлена на кінематографічному рівні.", HoursAfter = 7, LikesCount = 4 }
                            }
                        },
                        new SeedReviewDef
                        {
                            Username = "DragonSlayer",
                            Rating = 5,
                            IsRecommended = true,
                            PlayTimeHours = 62.0,
                            DaysAgo = 10,
                            Content = "Capcom знову довела, що вміє оновлювати класику без втрати її шарму. Кампанія Ади Вонг Separate Ways теж обов'язкова до проходження.",
                            UpvotedByUsernames = new() { "MrsZubarikessa" }
                        },
                        new SeedReviewDef
                        {
                            Username = "MrsZubarikessa",
                            Rating = 5,
                            IsRecommended = true,
                            PlayTimeHours = 36.0,
                            DaysAgo = 3,
                            Content = "Баланс між виживанням та адреналіновим бойовиком тут бездоганний. Патронів завжди впритул, кожен ворог становить загрозу.",
                            UpvotedByUsernames = new() { "SilentAssassin", "TitanCrusher" },
                            Replies = new()
                            {
                                new() { Username = "sinichka_bez_egg", Content = "Особливо коли вилазять регенератори в темноті — мороз по шкірі!", HoursAfter = 4, LikesCount = 3 }
                            }
                        }
                    },

                    ["Baldur's Gate 3"] = new()
                    {
                        new SeedReviewDef
                        {
                            Username = "MrsZubarikessa",
                            Rating = 5,
                            IsRecommended = true,
                            PlayTimeHours = 175.0,
                            DaysAgo = 30,
                            Content = "Абсолютний тріумф рольового жанру! Глибина діалогів, живі супутники, свобода вибору та покрокова бойовка на рівні кращих настільних сесій D&D. Гра року без жодних сумнівів.",
                            UpvotedByUsernames = new() { "LunarMage", "DragonSlayer", "FirePhoenix" },
                            Replies = new()
                            {
                                new() { Username = "LunarMage", Content = "Історії Астаріона та Шейдоугарт зворушують до сліз, настільки глибокі характери!", HoursAfter = 4, LikesCount = 7 },
                                new() { Username = "DragonSlayer", Content = "Варіативність рішень просто безпрецедентна — перепроходжу втретє і знаходжу купу нового контенту.", HoursAfter = 16, LikesCount = 5 }
                            }
                        },
                        new SeedReviewDef
                        {
                            Username = "LunarMage",
                            Rating = 5,
                            IsRecommended = true,
                            PlayTimeHours = 145.0,
                            DaysAgo = 17,
                            Content = "Неймовірна тактична глибина. Можна підпалювати масло, кидати ворогів зі скель, перетворюватися на звірів або домовлятися з босами взагалі без бою.",
                            UpvotedByUsernames = new() { "TitanCrusher", "SilentAssassin" }
                        },
                        new SeedReviewDef
                        {
                            Username = "TitanCrusher",
                            Rating = 5,
                            IsRecommended = true,
                            PlayTimeHours = 118.0,
                            DaysAgo = 9,
                            Content = "Проходимо кампанію вчотирьох із друзями по мережі. Це найкращий кооперативний досвід, який ми коли-небудь мали в RPG!",
                            UpvotedByUsernames = new() { "MrsZubarikessa" },
                            Replies = new()
                            {
                                new() { Username = "SilentAssassin", Content = "Кооператив тут дійсно шикарний, хаосу та сміху на кожному кроці вистачає.", HoursAfter = 6, LikesCount = 3 }
                            }
                        },
                        new SeedReviewDef
                        {
                            Username = "FirePhoenix",
                            Rating = 5,
                            IsRecommended = true,
                            PlayTimeHours = 90.0,
                            DaysAgo = 2,
                            Content = "Музика Боріслава Славова геніальна. 'Down by the River' та тема Рафаїла — це шедеври, які назавжди залишаються в плейлисті.",
                            UpvotedByUsernames = new() { "DragonSlayer", "LunarMage" }
                        }
                    },

                    ["Dead Cells"] = new()
                    {
                        new SeedReviewDef
                        {
                            Username = "FirePhoenix",
                            Rating = 5,
                            IsRecommended = true,
                            PlayTimeHours = 82.0,
                            DaysAgo = 24,
                            Content = "Шалена динаміка, ідеальний піксель-арт і постійне бажання зробити 'ще один забіг'. Комбінацій зброї, мутацій та щитів безліч. Кожна смерть — це новий урок.",
                            UpvotedByUsernames = new() { "SilentAssassin", "sinichka_bez_egg" },
                            Replies = new()
                            {
                                new() { Username = "SilentAssassin", Content = "Доповнення Return to Castlevania взагалі викликало бурю ностальгії. Битва з Дракулою — топ!", HoursAfter = 5, LikesCount = 4 }
                            }
                        },
                        new SeedReviewDef
                        {
                            Username = "SilentAssassin",
                            Rating = 5,
                            IsRecommended = true,
                            PlayTimeHours = 98.0,
                            DaysAgo = 14,
                            Content = "Управління чуйне до мілісекунди. Завдяки швидкому темпу гра дарує колосальний драйв. Найкраща метроїдванія-рогалик.",
                            UpvotedByUsernames = new() { "BlazingArrow" }
                        },
                        new SeedReviewDef
                        {
                            Username = "sinichka_bez_egg",
                            Rating = 5,
                            IsRecommended = true,
                            PlayTimeHours = 44.0,
                            DaysAgo = 5,
                            Content = "Один із найстильніших та найзахопливіших рогаликів сучасності. Музика запам'ятовується з перших хвилин.",
                            UpvotedByUsernames = new() { "FirePhoenix", "TitanCrusher" },
                            Replies = new()
                            {
                                new() { Username = "BlazingArrow", Content = "Саундтрек у Селі рибалок просто неймовірний, спокійна та водночас тривожна мелодія.", HoursAfter = 4, LikesCount = 2 }
                            }
                        }
                    },

                    ["Subnautica"] = new()
                    {
                        new SeedReviewDef
                        {
                            Username = "sinichka_bez_egg",
                            Rating = 5,
                            IsRecommended = true,
                            PlayTimeHours = 68.0,
                            DaysAgo = 23,
                            Content = "Неймовірний контраст між спокійним милуванням кораловими садами та паралізуючим жахом, коли занурюєшся в темну безодню і чуєш рев Женця-Левіафана.",
                            UpvotedByUsernames = new() { "MrsZubarikessa", "DragonSlayer" },
                            Replies = new()
                            {
                                new() { Username = "MrsZubarikessa", Content = "Океан тут настільки живий і лячний, що адреналін зашкалює при кожному глибоководному зануренні!", HoursAfter = 6, LikesCount = 4 }
                            }
                        },
                        new SeedReviewDef
                        {
                            Username = "BlazingArrow",
                            Rating = 5,
                            IsRecommended = true,
                            PlayTimeHours = 55.0,
                            DaysAgo = 12,
                            Content = "Будівництво баз, крафтинг обладнання та пілотування субмарини 'Циклоп' — усе зроблено на вищому рівні. Справжня перлина підводного виживання.",
                            UpvotedByUsernames = new() { "SilentAssassin" }
                        },
                        new SeedReviewDef
                        {
                            Username = "DragonSlayer",
                            Rating = 5,
                            IsRecommended = true,
                            PlayTimeHours = 73.0,
                            DaysAgo = 5,
                            Content = "Сюжет розкривається природно через сканування та дослідження таємниць вимерлої цивілізації. Незабутній досвід першопрохідця.",
                            UpvotedByUsernames = new() { "sinichka_bez_egg", "TitanCrusher" },
                            Replies = new()
                            {
                                new() { Username = "TitanCrusher", Content = "Коли знаходиш Дерево-Привид у Загубленій річці — це чисто естетичний шок, яка краса!", HoursAfter = 5, LikesCount = 3 }
                            }
                        }
                    }
                };

                foreach (var (gameTitle, reviewDefs) in reviewData)
                {
                    if (!gamesByTitle.TryGetValue(gameTitle, out var game)) continue;

                    foreach (var item in reviewDefs)
                    {
                        if (!usersByName.TryGetValue(item.Username, out var user)) continue;

                        var review = await context.Reviews.FirstOrDefaultAsync(r => r.GameId == game.Id && r.UserId == user.Id && !r.ParentReviewId.HasValue);
                        if (review == null)
                        {
                            var likedIds = item.UpvotedByUsernames
                                .Select(un => usersByName.GetValueOrDefault(un)?.Id.ToString())
                                .Where(id => !string.IsNullOrEmpty(id))
                                .Select(id => id!)
                                .ToList();

                            review = new Review
                            {
                                Id = Guid.NewGuid(),
                                UserId = user.Id,
                                GameId = game.Id,
                                Rating = item.Rating,
                                IsRecommended = item.IsRecommended,
                                PlayTimeHoursAtReview = item.PlayTimeHours,
                                Content = item.Content,
                                CreatedAt = DateTime.UtcNow.AddDays(-item.DaysAgo),
                                LikesCount = Math.Max(item.UpvotedByUsernames.Count, likedIds.Count),
                                LikedByUsers = likedIds
                            };
                            await context.Reviews.AddAsync(review);
                            await context.SaveChangesAsync();
                        }

                        foreach (var replyDef in item.Replies)
                        {
                            if (!usersByName.TryGetValue(replyDef.Username, out var replyUser)) continue;

                            bool replyExists = await context.Reviews.AnyAsync(r => r.ParentReviewId == review.Id && r.UserId == replyUser.Id && r.Content == replyDef.Content);
                            if (!replyExists)
                            {
                                var reply = new Review
                                {
                                    Id = Guid.NewGuid(),
                                    UserId = replyUser.Id,
                                    GameId = game.Id,
                                    ParentReviewId = review.Id,
                                    Rating = 5,
                                    IsRecommended = true,
                                    PlayTimeHoursAtReview = 0,
                                    Content = replyDef.Content,
                                    CreatedAt = review.CreatedAt.AddHours(replyDef.HoursAfter),
                                    LikesCount = replyDef.LikesCount,
                                    LikedByUsers = new List<string>()
                                };
                                await context.Reviews.AddAsync(reply);
                                await context.SaveChangesAsync();
                            }
                        }
                    }

                    var allTopReviews = await context.Reviews
                        .Where(r => r.GameId == game.Id && !r.ParentReviewId.HasValue)
                        .ToListAsync();

                    game.ReviewsCount = allTopReviews.Count;
                    game.AverageRating = allTopReviews.Count > 0 ? Math.Round(allTopReviews.Average(r => r.Rating), 1) : 5.0;
                    context.Games.Update(game);
                }

                await context.SaveChangesAsync();
                _logger?.LogInformation("[InitData] Seeded/verified game reviews and comment replies for all catalog games.");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "[InitData] Error ensuring game reviews and comments.");
            }
        }

        private class SeedPostCommentDef
        {
            public string AuthorUsername { get; set; } = string.Empty;
            public string Content { get; set; } = string.Empty;
            public int HoursAgo { get; set; } = 12;
            public int LikesCount { get; set; } = 3;
            public List<string> UpvotedByUsernames { get; set; } = new();
            public List<SeedPostCommentDef> Replies { get; set; } = new();
        }

        private class SeedCommunityPostDef
        {
            public string GameTitle { get; set; } = string.Empty;
            public string AuthorUsername { get; set; } = string.Empty;
            public string Category { get; set; } = "forum"; 
            public string Title { get; set; } = string.Empty;
            public string Content { get; set; } = string.Empty;
            public string MediaType { get; set; } = "none";
            public string MediaUrl { get; set; } = string.Empty;
            public string MediaThumbnailUrl { get; set; } = string.Empty;
            public int DaysAgo { get; set; } = 5;
            public List<string> UpvotedByUsernames { get; set; } = new();
            public List<SeedPostCommentDef> Comments { get; set; } = new();
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

                
                var junkPosts = await context.CommunityPosts
                    .Where(p => string.IsNullOrWhiteSpace(p.GameId)
                        || p.GameId.StartsWith("lib-")
                        || p.Title == "adsasd"
                        || p.Title == "ваыва"
                        || p.Title == "чавк"
                        || p.Title == "test"
                        || p.Title == "b"
                        || p.Title == "df"
                        || p.Title == "ddd"
                        || p.Title == "cdd"
                        || p.Title == "dsfsdf"
                        || p.Title == "adsasda")
                    .ToListAsync();

                if (junkPosts.Count > 0)
                {
                    var junkIds = junkPosts.Select(p => p.Id).ToList();
                    var junkComments = await context.CommunityComments
                        .Where(c => junkIds.Contains(c.PostId))
                        .ToListAsync();

                    if (junkComments.Count > 0) context.CommunityComments.RemoveRange(junkComments);
                    context.CommunityPosts.RemoveRange(junkPosts);
                    await context.SaveChangesAsync();
                }

                var allGames = await context.Games.Where(g => !g.IsDlc).ToListAsync();
                var gamesByTitle = allGames.ToDictionary(g => g.Title, g => g, StringComparer.OrdinalIgnoreCase);

                var allUsers = await context.Users.ToListAsync();
                var usersByName = allUsers.ToDictionary(u => u.Username, u => u, StringComparer.OrdinalIgnoreCase);
                var adminUser = allUsers.FirstOrDefault(u => u.IsAdmin) ?? allUsers.FirstOrDefault();
                if (adminUser == null) return;

                var seedPosts = new List<SeedCommunityPostDef>
                {
                    
                    new()
                    {
                        GameTitle = "Cyberpunk 2077",
                        AuthorUsername = "adim",
                        Category = "news",
                        Title = "Оновлення 2.13: Генерація кадрів FSR 3 та оптимізація міста",
                        Content = "Свіжий патч 2.13 для Cyberpunk 2077 та Phantom Liberty вже доступний! Оновлення додає офіційну підтримку генерації кадрів AMD FSR 3 та Intel XeSS 1.3, а також виправляє низку помилок у квестах та оптимізує використання відеопам'яті у важких сценах Найт-Сіті.",
                        MediaType = "image",
                        MediaUrl = "/game_images/cyberpunk-2077_header.jpg",
                        DaysAgo = 10,
                        UpvotedByUsernames = new() { "FirePhoenix", "SilentAssassin", "TitanCrusher" },
                        Comments = new()
                        {
                            new()
                            {
                                AuthorUsername = "FirePhoenix",
                                Content = "Нарешті FSR 3! Кадри в 4K зросли майже вдвічі, тепер картинка надзвичайно плавна.",
                                HoursAgo = 12,
                                LikesCount = 5,
                                Replies = new()
                                {
                                    new() { AuthorUsername = "SilentAssassin", Content = "Підтверджую, навіть на середніх картах тепер стабільні 80+ fps.", HoursAgo = 8, LikesCount = 3 }
                                }
                            },
                            new()
                            {
                                AuthorUsername = "TitanCrusher",
                                Content = "Дякую розробникам за постійну підтримку гри!",
                                HoursAgo = 6,
                                LikesCount = 2
                            }
                        }
                    },
                    new()
                    {
                        GameTitle = "Cyberpunk 2077",
                        AuthorUsername = "adim",
                        Category = "news",
                        Title = "Річниця Phantom Liberty: Понад 5 мільйонів агентів у Песиголові",
                        Content = "Минув рівно рік з моменту виходу шпигунського трилера Phantom Liberty! За цей час гравці врятували (або зрадили) Сойку понад 5 мільйонів разів. Дякуємо нашій неймовірній спільноті за відданість та щирі відгуки.",
                        MediaType = "image",
                        MediaUrl = "/game_images/cyberpunk-phantom-liberty_header.jpg",
                        DaysAgo = 20,
                        UpvotedByUsernames = new() { "MrsZubarikessa", "DragonSlayer" },
                        Comments = new()
                        {
                            new() { AuthorUsername = "MrsZubarikessa", Content = "Phantom Liberty — це найкраще доповнення, в яке я грала за останні 5 років!", HoursAgo = 18, LikesCount = 6 }
                        }
                    },
                    new()
                    {
                        GameTitle = "Cyberpunk 2077",
                        AuthorUsername = "FirePhoenix",
                        Category = "guides",
                        Title = "Гайд на найкращий білд Нетраннера у патчі 2.1",
                        Content = "Ділюся своєю перевіреною збіркою на скрипти та миттєвий перегрів кібердеки. Головне — поєднувати 'Замикання' з кіберімплантом на відновлення пам'яті 'Ex-Disk' та декою Tetratronic Rippler. Вороги падають ще до того, як встигнуть зрозуміти, звідки прилетів скрипт!",
                        MediaType = "image",
                        MediaUrl = "/game_images/cyberpunk-2077_ss1.jpg",
                        DaysAgo = 14,
                        UpvotedByUsernames = new() { "DragonSlayer", "sinichka_bez_egg" },
                        Comments = new()
                        {
                            new()
                            {
                                AuthorUsername = "DragonSlayer",
                                Content = "Спробував цей білд учора — зачистив базу 'Арасаки' взагалі не заходячи всередину через зовнішні камери!",
                                HoursAgo = 10,
                                LikesCount = 4
                            },
                            new()
                            {
                                AuthorUsername = "sinichka_bez_egg",
                                Content = "А які перки в гілці Інтелекту порадиш качати в першу чергу?",
                                HoursAgo = 7,
                                LikesCount = 1,
                                Replies = new()
                                {
                                    new() { AuthorUsername = "FirePhoenix", Content = "Качай одразу 'Чергу скриптів' та 'Оптимізацію пам'яті', це фундаментальна основа.", HoursAgo = 4, LikesCount = 2 }
                                }
                            }
                        }
                    },
                    new()
                    {
                        GameTitle = "Cyberpunk 2077",
                        AuthorUsername = "sinichka_bez_egg",
                        Category = "screenshots",
                        Title = "Нічний Найт-Сіті у неоні та дощі",
                        Content = "Зробив добірку скріншотів у фотомоді біля вежі Арасаки та у Джапан-тауні. Графіка з трасуванням шляху виглядає просто приголомшливо!",
                        MediaType = "image",
                        MediaUrl = "/game_images/cyberpunk-2077_ss2.jpg",
                        DaysAgo = 6,
                        UpvotedByUsernames = new() { "MrsZubarikessa", "BlazingArrow" },
                        Comments = new()
                        {
                            new() { AuthorUsername = "MrsZubarikessa", Content = "Вау, знімок з відображенням неону в калюжі взагалі як справжнє фото!", HoursAgo = 5, LikesCount = 3 }
                        }
                    },
                    new()
                    {
                        GameTitle = "Cyberpunk 2077",
                        AuthorUsername = "TitanCrusher",
                        Category = "forum",
                        Title = "Яка фракція Найт-Сіті на вашу думку найнебезпечніша?",
                        Content = "Як на мене, банда 'Мальстрем' з їхнім повальним кіберпсихозом та павукоподібними імплантами очей — це наймоторошніші психі міста. А кого ви намагаєтеся оминати десятою дорогою?",
                        DaysAgo = 3,
                        UpvotedByUsernames = new() { "DragonSlayer", "SilentAssassin" },
                        Comments = new()
                        {
                            new() { AuthorUsername = "SilentAssassin", Content = "Найнебезпечніша фракція — це корпорація 'Арасака'. Вони діють тихо, але стирають особистість назавжди.", HoursAgo = 4, LikesCount = 5 }
                        }
                    },

                    
                    new()
                    {
                        GameTitle = "The Witcher 3: Wild Hunt",
                        AuthorUsername = "adim",
                        Category = "news",
                        Title = "Офіційний редактор модів REDkit вже доступний усім гравцям!",
                        Content = "Студія CD PROJEKT RED офіційно випустила інструментарій REDkit для The Witcher 3! Тепер кожен охочий може створювати власні сюжетні квести, анімації, нові локації та модифікувати світ гри без жодних обмежень.",
                        MediaType = "image",
                        MediaUrl = "/game_images/witcher-3_header.jpg",
                        DaysAgo = 15,
                        UpvotedByUsernames = new() { "DragonSlayer", "LunarMage", "MrsZubarikessa" },
                        Comments = new()
                        {
                            new() { AuthorUsername = "DragonSlayer", Content = "Це просто свято для модоробів! Вже чекаю на нові сюжетні кампанії від фанатів.", HoursAgo = 12, LikesCount = 5 },
                            new() { AuthorUsername = "LunarMage", Content = "Нарешті можна створити власний затишний маєток для Ґеральта!", HoursAgo = 8, LikesCount = 3 }
                        }
                    },
                    new()
                    {
                        GameTitle = "The Witcher 3: Wild Hunt",
                        AuthorUsername = "DragonSlayer",
                        Category = "guides",
                        Title = "Повний гайд по знаходженню гросмейстерських обладунків Школи Кота",
                        Content = "У Туссенті легко пропустити кілька діаграм, якщо не обшукати затоплені печери біля руїн палацу Ґеліодор. Ось детальний опис маршруту, списку потрібних матеріалів та поради по білду на критичні удари швидкими атаками.",
                        MediaType = "image",
                        MediaUrl = "/game_images/witcher-3_ss1.jpg",
                        DaysAgo = 11,
                        UpvotedByUsernames = new() { "SilentAssassin", "TitanCrusher" },
                        Comments = new()
                        {
                            new() { AuthorUsername = "SilentAssassin", Content = "Дуже дякую за гайд! Школа Кота — мій улюблений сет для швидкого бою з ухиленнями.", HoursAgo = 9, LikesCount = 4 }
                        }
                    },
                    new()
                    {
                        GameTitle = "The Witcher 3: Wild Hunt",
                        AuthorUsername = "MrsZubarikessa",
                        Category = "forum",
                        Title = "Йенніфер чи Трісс? Вічна дилема відьмака",
                        Content = "Кожного разу починаю нову гру з думкою спробувати іншу романтичну лінію, але все одно серце обирає Йен. А кого обрали ви у своєму проходженні та чому?",
                        DaysAgo = 5,
                        UpvotedByUsernames = new() { "LunarMage", "FirePhoenix" },
                        Comments = new()
                        {
                            new() { AuthorUsername = "LunarMage", Content = "Йенніфер — канон за книгами, їхня хімія з Ґеральтом неперевершена!", HoursAgo = 6, LikesCount = 6 },
                            new() { AuthorUsername = "FirePhoenix", Content = "А мені більше до душі Трісс у Новіграді, вона завжди добра і підтримує.", HoursAgo = 3, LikesCount = 4 }
                        }
                    },
                    new()
                    {
                        GameTitle = "The Witcher 3: Wild Hunt",
                        AuthorUsername = "SilentAssassin",
                        Category = "screenshots",
                        Title = "Захід сонця на скелях Ард Скелліг",
                        Content = "Піднявся на вершину гори під місцеву музику. Краєвид на засніжені фіорди просто перехоплює подих.",
                        MediaType = "image",
                        MediaUrl = "/game_images/witcher-3_ss2.jpg",
                        DaysAgo = 2,
                        UpvotedByUsernames = new() { "DragonSlayer" },
                        Comments = new()
                        {
                            new() { AuthorUsername = "DragonSlayer", Content = "Музика на Скелліге — найкраща в усій грі.", HoursAgo = 2, LikesCount = 3 }
                        }
                    },

                    
                    new()
                    {
                        GameTitle = "Elden Ring",
                        AuthorUsername = "adim",
                        Category = "news",
                        Title = "Патч 1.14: Ребаланс зброї та балансування босів у Землях Тіні",
                        Content = "FromSoftware випустила важливе оновлення для Shadow of the Erdtree! Виправлено хітбокси фінального боса, посилено магію віри та парні клинки зворотного хвату, а також покращено стабільність частоти кадрів на ПК.",
                        MediaType = "image",
                        MediaUrl = "/game_images/elden-ring_header.jpg",
                        DaysAgo = 12,
                        UpvotedByUsernames = new() { "TitanCrusher", "LunarMage" },
                        Comments = new()
                        {
                            new() { AuthorUsername = "TitanCrusher", Content = "Нарешті пофіксили просідання кадрів! Тепер бій на арені відчувається чесно.", HoursAgo = 10, LikesCount = 5 },
                            new() { AuthorUsername = "LunarMage", Content = "Парні клинки тепер взагалі розривають, дуже приємне оновлення.", HoursAgo = 6, LikesCount = 3 }
                        }
                    },
                    new()
                    {
                        GameTitle = "Elden Ring",
                        AuthorUsername = "TitanCrusher",
                        Category = "guides",
                        Title = "Як підготуватися до битви з Генералом Раданом",
                        Content = "Головна помилка багатьох початківців — бігти на нього прямо під стріли. Використовуйте залишки зброї в піску як укриття та викликайте привидів-союзників по черзі. У другій фазі обов'язково тримайте дистанцію, коли він злітає в небо метеоритом!",
                        MediaType = "image",
                        MediaUrl = "/game_images/elden-ring_ss1.jpg",
                        DaysAgo = 8,
                        UpvotedByUsernames = new() { "DragonSlayer", "SilentAssassin" },
                        Comments = new()
                        {
                            new() { AuthorUsername = "DragonSlayer", Content = "Коли він падає палаючим метеором — це один із найепічніших моментів в історії ігор!", HoursAgo = 7, LikesCount = 6 }
                        }
                    },
                    new()
                    {
                        GameTitle = "Elden Ring",
                        AuthorUsername = "SilentAssassin",
                        Category = "videos",
                        Title = "Битва з Маленією без броні на 1 рівні",
                        Content = "Записав своє проходження клинка Мікелли персонажем 1 рівня без отримання поранень. Головне — знати таймінг відскоку від танцю Водяної птиці!",
                        MediaType = "video",
                        MediaUrl = "https://www.youtube.com/watch?v=E3Huy2cdih0",
                        MediaThumbnailUrl = "/game_images/elden-ring_ss2.jpg",
                        DaysAgo = 4,
                        UpvotedByUsernames = new() { "TitanCrusher", "sinichka_bez_egg" },
                        Comments = new()
                        {
                            new()
                            {
                                AuthorUsername = "TitanCrusher",
                                Content = "Це просто нелюдська реакція! Моя повага.",
                                HoursAgo = 4,
                                LikesCount = 5,
                                Replies = new()
                                {
                                    new() { AuthorUsername = "SilentAssassin", Content = "Близько 150 спроб, але зате який тріумф у кінці!", HoursAgo = 2, LikesCount = 3 }
                                }
                            }
                        }
                    },

                    
                    new()
                    {
                        GameTitle = "Baldur's Gate 3",
                        AuthorUsername = "adim",
                        Category = "news",
                        Title = "Патч 7 вже вийшов: Офіційний менеджер модифікацій та нові кінцівки",
                        Content = "Larian Studios випустила грандіозний Патч 7! Вбудований менеджер модифікацій, абсолютно нові кінематографічні злі кінцівки для темних персонажів та покращений спліт-скрін для кооперативу на одному ПК.",
                        MediaType = "image",
                        MediaUrl = "/game_images/baldurs-gate-3_header.jpg",
                        DaysAgo = 18,
                        UpvotedByUsernames = new() { "MrsZubarikessa", "LunarMage" },
                        Comments = new()
                        {
                            new() { AuthorUsername = "MrsZubarikessa", Content = "Нові кінцівки для Темної Спокуси моторошні та величні водночас!", HoursAgo = 14, LikesCount = 7 },
                            new() { AuthorUsername = "LunarMage", Content = "Окреме спасибі за менеджер модів просто в меню гри!", HoursAgo = 9, LikesCount = 4 }
                        }
                    },
                    new()
                    {
                        GameTitle = "Baldur's Gate 3",
                        AuthorUsername = "DragonSlayer",
                        Category = "guides",
                        Title = "Мультиклас Барда та Паладина: Смертоносний Смайт",
                        Content = "Поєднання Колегії Мечів з 2 рівнями Паладина дає максимальну кількість слотів заклинань під Божественну Кару та подвійні атаки за хід. Детальний білд на характеристики, вибір зброї та кращі татуювання...",
                        MediaType = "image",
                        MediaUrl = "/game_images/baldurs-gate-3_ss1.jpg",
                        DaysAgo = 9,
                        UpvotedByUsernames = new() { "MrsZubarikessa", "TitanCrusher" },
                        Comments = new()
                        {
                            new() { AuthorUsername = "MrsZubarikessa", Content = "Цей білд буквально розплавляє босів за один хід, грала ним у третьому акті.", HoursAgo = 6, LikesCount = 5 }
                        }
                    },
                    new()
                    {
                        GameTitle = "Baldur's Gate 3",
                        AuthorUsername = "LunarMage",
                        Category = "forum",
                        Title = "Ваш улюблений супутник та романтична лінія?",
                        Content = "У таборі кожен персонаж має неймовірну особисту драму. Хто підкорив ваше серце найбільше: Астаріон, Шейдоугарт, Ґейл, Карлах чи Вілл?",
                        DaysAgo = 4,
                        UpvotedByUsernames = new() { "MrsZubarikessa", "TitanCrusher" },
                        Comments = new()
                        {
                            new() { AuthorUsername = "MrsZubarikessa", Content = "Астаріон — це просто шедевр акторської гри Ніла Ньюбона! Кожна інтонація ідеальна.", HoursAgo = 4, LikesCount = 6 },
                            new() { AuthorUsername = "TitanCrusher", Content = "Карлах найкраща! Така енергійна, добра і щира тифлінг.", HoursAgo = 2, LikesCount = 4 }
                        }
                    },

                    
                    new()
                    {
                        GameTitle = "DOOM Eternal",
                        AuthorUsername = "adim",
                        Category = "news",
                        Title = "Турнірний сезон у Battlemode та оновлені ефекти вогню",
                        Content = "Стартував новий змагальний сезон у Battlemode! Оновлено баланс здібностей Мародера та Манкубуса, а також додано нові косметичні скіни Ката Року.",
                        MediaType = "image",
                        MediaUrl = "/game_images/doom-eternal_header.jpg",
                        DaysAgo = 14,
                        UpvotedByUsernames = new() { "FirePhoenix", "TitanCrusher" },
                        Comments = new()
                        {
                            new() { AuthorUsername = "FirePhoenix", Content = "Час повертатися на арену і роздавати люлей демонам!", HoursAgo = 10, LikesCount = 4 }
                        }
                    },
                    new()
                    {
                        GameTitle = "DOOM Eternal",
                        AuthorUsername = "TitanCrusher",
                        Category = "videos",
                        Title = "Знищення Мародера за 3 секунди: комбо зброї",
                        Content = "Покроковий розбір комбо: постріл супершотгана у вічі -> миттєва баліста -> граната під ноги -> повторний супершотган.",
                        MediaType = "video",
                        MediaUrl = "https://www.youtube.com/watch?v=_NOJ2b_k2yo",
                        MediaThumbnailUrl = "/game_images/doom-eternal_ss1.jpg",
                        DaysAgo = 5,
                        UpvotedByUsernames = new() { "BlazingArrow" },
                        Comments = new()
                        {
                            new() { AuthorUsername = "BlazingArrow", Content = "Швидкість рук просто неймовірна! Дякую за фішку з гранатою.", HoursAgo = 4, LikesCount = 3 }
                        }
                    },

                    
                    new()
                    {
                        GameTitle = "Forza Horizon 5",
                        AuthorUsername = "adim",
                        Category = "news",
                        Title = "Фестивальний сезон 'Суперкари Мексики': 4 нових боліди",
                        Content = "Зустрічайте новий фестивальний місяць у Мексиці! У гру додано ексклюзивні гіперкари, нові небезпечні спідтрапи на схилах каньйону та спеціальні нагороди.",
                        MediaType = "image",
                        MediaUrl = "/game_images/forza-horizon-5_header.jpg",
                        DaysAgo = 16,
                        UpvotedByUsernames = new() { "BlazingArrow", "FirePhoenix" },
                        Comments = new()
                        {
                            new() { AuthorUsername = "BlazingArrow", Content = "Нові траси в каньйоні — це чистий кайф для дрифту!", HoursAgo = 12, LikesCount = 4 }
                        }
                    },
                    new()
                    {
                        GameTitle = "Forza Horizon 5",
                        AuthorUsername = "BlazingArrow",
                        Category = "screenshots",
                        Title = "Дрифт на схилах Gran Caldera",
                        Content = "Мій улюблений Nissan Skyline у затяжному куті на серпантині під час заходу сонця.",
                        MediaType = "image",
                        MediaUrl = "/game_images/forza-horizon-5_ss1.jpg",
                        DaysAgo = 6,
                        UpvotedByUsernames = new() { "FirePhoenix" },
                        Comments = new()
                        {
                            new() { AuthorUsername = "FirePhoenix", Content = "Ракурс ідеальний, дим з-під коліс виглядає дуже реалістично.", HoursAgo = 5, LikesCount = 3 }
                        }
                    },

                    
                    new()
                    {
                        GameTitle = "Resident Evil 4 Remake",
                        AuthorUsername = "adim",
                        Category = "news",
                        Title = "Оновлення режиму The Mercenaries: Нові мапи та персонажі",
                        Content = "Безкоштовне оновлення для режиму Найманців! Нові локації, режим хаосу Mayhem та ексклюзивні костюми для Леона та Ади.",
                        MediaType = "image",
                        MediaUrl = "/game_images/resident-evil-4_header.jpg",
                        DaysAgo = 13,
                        UpvotedByUsernames = new() { "SilentAssassin" },
                        Comments = new()
                        {
                            new() { AuthorUsername = "SilentAssassin", Content = "Вже вибив ранг S++ на всіх мапах за Вескера!", HoursAgo = 9, LikesCount = 4 }
                        }
                    },
                    new()
                    {
                        GameTitle = "Resident Evil 4 Remake",
                        AuthorUsername = "SilentAssassin",
                        Category = "guides",
                        Title = "Як економити патрони та ресурси на хардкорі",
                        Content = "Головне правило: один постріл у коліно або голову, потім вертушка з ноги та обов'язковий добиваючий удар ножем. Ніколи не стріляйте чергами у звичайних селян!",
                        MediaType = "image",
                        MediaUrl = "/game_images/resident-evil-4_ss1.jpg",
                        DaysAgo = 7,
                        UpvotedByUsernames = new() { "sinichka_bez_egg" },
                        Comments = new()
                        {
                            new() { AuthorUsername = "sinichka_bez_egg", Content = "Ніж у цьому ремейку — просто рятівник життя.", HoursAgo = 6, LikesCount = 3 }
                        }
                    },

                    
                    new()
                    {
                        GameTitle = "Sid Meier's Civilization VI",
                        AuthorUsername = "adim",
                        Category = "news",
                        Title = "Щомісячний челендж лідерів: Морське панування за Португалію",
                        Content = "Випробуйте свої сили в новому сценарії на гігантській карті архіпелагів. Побудуйте наймогутнішу торговельну флотилію у світі!",
                        MediaType = "image",
                        MediaUrl = "/game_images/civ-6_header.jpg",
                        DaysAgo = 17,
                        UpvotedByUsernames = new() { "LunarMage" },
                        Comments = new()
                        {
                            new() { AuthorUsername = "LunarMage", Content = "Португалія на островах — це нескінченне золото кожного ходу!", HoursAgo = 11, LikesCount = 4 }
                        }
                    },
                    new()
                    {
                        GameTitle = "Sid Meier's Civilization VI",
                        AuthorUsername = "MrsZubarikessa",
                        Category = "forum",
                        Title = "Який тип перемоги для вас найприємніший?",
                        Content = "Військове панування буває втомливим на пізніх стадіях гри, тому мій вибір — культура та туризм або науковий прорив. А який ваш улюблений стиль перемоги?",
                        DaysAgo = 5,
                        UpvotedByUsernames = new() { "LunarMage", "BlazingArrow" },
                        Comments = new()
                        {
                            new() { AuthorUsername = "LunarMage", Content = "Наукова перемога через космічні проєкти — класика!", HoursAgo = 4, LikesCount = 4 },
                            new() { AuthorUsername = "BlazingArrow", Content = "А я люблю релігійну перемогу, посилати апостолів у чужі столиці — це весело.", HoursAgo = 2, LikesCount = 3 }
                        }
                    },

                    
                    new()
                    {
                        GameTitle = "Dead Cells",
                        AuthorUsername = "adim",
                        Category = "news",
                        Title = "Оновлення 'The End is Near': Фінальний патч для безсмертного рогалика",
                        Content = "Motion Twin та Evil Empire випустили контентний патч 35! Нові прокляті монстри, зброя найвищого ризику та унікальні мутації, що ставлять яскраву крапку в шестирічній історії гри.",
                        MediaType = "image",
                        MediaUrl = "/game_images/dead-cells_header.jpg",
                        DaysAgo = 22,
                        UpvotedByUsernames = new() { "FirePhoenix", "SilentAssassin" },
                        Comments = new()
                        {
                            new() { AuthorUsername = "FirePhoenix", Content = "Дякуємо розробникам за ці 6 років фантастичної підтримки гри!", HoursAgo = 16, LikesCount = 5 }
                        }
                    },
                    new()
                    {
                        GameTitle = "Dead Cells",
                        AuthorUsername = "FirePhoenix",
                        Category = "screenshots",
                        Title = "Перемога над Дракулою без втрати здоров'я",
                        Content = "Скріншот мого фінального білду з клинком Алукарда та щитом крові після бездоганної перемоги над володарем замку.",
                        MediaType = "image",
                        MediaUrl = "/game_images/dead-cells_ss1.jpg",
                        DaysAgo = 8,
                        UpvotedByUsernames = new() { "SilentAssassin" },
                        Comments = new()
                        {
                            new() { AuthorUsername = "SilentAssassin", Content = "Краса! Щит Алукарда — один із найприємніших у грі.", HoursAgo = 6, LikesCount = 3 }
                        }
                    },

                    
                    new()
                    {
                        GameTitle = "Subnautica",
                        AuthorUsername = "adim",
                        Category = "news",
                        Title = "Оновлення 2.0: Оновлений рушій та повна підтримка хмарних збережень",
                        Content = "Оригінальну Subnautica переведено на сучасну версію рушія з Below Zero! Понад 800 виправлень багів, оптимізація баз та синхронізація через Dteam Cloud.",
                        MediaType = "image",
                        MediaUrl = "/game_images/subnautica_header.jpg",
                        DaysAgo = 25,
                        UpvotedByUsernames = new() { "sinichka_bez_egg", "MrsZubarikessa" },
                        Comments = new()
                        {
                            new() { AuthorUsername = "sinichka_bez_egg", Content = "Чудова новина! Тепер під водою немає жодних підвисань.", HoursAgo = 18, LikesCount = 4 }
                        }
                    },
                    new()
                    {
                        GameTitle = "Subnautica",
                        AuthorUsername = "sinichka_bez_egg",
                        Category = "screenshots",
                        Title = "Затишна підводна база у Загубленій річці",
                        Content = "Побудував оглядову купольну кімнату прямо біля підводного Дерева-Привида на глибині 900 метрів. Затишок серед безодні.",
                        MediaType = "image",
                        MediaUrl = "/game_images/subnautica_ss1.jpg",
                        DaysAgo = 7,
                        UpvotedByUsernames = new() { "MrsZubarikessa", "DragonSlayer" },
                        Comments = new()
                        {
                            new() { AuthorUsername = "MrsZubarikessa", Content = "Яке гарне блакитне сяйво! Одне з моїх улюблених місць на планеті 4546B.", HoursAgo = 5, LikesCount = 4 },
                            new() { AuthorUsername = "DragonSlayer", Content = "І головне — там немає Женців-Левіафанів, можна спокійно жити.", HoursAgo = 3, LikesCount = 3 }
                        }
                    }
                };

                foreach (var item in seedPosts)
                {
                    if (!gamesByTitle.TryGetValue(item.GameTitle, out var game)) continue;

                    var author = usersByName.GetValueOrDefault(item.AuthorUsername) ?? adminUser;

                    var post = await context.CommunityPosts.FirstOrDefaultAsync(p =>
                        p.Title == item.Title && (p.GameGuidId == game.Id || p.GameId == game.Id.ToString()));

                    if (post == null)
                    {
                        var likedIds = item.UpvotedByUsernames
                            .Select(un => usersByName.GetValueOrDefault(un)?.Id.ToString())
                            .Where(id => !string.IsNullOrEmpty(id))
                            .Select(id => id!)
                            .ToList();

                        post = new CommunityPost
                        {
                            Id = $"seed-{Guid.NewGuid():N}",
                            GameId = game.Id.ToString(),
                            GameGuidId = game.Id,
                            GameTitle = game.Title,
                            GameBannerUrl = game.HeaderImageUrl ?? game.CoverImageUrl ?? string.Empty,
                            Author = new AuthorDto
                            {
                                Id = author.Id.ToString(),
                                Username = author.Username,
                                AvatarUrl = author.AvatarUrl ?? string.Empty
                            },
                            CreatedAt = DateTime.UtcNow.AddDays(-item.DaysAgo),
                            Category = item.Category,
                            Title = item.Title,
                            Content = item.Content,
                            Media = new PostMedia
                            {
                                Type = item.MediaType,
                                Url = item.MediaUrl,
                                ThumbnailUrl = string.IsNullOrWhiteSpace(item.MediaThumbnailUrl) ? item.MediaUrl : item.MediaThumbnailUrl
                            },
                            LikedByUsers = likedIds
                        };

                        await context.CommunityPosts.AddAsync(post);
                        await context.SaveChangesAsync();
                    }
                    else
                    {
                        post.GameBannerUrl = game.HeaderImageUrl ?? game.CoverImageUrl ?? string.Empty;
                        if (!string.IsNullOrWhiteSpace(item.MediaUrl))
                        {
                            post.Media = new PostMedia
                            {
                                Type = item.MediaType,
                                Url = item.MediaUrl,
                                ThumbnailUrl = string.IsNullOrWhiteSpace(item.MediaThumbnailUrl) ? item.MediaUrl : item.MediaThumbnailUrl
                            };
                        }
                        context.CommunityPosts.Update(post);
                        await context.SaveChangesAsync();
                    }

                    foreach (var c in item.Comments)
                    {
                        var commenter = usersByName.GetValueOrDefault(c.AuthorUsername) ?? adminUser;

                        var comment = await context.CommunityComments.FirstOrDefaultAsync(cc =>
                            cc.PostId == post.Id && string.IsNullOrEmpty(cc.ParentCommentId) && cc.Content == c.Content);

                        if (comment == null)
                        {
                            var commentLikedIds = c.UpvotedByUsernames
                                .Select(un => usersByName.GetValueOrDefault(un)?.Id.ToString())
                                .Where(id => !string.IsNullOrEmpty(id))
                                .Select(id => id!)
                                .ToList();

                            comment = new CommunityComment
                            {
                                Id = $"c-{Guid.NewGuid():N}",
                                PostId = post.Id,
                                ParentCommentId = null,
                                Author = new AuthorDto
                                {
                                    Id = commenter.Id.ToString(),
                                    Username = commenter.Username,
                                    AvatarUrl = commenter.AvatarUrl ?? string.Empty
                                },
                                CreatedAt = post.CreatedAt.AddHours(c.HoursAgo),
                                Content = c.Content,
                                LikesCount = Math.Max(c.LikesCount, commentLikedIds.Count),
                                LikedByUsers = commentLikedIds
                            };

                            await context.CommunityComments.AddAsync(comment);
                            await context.SaveChangesAsync();
                        }

                        foreach (var r in c.Replies)
                        {
                            var replyAuthor = usersByName.GetValueOrDefault(r.AuthorUsername) ?? adminUser;

                            bool replyExists = await context.CommunityComments.AnyAsync(cc =>
                                cc.PostId == post.Id && cc.ParentCommentId == comment.Id && cc.Content == r.Content);

                            if (!replyExists)
                            {
                                var reply = new CommunityComment
                                {
                                    Id = $"r-{Guid.NewGuid():N}",
                                    PostId = post.Id,
                                    ParentCommentId = comment.Id,
                                    Author = new AuthorDto
                                    {
                                        Id = replyAuthor.Id.ToString(),
                                        Username = replyAuthor.Username,
                                        AvatarUrl = replyAuthor.AvatarUrl ?? string.Empty
                                    },
                                    CreatedAt = comment.CreatedAt.AddHours(r.HoursAgo),
                                    Content = r.Content,
                                    LikesCount = r.LikesCount,
                                    LikedByUsers = new List<string>()
                                };

                                await context.CommunityComments.AddAsync(reply);
                                await context.SaveChangesAsync();
                            }
                        }
                    }
                }

                _logger?.LogInformation("[InitData] Successfully seeded community posts, game news, and comments into database.");
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
                        existing.HeaderImageUrl = headerImg;
                        existing.CoverImageUrl = coverImg;
                        existing.ScreenshotUrls = screenshots;
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
                    "/game_images/cyberpunk-2077_header.jpg",
                    "/game_images/cyberpunk-2077_cover.jpg",
                    new List<string>
                    {
                        "/game_images/cyberpunk-2077_ss1.jpg",
                        "/game_images/cyberpunk-2077_ss2.jpg",
                        "/game_images/cyberpunk-2077_ss3.jpg"
                    },
                    "https://www.youtube.com/watch?v=dQw4w9WgXcQ",
                    "Чудова гра! Неймовірна атмосфера, графіка та сюжет тримають у напрузі до останньої хвилини. Рекомендую всім!"
                );
                GetOrAddDlc(cp2077.Id, "Cyberpunk 2077: Phantom Liberty", "Масштабне сюжетне доповнення у жанрі шпигунського трилера до Cyberpunk 2077.", "Шпигунський трилер у Найт-Сіті.", 2_500_000_000, 0, "/storage/games/cyberpunk-phantom-liberty.zip", 11000, 5.0, 2, new List<string> { "Action", "RPG", "Cyberpunk", "DLC" }, new List<string> { "Windows", "MacOS" }, "/game_images/cyberpunk-phantom-liberty_header.jpg", "/game_images/cyberpunk-phantom-liberty_cover.jpg");
                GetOrAddDlc(cp2077.Id, "Cyberpunk 2077: REDmod", "Безкоштовний інструмент для створення та завантаження модифікацій.", "Інструмент модифікацій.", 0, 0, "/storage/games/cyberpunk-redmod.zip", 8000, 5.0, 1, new List<string> { "DLC", "Modding" }, new List<string> { "Windows" }, "/game_images/cyberpunk-redmod_header.jpg", "/game_images/cyberpunk-redmod_cover.jpg");

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
                    "/game_images/witcher-3_header.jpg",
                    "/game_images/witcher-3_cover.jpg",
                    new List<string>
                    {
                        "/game_images/witcher-3_ss1.jpg",
                        "/game_images/witcher-3_ss2.jpg",
                        "/game_images/witcher-3_ss3.jpg"
                    },
                    "https://www.youtube.com/watch?v=c0i88t0Kacs",
                    "Одна з найкращих RPG в історії! Неймовірний сюжет, живі персонажі та прекрасна музика."
                );
                GetOrAddDlc(witcher3.Id, "The Witcher 3: Hearts of Stone", "Знову станьте Ґеральтом із Рівії, щоб виконати контракт для безжального капітана бандитів Ольґерда фон Еверека.", "Перше велике сюжетне доповнення для гри The Witcher 3.", 1_200_000_000, 30, "/storage/games/witcher3-hos.zip", 14000, 4.9, 3, new List<string> { "RPG", "Fantasy", "DLC" }, new List<string> { "Windows", "MacOS" }, "/game_images/witcher-3-hos_header.jpg", "/game_images/witcher-3-hos_cover.jpg");
                GetOrAddDlc(witcher3.Id, "The Witcher 3: Blood and Wine", "Вирушайте до далекої та залитої сонцем землі Туссент, де безтурботне життя ховає страшну та криваву таємницю.", "Масштабне доповнення на 30+ годин у новому регіоні Туссент.", 1_800_000_000, 30, "/storage/games/witcher3-baw.zip", 19000, 5.0, 5, new List<string> { "RPG", "Fantasy", "Adventure", "DLC" }, new List<string> { "Windows", "MacOS" }, "/game_images/witcher-3-baw_header.jpg", "/game_images/witcher-3-baw_cover.jpg");

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
                    "/game_images/elden-ring_header.jpg",
                    "/game_images/elden-ring_cover.jpg",
                    new List<string>
                    {
                        "/game_images/elden-ring_ss1.jpg",
                        "/game_images/elden-ring_ss2.jpg",
                        "/game_images/elden-ring_ss3.jpg"
                    },
                    "https://www.youtube.com/watch?v=E3Huy2cdih0",
                    "Неймовірний відкритий світ і незабутні битви з босами. Справжнє мистецтво геймдизайну."
                );
                GetOrAddDlc(eldenRing.Id, "Elden Ring: Shadow of the Erdtree", "Вирушайте в Землі Тіні слідами Мікелли та розкрийте темні таємниці світового порядку.", "Найбільше сюжетне доповнення для Elden Ring.", 3_800_000_000, 0, "/storage/games/elden-ring-erdtree.zip", 22000, 4.8, 6, new List<string> { "RPG", "Fantasy", "Action", "DLC" }, new List<string> { "Windows" }, "/game_images/elden-ring-erdtree_header.jpg", "/game_images/elden-ring-erdtree_cover.jpg");
                GetOrAddDlc(eldenRing.Id, "Elden Ring: Colosseum & Soundtrack", "Офіційне доповнення із PvP-аренами Колізеїв та цифровим саундтреком.", "PvP-арени Колізеїв та офіційний саундтрек.", 800_000_000, 0, "/storage/games/elden-ring-colosseum.zip", 14000, 4.8, 3, new List<string> { "RPG", "Action", "DLC" }, new List<string> { "Windows" }, "/game_images/elden-ring-colosseum_header.jpg", "/game_images/elden-ring-colosseum_cover.jpg");

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
                    "/game_images/doom-eternal_header.jpg",
                    "/game_images/doom-eternal_cover.jpg",
                    new List<string>
                    {
                        "/game_images/doom-eternal_ss1.jpg",
                        "/game_images/doom-eternal_ss2.jpg",
                        "/game_images/doom-eternal_ss3.jpg"
                    },
                    "https://www.youtube.com/watch?v=_NOJ2b_k2yo",
                    "Безперервний адреналіновий екшн і неймовірна музика Міка Гордона. Чистий драйв!"
                );
                GetOrAddDlc(doom.Id, "DOOM Eternal: The Ancient Gods - Part One", "Ваша війна проти демонів триває. Здолайте нові загрози у найвищих небесних сферах та безоднях.", "Перше сюжетне доповнення найвищої складності.", 1_800_000_000, 50, "/storage/games/doom-ancient-gods-1.zip", 9500, 4.7, 2, new List<string> { "Shooter", "Action", "SciFi", "DLC" }, new List<string> { "Windows" }, "/game_images/doom-ancient-gods-1_header.jpg", "/game_images/doom-ancient-gods-1_cover.jpg");
                GetOrAddDlc(doom.Id, "DOOM Eternal: The Ancient Gods - Part Two", "Кульмінація саги про Ката Року. Штурмуйте цитадель Темного Володаря в Іммфорі.", "Фінальне епічне протистояння з владикою Пекла.", 1_800_000_000, 50, "/storage/games/doom-ancient-gods-2.zip", 8900, 4.75, 2, new List<string> { "Shooter", "Action", "SciFi", "DLC" }, new List<string> { "Windows" }, "/game_images/doom-ancient-gods-2_header.jpg", "/game_images/doom-ancient-gods-2_cover.jpg");

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
                    "/game_images/civ-6_header.jpg",
                    "/game_images/civ-6_cover.jpg",
                    new List<string>
                    {
                        "/game_images/civ-6_ss1.jpg",
                        "/game_images/civ-6_ss2.jpg",
                        "/game_images/civ-6_ss3.jpg"
                    },
                    "https://www.youtube.com/watch?v=5KdE0p2joJw",
                    "Ще один хід — і вже ранок! Чудова покрокова стратегія для довгих вечорів."
                );
                GetOrAddDlc(civ6.Id, "Civilization VI: Gathering Storm", "Світ стає живим: кліматичні зміни, стихійні лиха, енергетичні ресурси та Світовий конгрес.", "Глобальне доповнення з новими механіками екології та дипломатії.", 1_500_000_000, 70, "/storage/games/civ6-gs.zip", 16000, 4.8, 3, new List<string> { "Strategy", "Simulation", "DLC" }, new List<string> { "Windows", "MacOS", "Linux" }, "/game_images/civ-6-gathering-storm_header.jpg", "/game_images/civ-6-gathering-storm_cover.jpg");
                GetOrAddDlc(civ6.Id, "Civilization VI: Rise and Fall", "Епохи розквіту й темряви, вірність міст та нова система губернаторів.", "Доповнення про піднесення та занепад цивілізацій.", 1_200_000_000, 70, "/storage/games/civ6-rf.zip", 14000, 4.7, 2, new List<string> { "Strategy", "Simulation", "DLC" }, new List<string> { "Windows", "MacOS", "Linux" }, "/game_images/civ-6-rise-and-fall_header.jpg", "/game_images/civ-6-rise-and-fall_cover.jpg");

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
                    "/game_images/forza-horizon-5_header.jpg",
                    "/game_images/forza-horizon-5_cover.jpg",
                    new List<string>
                    {
                        "/game_images/forza-horizon-5_ss1.jpg",
                        "/game_images/forza-horizon-5_ss2.jpg",
                        "/game_images/forza-horizon-5_ss3.jpg"
                    },
                    "https://www.youtube.com/watch?v=FYH9n37B7Yw",
                    "Приголомшлива графіка і фізика керування авто. Найкраща автогонка десятиліття."
                );
                GetOrAddDlc(fh5.Id, "Forza Horizon 5: Hot Wheels", "Злітайте у небеса на запаморочливих помаранчевих треках Hot Wheels у хмарах над Мексикою.", "Шалені петлі та екстремальні треки Hot Wheels.", 1_800_000_000, 20, "/storage/games/fh5-hotwheels.zip", 18000, 4.85, 4, new List<string> { "Racing", "Sports", "DLC" }, new List<string> { "Windows" }, "/game_images/forza-horizon-5-hot-wheels_header.jpg", "/game_images/forza-horizon-5-hot-wheels_cover.jpg");
                GetOrAddDlc(fh5.Id, "Forza Horizon 5: Rally Adventure", "Підкорюйте бездоріжжя Сьєрра-Нуева на спеціально підготовлених ралійних болідах.", "Справжнє ралі з ґрунтовими трасами та пилом.", 1_800_000_000, 20, "/storage/games/fh5-rally.zip", 13000, 4.7, 2, new List<string> { "Racing", "Sports", "DLC" }, new List<string> { "Windows" }, "/game_images/forza-horizon-5-rally_header.jpg", "/game_images/forza-horizon-5-rally_cover.jpg");

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
                    "/game_images/resident-evil-4_header.jpg",
                    "/game_images/resident-evil-4_cover.jpg",
                    new List<string>
                    {
                        "/game_images/resident-evil-4_ss1.jpg",
                        "/game_images/resident-evil-4_ss2.jpg",
                        "/game_images/resident-evil-4_ss3.jpg"
                    },
                    "https://www.youtube.com/watch?v=j5Ic2z3_xP8",
                    "Ідеальний ремейк. Атмосфера жаху та напружені перестрілки тримають до фінальних титрів."
                );
                GetOrAddDlc(re4.Id, "Resident Evil 4: Separate Ways", "Дізнайтеся інший бік історії очима загадкової Ади Вонг під час її таємної місії.", "Сюжетна кампанія Ади Вонг із крюком-кішкою.", 1_200_000_000, 0, "/storage/games/re4-separate-ways.zip", 15000, 4.95, 3, new List<string> { "Horror", "Action", "DLC" }, new List<string> { "Windows", "MacOS" }, "/game_images/re4-separate-ways_header.jpg", "/game_images/re4-separate-ways_cover.jpg");
                GetOrAddDlc(re4.Id, "Resident Evil 4: Extra DLC Pack", "Комплект спеціальних костюмів, фільтрів та ексклюзивної зброї для Леона та Ешлі.", "Набір ексклюзивного спорядження та стилів.", 700_000_000, 0, "/storage/games/re4-extra-dlc.zip", 12000, 4.8, 2, new List<string> { "Horror", "Action", "DLC" }, new List<string> { "Windows", "MacOS" }, "/game_images/re4-extra-dlc_header.jpg", "/game_images/re4-extra-dlc_cover.jpg");

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
                    "/game_images/baldurs-gate-3_header.jpg",
                    "/game_images/baldurs-gate-3_cover.jpg",
                    new List<string>
                    {
                        "/game_images/baldurs-gate-3_ss1.jpg",
                        "/game_images/baldurs-gate-3_ss2.jpg",
                        "/game_images/baldurs-gate-3_ss3.jpg"
                    },
                    "https://www.youtube.com/watch?v=1T22wN1BIzU",
                    "Абсолютна свобода дій та неймовірно прописані супутники. Гра року!"
                );
                GetOrAddDlc(bg3.Id, "Baldur's Gate 3: Digital Deluxe Upgrade", "Унікальні бардичні пісні, артефакти з всесвіту Divinity, цифровий артбук та саундтрек.", "Колекційний набір бонусних матеріалів.", 1_000_000_000, 0, "/storage/games/bg3-deluxe.zip", 21000, 4.9, 3, new List<string> { "RPG", "Fantasy", "DLC" }, new List<string> { "Windows", "MacOS" }, "/game_images/bg3-deluxe_header.jpg", "/game_images/bg3-deluxe_cover.jpg");
                GetOrAddDlc(bg3.Id, "Baldur's Gate 3: Adventure Companion Pack", "Додаткові косметичні набори, ігрові кубики та унікальні діалогові портрети.", "Набір кастомізації персонажів та кубиків.", 600_000_000, 0, "/storage/games/bg3-companion.zip", 16000, 4.85, 2, new List<string> { "RPG", "Fantasy", "DLC" }, new List<string> { "Windows", "MacOS" }, "/game_images/bg3-companion_header.jpg", "/game_images/bg3-companion_cover.jpg");

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
                    "/game_images/dead-cells_header.jpg",
                    "/game_images/dead-cells_cover.jpg",
                    new List<string>
                    {
                        "/game_images/dead-cells_ss1.jpg",
                        "/game_images/dead-cells_ss2.jpg",
                        "/game_images/dead-cells_ss3.jpg"
                    },
                    "https://www.youtube.com/watch?v=rvXZQn44AAU",
                    "Неймовірна динаміка та різноманіття зброї. Відірватися неможливо."
                );
                GetOrAddDlc(deadCells.Id, "Dead Cells: Return to Castlevania", "Легендарний кросовер: бийтеся пліч-о-пліч з Алукардом і Ріхтером Белмонтом у залах замку Дракули.", "Масштабне доповнення за мотивами всесвіту Castlevania.", 900_000_000, 20, "/storage/games/dead-cells-castlevania.zip", 17000, 4.95, 4, new List<string> { "Indie", "Action", "Fantasy", "DLC" }, new List<string> { "Windows", "MacOS", "Linux" }, "/game_images/dead-cells-castlevania_header.jpg", "/game_images/dead-cells-castlevania_cover.jpg");
                GetOrAddDlc(deadCells.Id, "Dead Cells: The Bad Seed", "Досліджуйте розслаблене Дендропарк, боріться з новими мутантами у болотах та киньте виклик Матері Кліщів.", "Доповнення з новими біомами та зброєю.", 700_000_000, 20, "/storage/games/dead-cells-bad-seed.zip", 13000, 4.8, 2, new List<string> { "Indie", "Action", "DLC" }, new List<string> { "Windows", "MacOS", "Linux" }, "/game_images/dead-cells-bad-seed_header.jpg", "/game_images/dead-cells-bad-seed_cover.jpg");

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
                    "/game_images/subnautica_header.jpg",
                    "/game_images/subnautica_cover.jpg",
                    new List<string>
                    {
                        "/game_images/subnautica_ss1.jpg",
                        "/game_images/subnautica_ss2.jpg",
                        "/game_images/subnautica_ss3.jpg"
                    },
                    "https://www.youtube.com/watch?v=Rz2SNm8VguE",
                    "Неймовірне відчуття першовідкривача та легкий страх глибини. Чудовий підводний світ!"
                );
                GetOrAddDlc(subnautica.Id, "Subnautica: Below Zero", "Зануртеся в крижану експедицію на арктичному регіоні планети 4546B.", "Окрема арктична історія у всесвіті Subnautica.", 2_400_000_000, 30, "/storage/games/subnautica-bz.zip", 21000, 4.7, 5, new List<string> { "Adventure", "SciFi", "Simulation", "DLC" }, new List<string> { "Windows", "MacOS" }, "/game_images/subnautica-below-zero_header.jpg", "/game_images/subnautica-below-zero_cover.jpg");
                GetOrAddDlc(subnautica.Id, "Subnautica: Deep Sea Pack", "Додаткові креслення підводних модулів та екзокостюмів для глибоководних занурень.", "Набір глибоководного спорядження та креслень.", 900_000_000, 0, "/storage/games/subnautica-deepsea.zip", 14000, 4.8, 3, new List<string> { "Adventure", "SciFi", "DLC" }, new List<string> { "Windows", "MacOS" }, "/game_images/subnautica-deepsea_header.jpg", "/game_images/subnautica-deepsea_cover.jpg");

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

        private async Task EnsureDefaultAdminsAsync(AppDbContext context)
        {
            PasswordHasher.CreatePasswordHash("admin123321", out string passwordHash, out string passwordSalt);
            var adminEthAddress = _configuration?["Ethereum:PublicKey"] ?? "0xf39fd6e51aad88f6f4ce6ab8827279cfffb92266";

            
            var adim = await context.Users.FirstOrDefaultAsync(u => u.Email == "adim@gmail.com" || u.Username == "adim");
            if (adim == null)
            {
                adim = new Duser
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
                await context.Users.AddAsync(adim);
                _logger?.LogInformation("[InitData] Created default admin user 'adim'.");
            }
            else if (!adim.IsAdmin)
            {
                adim.IsAdmin = true;
            }

            
            var admin = await context.Users.FirstOrDefaultAsync(u => u.Email == "admin@gmail.com" || u.Username == "admin");
            if (admin == null)
            {
                admin = new Duser
                {
                    Id = Guid.NewGuid(),
                    Email = "admin@gmail.com",
                    Username = "admin",
                    PasswordHash = passwordHash,
                    PasswordSalt = passwordSalt,
                    WalletAddress = "EQB_v2Admin_SampleTonWalletAddress_888",
                    HardhatAddress = "0x70997970C51812dc3A010C7d01b50e0d17dc79C8",
                    BalanceInNanoTons = 100_000_000_000,
                    TotalEarningsInNanoTons = 0,
                    CreatedAt = DateTime.UtcNow,
                    IsAdmin = true,
                    IsBanned = false,
                    Status = UserStatus.Online,
                    AvatarUrl = "https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?w=500&auto=format&fit=crop&q=80",
                    Bio = "Dteam Lead Administrator"
                };
                await context.Users.AddAsync(admin);
                _logger?.LogInformation("[InitData] Created default admin user 'admin'.");
            }
            else if (!admin.IsAdmin)
            {
                admin.IsAdmin = true;
            }

            await context.SaveChangesAsync();
        }
    }
}
