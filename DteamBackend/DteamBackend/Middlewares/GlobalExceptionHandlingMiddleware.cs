using System.Net;
using System.Text.Json;
using DteamBackend.Interfaces;

namespace DteamBackend.Middlewares
{

    public class GlobalExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

        public GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, ITelegramAlertService telegramAlertService)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Необработанное исключение при выполнении запроса: {Method} {Path}", context.Request.Method, context.Request.Path);

                _ = Task.Run(async () =>
                {
                    try
                    {
                        await telegramAlertService.SendExceptionAlertAsync(ex, context);
                    }
                    catch (Exception tgEx)
                    {
                        _logger.LogError(tgEx, "Ошибка при отправке уведомления об исключении в Telegram");
                    }
                });

                await HandleExceptionResponseAsync(context, ex);
            }
        }

        private static async Task HandleExceptionResponseAsync(HttpContext context, Exception exception)
        {
            if (context.Response.HasStarted)
            {
                return;
            }

            context.Response.ContentType = "application/json; charset=utf-8";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var response = new
            {
                statusCode = context.Response.StatusCode,
                error = "InternalServerError",
                message = "Произошла непредвиденная ошибка на сервере. Команда поддержки уже уведомлена.",
                detail = exception.Message,
                path = context.Request.Path.Value,
                traceId = context.TraceIdentifier,
                timestamp = DateTime.UtcNow
            };

            var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            });

            await context.Response.WriteAsync(json);
        }
    }
}
