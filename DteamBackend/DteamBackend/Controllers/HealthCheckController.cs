using DteamBackend.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DteamBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Route("api/health")]
    public class HealthCheckController : ControllerBase
    {
        private readonly AppDbContext _context;

        public HealthCheckController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("check-status")]
        [HttpGet("/api/check-status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<IActionResult> CheckStatus()
        {
            try
            {
                var adminUser = await _context.Users
                    .AsNoTracking()
                    .Where(u => u.IsAdmin)
                    .Select(u => new { u.Id, u.Email, u.Username })
                    .FirstOrDefaultAsync();

                var totalUsers = await _context.Users.CountAsync();
                var totalGames = await _context.Games.CountAsync();

                return Ok(new
                {
                    status = "Healthy",
                    timestamp = DateTime.UtcNow,
                    adminId = adminUser?.Id,
                    totalUsers,
                    totalGames
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new
                {
                    status = "Unhealthy",
                    error = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }
    }
}
