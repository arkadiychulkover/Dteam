using DteamBackend.Data;
using DteamBackend.Models.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DteamBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class RecommendationController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RecommendationController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<GameRecommendationDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<GameRecommendationDto>>> GetRecommendations(
            [FromQuery] string? query,
            [FromQuery] string? q,
            [FromQuery] int limit = 10)
        {
            var searchStr = !string.IsNullOrWhiteSpace(query) ? query : q;
            return await SearchGamesByPrefixAsync(searchStr, limit);
        }

        [HttpGet("{query}")]
        [ProducesResponseType(typeof(IEnumerable<GameRecommendationDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<GameRecommendationDto>>> GetRecommendationsByRoute(
            string query,
            [FromQuery] int limit = 10)
        {
            return await SearchGamesByPrefixAsync(query, limit);
        }

        private async Task<ActionResult<IEnumerable<GameRecommendationDto>>> SearchGamesByPrefixAsync(string? searchStr, int limit)
        {
            if (string.IsNullOrWhiteSpace(searchStr))
            {
                return Ok(new List<GameRecommendationDto>());
            }

            var prefix = searchStr.Trim().ToLower();
            var maxCount = Math.Clamp(limit, 1, 50);

            var games = await _context.Games
                .AsNoTracking()
                .Where(g => g.IsPublished && (
                    g.Title.ToLower().StartsWith(prefix) ||
                    g.Title.ToLower().Contains(prefix) ||
                    (g.ShortDescription != null && g.ShortDescription.ToLower().Contains(prefix)) ||
                    g.Tags.Any(t => t.ToLower().Contains(prefix)) ||

                    g.Genres.Any(gen => gen.ToString().ToLower().Contains(prefix))
                ))
                .OrderByDescending(g => g.Title.ToLower().StartsWith(prefix))
                .ThenBy(g => g.Title)
                .Take(maxCount)
                .Select(g => new GameRecommendationDto
                {
                    Id = g.Id,
                    Title = g.Title,
                    Banner = g.HeaderImageUrl ?? g.CoverImageUrl ?? string.Empty,
                    BannerUrl = g.HeaderImageUrl ?? g.CoverImageUrl ?? string.Empty,
                    HeaderImageUrl = g.HeaderImageUrl,
                    CoverImageUrl = g.CoverImageUrl
                })
                .ToListAsync();

            return Ok(games);
        }
    }
}

