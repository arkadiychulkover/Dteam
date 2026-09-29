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
                    var userRecord = await dbContext.Users
                        .AsNoTracking()
                        .Where(u => u.Id == userId)
                        .Select(u => new { u.IsBanned, u.IsAdmin })
                        .FirstOrDefaultAsync();

                    if (userRecord != null && userRecord.IsBanned)
                    {
                        var path = httpContext.Request.Path.Value ?? string.Empty;

                        // 1. Never block logout
                        if (path.StartsWith("/api/auth/logout", StringComparison.OrdinalIgnoreCase))
                        {
                            await _next(httpContext);
                            return;
                        }

                        // 2. Never block ban or admin check queries
                        if (path.Contains("/is-banned", StringComparison.OrdinalIgnoreCase) ||
                            path.Contains("/is-admin", StringComparison.OrdinalIgnoreCase))
                        {
                            await _next(httpContext);
                            return;
                        }

                        // 3. If user is Admin, never block admin paths or essential dashboard data
                        if (userRecord.IsAdmin)
                        {
                            bool isAdminPath =
                                path.Contains("/admin", StringComparison.OrdinalIgnoreCase) ||
                                path.Contains("/is-admin", StringComparison.OrdinalIgnoreCase) ||
                                path.StartsWith("/health", StringComparison.OrdinalIgnoreCase) ||
                                path.StartsWith("/api/auth/me", StringComparison.OrdinalIgnoreCase) ||
                                path.StartsWith("/api/users/me", StringComparison.OrdinalIgnoreCase) ||
                                path.StartsWith("/api/user/me", StringComparison.OrdinalIgnoreCase);

                            if (isAdminPath)
                            {
                                await _next(httpContext);
                                return;
                            }
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
                            isAdmin = userRecord.IsAdmin,
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
