using DteamBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DteamBackend.Controllers
{
    [ApiController]
    [Route("api/recommendations")]
    public class RecommendationsController : ControllerBase
    {
        private readonly RecommendationService _recommendations;

        public RecommendationsController(RecommendationService recommendations)
        {
            _recommendations = recommendations;
        }

        private Guid? CurrentUserId
        {
            get
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
                return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetRecommended([FromQuery] int take = 24, [FromQuery] int skip = 0)
        {
            var userId = CurrentUserId ?? Guid.Empty;
            var games = await _recommendations.GetRecommendedGamesAsync(userId, take, skip);
            return Ok(games);
        }

        [HttpPost("track/{gameId}")]
        [Authorize]
        public async Task<IActionResult> TrackAction(Guid gameId, [FromQuery] TasteAction action)
        {
            if (CurrentUserId is { } userId && userId != Guid.Empty)
            {
                await _recommendations.RegisterActionAsync(userId, gameId, action);
            }
            return NoContent();
        }
    }
}
