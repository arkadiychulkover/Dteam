using System.Security.Claims;
using DteamBackend.Interfaces;
using DteamBackend.Models.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DteamBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ActivityController : ControllerBase
    {
        private readonly IActivityService _activityService;

        public ActivityController(IActivityService activityService)
        {
            _activityService = activityService;
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                           ?? User.FindFirst("sub")?.Value;

            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }

        [Authorize]
        [HttpGet("friends")]
        [ProducesResponseType(typeof(List<UserActivityDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<List<UserActivityDto>>> GetFriendsActivities(
            [FromQuery] int limit = 50, 
            [FromQuery] int offset = 0)
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized(new { message = "Користувач не авторизований." });
            }

            var activities = await _activityService.GetFriendsActivitiesAsync(userId, limit, offset);
            return Ok(activities);
        }

        [HttpGet("user/{userId:guid}")]
        [ProducesResponseType(typeof(List<UserActivityDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<UserActivityDto>>> GetUserActivities(
            Guid userId, 
            [FromQuery] int limit = 50, 
            [FromQuery] int offset = 0)
        {
            var activities = await _activityService.GetUserActivitiesAsync(userId, limit, offset);
            return Ok(activities);
        }

        [HttpGet("global")]
        [HttpGet("feed")]
        [ProducesResponseType(typeof(List<UserActivityDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<UserActivityDto>>> GetGlobalFeed(
            [FromQuery] int limit = 50, 
            [FromQuery] int offset = 0)
        {
            var activities = await _activityService.GetGlobalFeedAsync(limit, offset);
            return Ok(activities);
        }
    }
}
