using DteamBackend.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DteamBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UserController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("is-banned")]
        public async Task<IActionResult> CheckIsBanned([FromQuery] Guid? userId)
        {
            if (userId == null || userId == Guid.Empty)
            {
                return BadRequest(new { message = "Параметр userId обязателен" });
            }

            var user = await _context.Users
                .AsNoTracking()
                .Where(u => u.Id == userId.Value)
                .Select(u => new
                {
                    UserId = u.Id,
                    u.Username,
                    u.Email,
                    u.IsBanned,
                    u.IsAdmin
                })
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound(new { message = $"Пользователь с ID '{userId}' не найден" });
            }

            return Ok(new
            {
                userId = user.UserId,
                username = user.Username,
                email = user.Email,
                isBanned = user.IsBanned,
                isAdmin = user.IsAdmin
            });
        }
    }
}
