using DteamBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using DteamBackend.Models.DTO;

namespace DteamBackend.Controllers
{
    [ApiController]
    [Route("api/recommendations")]
    [Authorize]
    public class RecommendationsController : ControllerBase
    {
        private readonly RecommendationService _recommendations;

        public RecommendationsController(RecommendationService recommendations)
        {
            _recommendations = recommendations;
        }

        private Guid CurrentUserId =>
            Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet]
        public async Task<IActionResult> GetRecommended([FromQuery] int take = 24, [FromQuery] int skip = 0)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Guid.TryParse(userIdClaim, out var userId);

            var games = await _recommendations.GetRecommendedGamesAsync(userId, take, skip);
            return Ok(games);
        }


        [HttpPost("track/{gameId}")]
        public async Task<IActionResult> TrackAction(Guid gameId, [FromQuery] TasteAction action)
        {
            await _recommendations.RegisterActionAsync(CurrentUserId, gameId, action);
            return NoContent();
        }
    }
}
