using System.Security.Claims;
using System.Text.Json;
using DteamBackend.Data;
using Microsoft.EntityFrameworkCore;

namespace DteamBackend.Middlewares
{
    public class CheckBannedUserMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<CheckBannedUserMiddleware> _logger;

        public CheckBannedUserMiddleware(RequestDelegate next, ILogger<CheckBannedUserMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext httpContext, AppDbContext dbContext)
        {
            if (httpContext.User?.Identity?.IsAuthenticated == true)
            {
                var userIdStr = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                             ?? httpContext.User.FindFirst("sub")?.Value;

                if (!string.IsNullOrWhiteSpace(userIdStr) && Guid.TryParse(userIdStr, out Guid userId))
                {
                    var isBanned = await dbContext.Users
                        .AsNoTracking()
                        .Where(u => u.Id == userId)
                        .Select(u => u.IsBanned)
                        .FirstOrDefaultAsync();

                    if (isBanned)
                    {
                        var path = httpContext.Request.Path.Value ?? string.Empty;

                        // Allow checking ban status endpoint so client can verify ban status with 200 OK
                        if (path.Contains("/is-banned", StringComparison.OrdinalIgnoreCase))
                        {
                            httpContext.Response.StatusCode = StatusCodes.Status200OK;
                            httpContext.Response.ContentType = "application/json; charset=utf-8";
                            var banStatusObj = new
                            {
                                userId,
                                isBanned = true,
                                message = "Пользователь заблокирован администратором платформы"
                            };
                            await httpContext.Response.WriteAsync(JsonSerializer.Serialize(banStatusObj));
                            return;
                        }

                        // Allow logout endpoint so banned user can cleanly log out
                        if (path.StartsWith("/api/auth/logout", StringComparison.OrdinalIgnoreCase))
                        {
                            await _next(httpContext);
                            return;
                        }

                        _logger.LogWarning("Заблокированный пользователь {UserId} попытался выполнить запрос: {Path}", userId, httpContext.Request.Path);

                        httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
                        httpContext.Response.ContentType = "application/json; charset=utf-8";

                        var responseObj = new
                        {
                            statusCode = StatusCodes.Status403Forbidden,
                            error = "Forbidden",
                            message = "Ваш аккаунт заблокирован администратором платформы / Your account has been banned by an administrator",
                            isBanned = true,
                            userId
                        };

                        await httpContext.Response.WriteAsync(JsonSerializer.Serialize(responseObj));
                        return;
                    }
                }
            }

            await _next(httpContext);
        }
    }
}
