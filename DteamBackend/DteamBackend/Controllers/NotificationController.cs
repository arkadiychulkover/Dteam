using System.Security.Claims;
using DteamBackend.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DteamBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Route("api/notifications")]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        private Guid? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("id")?.Value
                ?? User.FindFirst("sub")?.Value;

            if (Guid.TryParse(userIdClaim, out var userId))
            {
                return userId;
            }

            return null;
        }

        [HttpGet]
        public async Task<IActionResult> GetFeed([FromQuery] int limit = 30, [FromQuery] DateTime? before = null)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Unauthorized(new { message = "Не авторизовано" });
            }

            var feed = await _notificationService.GetFeedAsync(userId.Value, limit, before);
            return Ok(feed);
        }

        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Unauthorized(new { message = "Не авторизовано" });
            }

            var count = await _notificationService.GetUnreadCountAsync(userId.Value);
            return Ok(new { unreadCount = count });
        }

        [HttpPut("{id:guid}/read")]
        public async Task<IActionResult> MarkAsRead(Guid id)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Unauthorized(new { message = "Не авторизовано" });
            }

            var success = await _notificationService.MarkAsReadAsync(userId.Value, id);
            if (!success)
            {
                return NotFound(new { message = "Сповіщення не знайдено або доступ заборонено" });
            }

            return Ok(new { message = "Сповіщення позначено як прочитане" });
        }

        [HttpPut("read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Unauthorized(new { message = "Не авторизовано" });
            }

            var updatedCount = await _notificationService.MarkAllAsReadAsync(userId.Value);
            return Ok(new { message = "Всі сповіщення позначено як прочитані", count = updatedCount });
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteNotification(Guid id)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Unauthorized(new { message = "Не авторизовано" });
            }

            var success = await _notificationService.DeleteAsync(userId.Value, id);
            if (!success)
            {
                return NotFound(new { message = "Сповіщення не знайдено або доступ заборонено" });
            }

            return Ok(new { message = "Сповіщення видалено" });
        }
    }
}
