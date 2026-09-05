using DteamBackend.Data;
using DteamBackend.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace DteamBackend.Hubs
{
    public class OnlineHub : Hub
    {
        public static readonly ConcurrentDictionary<string, string> OnlineUsers = new();
        public static readonly ConcurrentDictionary<string, DateTime> ConnectionStartTimes = new();
        public static readonly ConcurrentDictionary<string, Guid> ConnectionUserGuids = new();
        public static readonly ConcurrentDictionary<Guid, DateTime> UserLastSyncTimes = new();

        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly INftService _nftService;
        private readonly ILogger<OnlineHub> _logger;

        // Інтервал видачі NFT за час перебування на сайті:
        public const long HourlyRewardIntervalSeconds = 3600;
        public const long ShortRewardIntervalSeconds = 30;
        public const long TenHourRewardIntervalSeconds = 36000;

        // Поточний активний інтервал (10 годин):
        public const long ActiveRewardIntervalSeconds = TenHourRewardIntervalSeconds;

        public OnlineHub(
            IDbContextFactory<AppDbContext> contextFactory,
            INftService nftService,
            ILogger<OnlineHub> logger)
        {
            _contextFactory = contextFactory;
            _nftService = nftService;
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            var connectionId = Context.ConnectionId;
            var userIdStr = Context.UserIdentifier
                ?? Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? Context.User?.FindFirst("sub")?.Value;

            Guid userGuid = Guid.Empty;
            if (!string.IsNullOrEmpty(userIdStr))
            {
                Guid.TryParse(userIdStr, out userGuid);
            }

            if (userGuid == Guid.Empty)
            {
                var httpContext = Context.GetHttpContext();
                var queryToken = httpContext?.Request.Query["access_token"].ToString();
                if (string.IsNullOrEmpty(queryToken))
                {
                    queryToken = httpContext?.Request.Query["token"].ToString();
                }
                userGuid = ExtractUserIdFromToken(queryToken);
            }

            OnlineUsers[connectionId] = userGuid != Guid.Empty ? userGuid.ToString() : connectionId;
            var now = DateTime.UtcNow;
            ConnectionStartTimes[connectionId] = now;

            if (userGuid != Guid.Empty)
            {
                ConnectionUserGuids[connectionId] = userGuid;
                UserLastSyncTimes.TryAdd(userGuid, now);

                try
                {
                    await using var dbContext = await _contextFactory.CreateDbContextAsync();

                    var user = await dbContext.Users.FindAsync(userGuid);
                    if (user != null)
                    {
                        user.LastConnectedAt = now;
                        await dbContext.SaveChangesAsync();
                        _logger.LogInformation("[OnlineHub] Користувач {Username} ({UserId}) підключився до сайту о {Time}", user.Username, user.Id, now);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[OnlineHub] Помилка збереження LastConnectedAt для користувача {UserId}", userGuid);
                }
            }
            else
            {
                _logger.LogInformation("[OnlineHub] Анонімне підключення {ConnectionId}", connectionId);
            }

            await Clients.All.SendAsync("UpdateOnlineCount", GetUniqueOnlineCount());

            await base.OnConnectedAsync();
        }

        [HubMethodName("Heartbeat")]
        public async Task Heartbeat(string? token = null)
        {
            var connectionId = Context.ConnectionId;
            var userIdStr = Context.UserIdentifier
                ?? Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? Context.User?.FindFirst("sub")?.Value;

            Guid userGuid = Guid.Empty;
            if (!string.IsNullOrEmpty(userIdStr))
            {
                Guid.TryParse(userIdStr, out userGuid);
            }

            if (userGuid == Guid.Empty && !string.IsNullOrWhiteSpace(token))
            {
                userGuid = ExtractUserIdFromToken(token);
            }

            if (userGuid != Guid.Empty)
            {
                bool wasMissing = !ConnectionUserGuids.ContainsKey(connectionId);
                ConnectionUserGuids[connectionId] = userGuid;
                OnlineUsers[connectionId] = userGuid.ToString();
                UserLastSyncTimes.TryAdd(userGuid, DateTime.UtcNow);

                if (wasMissing)
                {
                    _logger.LogInformation("[OnlineHub] Heartbeat зареєстрував користувача {UserId} для connection {ConnectionId}", userGuid, connectionId);
                    await Clients.All.SendAsync("UpdateOnlineCount", GetUniqueOnlineCount());
                }
            }
        }

        private Guid ExtractUserIdFromToken(string? token)
        {
            if (string.IsNullOrWhiteSpace(token)) return Guid.Empty;
            try
            {
                var handler = new JwtSecurityTokenHandler();
                if (handler.CanReadToken(token))
                {
                    var jwtToken = handler.ReadJwtToken(token);
                    var sub = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub || c.Type == ClaimTypes.NameIdentifier)?.Value;
                    if (!string.IsNullOrEmpty(sub) && Guid.TryParse(sub, out var guid))
                    {
                        return guid;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug("[OnlineHub] ExtractUserIdFromToken failed: {Message}", ex.Message);
            }
            return Guid.Empty;
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var connectionId = Context.ConnectionId;
            OnlineUsers.TryRemove(connectionId, out _);

            ConnectionStartTimes.TryRemove(connectionId, out var connectedAt);
            ConnectionUserGuids.TryRemove(connectionId, out var userGuid);

            if (userGuid == Guid.Empty)
            {
                var userIdStr = Context.UserIdentifier
                    ?? Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? Context.User?.FindFirst("sub")?.Value;
                Guid.TryParse(userIdStr, out userGuid);
            }

            if (userGuid != Guid.Empty)
            {
                // Перевіряємо чи у користувача ще залишились активні з'єднання (наприклад інші вкладки)
                bool hasOtherConnections = ConnectionUserGuids.Values.Any(g => g == userGuid);
                if (!hasOtherConnections)
                {
                    var disconnectedAt = DateTime.UtcNow;
                    UserLastSyncTimes.TryRemove(userGuid, out var lastSync);

                    var sessionDurationSeconds = lastSync != default
                        ? (long)Math.Max(0, (disconnectedAt - lastSync).TotalSeconds)
                        : (connectedAt != default ? (long)Math.Max(0, (disconnectedAt - connectedAt).TotalSeconds) : 0);

                    try
                    {
                        await using var dbContext = await _contextFactory.CreateDbContextAsync();

                        var user = await dbContext.Users.FindAsync(userGuid);
                        if (user != null)
                        {
                            user.LastDisconnectedAt = disconnectedAt;
                            user.TotalTimeSpentSeconds += sessionDurationSeconds;

                            _logger.LogInformation(
                                "[OnlineHub] Користувач {Username} вийшов із сайту. Залишок сесії: {SessionSec} сек. Загальний час: {TotalSec} сек",
                                user.Username, sessionDurationSeconds, user.TotalTimeSpentSeconds);

                            int eligibleRewardCount = (int)(user.TotalTimeSpentSeconds / ActiveRewardIntervalSeconds);
                            int pendingMints = eligibleRewardCount - user.TimeRewardNftsMintedCount;

                            if (pendingMints > 0)
                            {
                                int mintsToProcess = Math.Min(pendingMints, 1);

                                var recipientAddress = user.HardhatAddress ?? user.WalletAddress;
                                if (!string.IsNullOrWhiteSpace(recipientAddress))
                                {
                                    _logger.LogInformation(
                                        "[OnlineHub] Користувач {Username} набрав достатньо часу ({TotalSec} сек). Мінтуємо {Count} NFT на адресу {Address}...",
                                        user.Username, user.TotalTimeSpentSeconds, mintsToProcess, recipientAddress);

                                    for (int i = 0; i < mintsToProcess; i++)
                                    {
                                        try
                                        {
                                            int rewardNumber = user.TimeRewardNftsMintedCount + 1;
                                            string desc = ActiveRewardIntervalSeconds >= 3600
                                                ? $"Нагорода за {rewardNumber * (ActiveRewardIntervalSeconds / 3600)} год. активності на сайті"
                                                : ActiveRewardIntervalSeconds >= 60
                                                    ? $"Нагорода за {rewardNumber * (ActiveRewardIntervalSeconds / 60)} хв. активності на сайті (тестовий режим)"
                                                    : $"Нагорода за {rewardNumber * ActiveRewardIntervalSeconds} сек. активності на сайті (тестовий режим)";

                                            var mintedNft = await _nftService.MintToUserAsync(
                                                user.Id,
                                                customDescription: desc);

                                            user.TimeRewardNftsMintedCount++;
                                            _logger.LogInformation(
                                                "[OnlineHub] Успішно змінчено NFT #{TokenId} ({Rarity}) для {Username}",
                                                mintedNft.TokenId, mintedNft.Rarity, user.Username);
                                        }
                                        catch (Exception mintEx)
                                        {
                                            _logger.LogError(mintEx, "[OnlineHub] Помилка мінту NFT для користувача {Username}", user.Username);
                                            break;
                                        }
                                    }
                                }
                            }

                            await dbContext.SaveChangesAsync();
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "[OnlineHub] Помилка збереження даних сесії для {UserId}", userGuid);
                    }
                }
            }

            await Clients.All.SendAsync("UpdateOnlineCount", GetUniqueOnlineCount());

            await base.OnDisconnectedAsync(exception);
        }

        private int GetUniqueOnlineCount()
        {
            return OnlineUsers.Values.Distinct().Count();
        }
    }
}