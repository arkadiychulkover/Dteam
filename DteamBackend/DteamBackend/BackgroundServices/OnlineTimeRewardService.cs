using DteamBackend.Data;
using DteamBackend.Hubs;
using DteamBackend.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace DteamBackend.BackgroundServices
{
    public class OnlineTimeRewardService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IHubContext<OnlineHub> _hubContext;
        private readonly ILogger<OnlineTimeRewardService> _logger;

        public OnlineTimeRewardService(
            IServiceScopeFactory scopeFactory,
            IHubContext<OnlineHub> hubContext,
            ILogger<OnlineTimeRewardService> logger)
        {
            _scopeFactory = scopeFactory;
            _hubContext = hubContext;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("[OnlineTimeRewardService] Started background real-time time & NFT reward tracker.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(10000, stoppingToken);
                    await ProcessActiveUsersAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[OnlineTimeRewardService] Error processing active user rewards.");
                }
            }
        }

        private async Task ProcessActiveUsersAsync(CancellationToken ct)
        {
            var activeGuids = OnlineHub.ConnectionUserGuids.Values.Where(g => g != Guid.Empty).Distinct().ToList();
            if (activeGuids.Count == 0) return;

            var now = DateTime.UtcNow;

            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var nftService = scope.ServiceProvider.GetRequiredService<INftService>();

            foreach (var userGuid in activeGuids)
            {
                var lastSync = OnlineHub.UserLastSyncTimes.GetOrAdd(userGuid, now);
                var deltaSeconds = (long)Math.Max(0, (now - lastSync).TotalSeconds);
                if (deltaSeconds <= 0) continue;

                var user = await db.Users.FindAsync(new object[] { userGuid }, ct);
                if (user == null) continue;

                OnlineHub.UserLastSyncTimes[userGuid] = now;
                user.TotalTimeSpentSeconds += deltaSeconds;

                int eligibleRewardCount = (int)(user.TotalTimeSpentSeconds / OnlineHub.ActiveRewardIntervalSeconds);
                int pendingMints = eligibleRewardCount - user.TimeRewardNftsMintedCount;
                long nextMilestoneSec = (user.TimeRewardNftsMintedCount + 1) * OnlineHub.ActiveRewardIntervalSeconds;

                _logger.LogInformation(
                    "[OnlineTimeRewardService] User {Username}: total online = {TotalSec}s (next NFT at {NextSec}s, delta +{Delta}s, minted: {Minted})",
                    user.Username, user.TotalTimeSpentSeconds, nextMilestoneSec, deltaSeconds, user.TimeRewardNftsMintedCount);

                if (pendingMints > 0)
                {
                    int mintsToProcess = Math.Min(pendingMints, 1);

                    var recipientAddress = user.HardhatAddress ?? user.WalletAddress;
                    if (!string.IsNullOrWhiteSpace(recipientAddress))
                    {
                        _logger.LogInformation(
                            "[OnlineTimeRewardService] User {Username} reached {TotalSec}s online. Minting {Count} reward NFT(s)...",
                            user.Username, user.TotalTimeSpentSeconds, mintsToProcess);

                        for (int i = 0; i < mintsToProcess; i++)
                        {
                            try
                            {
                                int rewardNumber = user.TimeRewardNftsMintedCount + 1;
                                string desc = OnlineHub.ActiveRewardIntervalSeconds >= 3600
                                    ? $"Нагорода за {rewardNumber * (OnlineHub.ActiveRewardIntervalSeconds / 3600)} год. активності на сайті"
                                    : OnlineHub.ActiveRewardIntervalSeconds >= 60
                                        ? $"Нагорода за {rewardNumber * (OnlineHub.ActiveRewardIntervalSeconds / 60)} хв. активності на сайті (тестовий режим)"
                                        : $"Нагорода за {rewardNumber * OnlineHub.ActiveRewardIntervalSeconds} сек. активності на сайті (тестовий режим)";

                                var minted = await nftService.MintToUserAsync(user.Id, customDescription: desc);
                                user.TimeRewardNftsMintedCount++;

                                _logger.LogInformation(
                                    "[OnlineTimeRewardService] Minted real-time NFT #{TokenId} ({Rarity}) for user {Username} while online!",
                                    minted.TokenId, minted.Rarity, user.Username);

                                var payload = new
                                {
                                    tokenId = minted.TokenId,
                                    name = minted.Name,
                                    rarity = minted.Rarity,
                                    totalTimeSpent = user.TotalTimeSpentSeconds
                                };

                                // Відправляємо сповіщення на всі активні з'єднання цього користувача
                                var targetConnectionIds = OnlineHub.ConnectionUserGuids
                                    .Where(kvp => kvp.Value == user.Id)
                                    .Select(kvp => kvp.Key)
                                    .ToList();

                                if (targetConnectionIds.Count > 0)
                                {
                                    await _hubContext.Clients.Clients(targetConnectionIds).SendAsync("NftRewardMinted", payload, ct);
                                }
                            }
                            catch (Exception mintEx)
                            {
                                _logger.LogError(mintEx, "[OnlineTimeRewardService] Mint failed for user {Username}", user.Username);
                                break;
                            }
                        }
                    }
                }

                await db.SaveChangesAsync(ct);
            }
        }

        private const long ActiveRewardIntervalSeconds = OnlineHub.ActiveRewardIntervalSeconds;
    }
}
