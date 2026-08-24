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
            string? userIdStr = httpContext.Request.Query["userId"].FirstOrDefault()
                                ?? httpContext.Request.Query["adminId"].FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(userIdStr) && Guid.TryParse(userIdStr, out Guid userId))
            {
                var isBanned = await dbContext.Users
                    .AsNoTracking()
                    .Where(u => u.Id == userId)
                    .Select(u => u.IsBanned)
                    .FirstOrDefaultAsync();

                if (isBanned)
                {
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

            await _next(httpContext);
        }
    }
}
