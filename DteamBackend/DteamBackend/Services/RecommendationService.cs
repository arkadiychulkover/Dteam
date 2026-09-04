using DteamBackend.Data;
using DteamBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace DteamBackend.Services
{
    public enum TasteAction
    {
        ViewGame,
        AddToCart,
        AddToWishlist,
        Purchase
    }

    public class RecommendationService
    {
        private readonly AppDbContext _db;

        private static readonly Dictionary<TasteAction, float> ActionWeights = new()
        {
            [TasteAction.ViewGame] = 0.02f,
            [TasteAction.AddToCart] = 0.08f,
            [TasteAction.AddToWishlist] = 0.12f,
            [TasteAction.Purchase] = 0.30f,
        };

        public RecommendationService(AppDbContext db)
        {
            _db = db;
        }

        public async Task RegisterActionAsync(Guid userId, Guid gameId, TasteAction action)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
            var game = await _db.Games.AsNoTracking().FirstOrDefaultAsync(g => g.Id == gameId);
            if (user is null || game is null) return;

            var lr = ActionWeights[action];

            var userVec = (user.TasteVector != null && user.TasteVector.Length == TasteCategories.Length)
                ? user.TasteVector
                : TasteCategories.Empty();

            var gameVec = (game.TasteVector != null && game.TasteVector.Length == TasteCategories.Length)
                ? game.TasteVector
                : TasteCategories.Empty();

            var updated = new float[TasteCategories.Length];
            for (var i = 0; i < TasteCategories.Length; i++)
            {
                updated[i] = userVec[i] * (1f - lr) + gameVec[i] * lr;
            }

            user.TasteVector = TasteCategories.Normalize(updated);
            await _db.SaveChangesAsync();
        }

        public async Task<List<Game>> GetRecommendedGamesAsync(Guid userId, int take = 24, int skip = 0)
        {
            var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
            var userVec = (user?.TasteVector != null && user.TasteVector.Length == TasteCategories.Length)
                ? user.TasteVector
                : TasteCategories.Empty();

            var ownedGameIds = await _db.UserGames
                .Where(ug => ug.UserId == userId)
                .Select(ug => ug.GameId)
                .ToListAsync();

            var wishlistGameIds = await _db.UserWishlists
                .Where(w => w.UserId == userId)
                .Select(w => w.GameId)
                .ToListAsync();

            var candidates = await _db.Games
                .AsNoTracking()
                .Where(g => g.IsPublished && !g.IsDlc && !ownedGameIds.Contains(g.Id))
                .ToListAsync();

            var wishlistCandidates = candidates
                .Where(g => wishlistGameIds.Contains(g.Id))
                .OrderByDescending(g => DotProduct(userVec, g.TasteVector) + (g.DiscountPercentage > 0 ? 0.05 : 0))
                .Take(3)
                .ToList();

            var newDiscoveryCandidates = candidates
                .Where(g => !wishlistGameIds.Contains(g.Id))
                .OrderByDescending(g => DotProduct(userVec, g.TasteVector) + (g.AverageRating * 0.05))
                .Take(take - wishlistCandidates.Count)
                .ToList();

            var scored = wishlistCandidates
                .Concat(newDiscoveryCandidates)
                .Skip(skip)
                .Take(take)
                .ToList();

            if (scored.Count < take)
            {
                var extra = candidates
                    .Except(scored)
                    .OrderByDescending(g => g.AverageRating)
                    .ThenByDescending(g => g.DownloadCount)
                    .Take(take - scored.Count);
                scored.AddRange(extra);
            }

            return scored;
        }

        private static double DotProduct(float[]? a, float[]? b)
        {
            if (a == null || b == null) return 0;
            var len = Math.Min(a.Length, b.Length);
            double sum = 0;
            for (var i = 0; i < len; i++) sum += a[i] * b[i];
            return sum;
        }

        private static bool IsZeroVector(float[]? v)
            => v == null || v.All(x => Math.Abs(x) < 1e-6f);
    }
}