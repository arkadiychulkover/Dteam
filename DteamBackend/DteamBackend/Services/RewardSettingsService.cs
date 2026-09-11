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

        public RewardSettingsService(AppDbContext context, ILogger<RewardSettingsService> logger)
        {
            _context = context;
            _logger = logger;
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

                var settings = await _context.RewardSettings.FirstOrDefaultAsync(s => s.Id == 1);
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
                var settings = await _context.RewardSettings.FirstOrDefaultAsync(s => s.Id == 1);
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
