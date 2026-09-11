using DteamBackend.Data;
using DteamBackend.Hubs;
using DteamBackend.Models;
using DteamBackend.Models.DTO;
using Microsoft.EntityFrameworkCore;

namespace DteamBackend.Services
{
    public interface IRewardSettingsService
    {
        Task<RewardSettings> GetSettingsAsync();
        Task<RewardSettings> UpdateSettingsAsync(UpdateRewardSettingsDto dto);
    }

    public class RewardSettingsService : IRewardSettingsService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<RewardSettingsService> _logger;
        private static RewardSettings? _cachedSettings;
        private static readonly SemaphoreSlim _semaphore = new(1, 1);
        private static bool _tableEnsured = false;

        public RewardSettingsService(AppDbContext context, ILogger<RewardSettingsService> logger)
        {
            _context = context;
            _logger = logger;
        }

        private async Task EnsureTableAsync()
        {
            if (_tableEnsured) return;

            try
            {
                var isPostgres = _context.Database.IsNpgsql();
                if (isPostgres)
                {
                    await _context.Database.ExecuteSqlRawAsync(@"
                        CREATE TABLE IF NOT EXISTS ""RewardSettings"" (
                            ""Id"" integer NOT NULL PRIMARY KEY,
                            ""RewardIntervalMinutes"" integer NOT NULL DEFAULT 60,
                            ""TokensPerHour"" numeric NOT NULL DEFAULT 50,
                            ""IsEnabled"" boolean NOT NULL DEFAULT TRUE,
                            ""UpdatedAt"" timestamp with time zone NOT NULL DEFAULT NOW()
                        );
                    ");
                }
                else
                {
                    await _context.Database.ExecuteSqlRawAsync(@"
                        CREATE TABLE IF NOT EXISTS ""RewardSettings"" (
                            ""Id"" INTEGER NOT NULL PRIMARY KEY,
                            ""RewardIntervalMinutes"" INTEGER NOT NULL DEFAULT 60,
                            ""TokensPerHour"" NUMERIC NOT NULL DEFAULT 50,
                            ""IsEnabled"" INTEGER NOT NULL DEFAULT 1,
                            ""UpdatedAt"" TEXT NOT NULL DEFAULT (CURRENT_TIMESTAMP)
                        );
                    ");
                }
                _tableEnsured = true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[RewardSettingsService] Error ensuring RewardSettings table: {Message}", ex.Message);
            }
        }

        public async Task<RewardSettings> GetSettingsAsync()
        {
            if (_cachedSettings != null)
            {
                return _cachedSettings;
            }

            await _semaphore.WaitAsync();
            try
            {
                if (_cachedSettings != null) return _cachedSettings;

                await EnsureTableAsync();

                RewardSettings? settings = null;
                try
                {
                    settings = await _context.RewardSettings.FirstOrDefaultAsync(s => s.Id == 1);
                    if (settings == null)
                    {
                        settings = new RewardSettings
                        {
                            Id = 1,
                            RewardIntervalMinutes = 60,
                            TokensPerHour = 50m,
                            IsEnabled = true,
                            UpdatedAt = DateTime.UtcNow
                        };
                        await _context.RewardSettings.AddAsync(settings);
                        try
                        {
                            await _context.SaveChangesAsync();
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "[RewardSettingsService] Could not save initial settings to DB (table might be initializing)");
                        }
                    }
                }
                catch (Exception dbEx)
                {
                    _logger.LogWarning(dbEx, "[RewardSettingsService] Database query failed. Falling back to default settings.");
                    settings = new RewardSettings
                    {
                        Id = 1,
                        RewardIntervalMinutes = 60,
                        TokensPerHour = 50m,
                        IsEnabled = true,
                        UpdatedAt = DateTime.UtcNow
                    };
                }

                _cachedSettings = settings;
                OnlineHub.ActiveRewardIntervalSeconds = (long)settings.RewardIntervalMinutes * 60;
                return settings;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<RewardSettings> UpdateSettingsAsync(UpdateRewardSettingsDto dto)
        {
            await _semaphore.WaitAsync();
            try
            {
                await EnsureTableAsync();

                RewardSettings? settings = null;
                try
                {
                    settings = await _context.RewardSettings.FirstOrDefaultAsync(s => s.Id == 1);
                    if (settings == null)
                    {
                        settings = new RewardSettings
                        {
                            Id = 1,
                            RewardIntervalMinutes = dto.RewardIntervalMinutes,
                            TokensPerHour = dto.TokensPerHour,
                            IsEnabled = dto.IsEnabled,
                            UpdatedAt = DateTime.UtcNow
                        };
                        await _context.RewardSettings.AddAsync(settings);
                    }
                    else
                    {
                        settings.RewardIntervalMinutes = dto.RewardIntervalMinutes;
                        settings.TokensPerHour = dto.TokensPerHour;
                        settings.IsEnabled = dto.IsEnabled;
                        settings.UpdatedAt = DateTime.UtcNow;
                    }

                    await _context.SaveChangesAsync();
                }
                catch (Exception dbEx)
                {
                    _logger.LogWarning(dbEx, "[RewardSettingsService] Database write failed. Keeping settings in memory.");
                    settings ??= new RewardSettings
                    {
                        Id = 1,
                        RewardIntervalMinutes = dto.RewardIntervalMinutes,
                        TokensPerHour = dto.TokensPerHour,
                        IsEnabled = dto.IsEnabled,
                        UpdatedAt = DateTime.UtcNow
                    };
                    settings.RewardIntervalMinutes = dto.RewardIntervalMinutes;
                    settings.TokensPerHour = dto.TokensPerHour;
                    settings.IsEnabled = dto.IsEnabled;
                    settings.UpdatedAt = DateTime.UtcNow;
                }

                _cachedSettings = settings;
                OnlineHub.ActiveRewardIntervalSeconds = (long)settings.RewardIntervalMinutes * 60;

                _logger.LogInformation(
                    "[RewardSettingsService] Reward settings updated: Interval = {Minutes} min ({Seconds}s), Tokens/Hr = {Tokens}, Enabled = {Enabled}",
                    settings.RewardIntervalMinutes,
                    OnlineHub.ActiveRewardIntervalSeconds,
                    settings.TokensPerHour,
                    settings.IsEnabled);

                return settings;
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}
