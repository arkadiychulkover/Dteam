using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using DteamBackend.Interfaces;

namespace DteamBackend.Services
{
    /// <summary>
    /// Статический класс для прямой отправки сообщений в Telegram Bot API
    /// </summary>
    public static class TelegramSender
    {
        private static readonly HttpClient HttpClient = new HttpClient();

        public static async Task<bool> SendAsync(string botToken, string chatId, string message)
        {
            if (string.IsNullOrWhiteSpace(botToken) || string.IsNullOrWhiteSpace(chatId) || string.IsNullOrWhiteSpace(message))
            {
                return false;
            }

            try
            {
                var endpoint = $"https://api.telegram.org/bot{botToken}/sendMessage";
                var payload = new
                {
                    chat_id = chatId,
                    text = message,
                    parse_mode = "HTML"
                };

                var json = JsonSerializer.Serialize(payload);
                using var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await HttpClient.PostAsync(endpoint, content);
                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[TelegramSender] Failed to send telegram message: {response.StatusCode} - {errorBody}");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TelegramSender] Exception while sending telegram alert: {ex.Message}");
                return false;
            }
        }

        public static bool Send(string botToken, string chatId, string message)
        {
            return SendAsync(botToken, chatId, message).GetAwaiter().GetResult();
        }
    }

    /// <summary>
    /// Сервис для централизованной отправки алертов и отчетов об ошибках
    /// </summary>
    public class TelegramAlertService : ITelegramAlertService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<TelegramAlertService> _logger;
        private static string? _cachedChatId;

        public TelegramAlertService(IConfiguration configuration, ILogger<TelegramAlertService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> SendAlertAsync(string message)
        {
            var (botToken, chatId) = await ResolveCredentialsAsync(null);
            if (string.IsNullOrWhiteSpace(botToken) || string.IsNullOrWhiteSpace(chatId))
            {
                _logger.LogWarning("[TelegramAlert] Не удалось отправить сообщение: отсутствует BotToken или ChatId.");
                return false;
            }

            return await TelegramSender.SendAsync(botToken, chatId, message);
        }

        public async Task<bool> SendAlertAsync(string message, string? chatId)
        {
            var (botToken, resolvedChatId) = await ResolveCredentialsAsync(chatId);
            if (string.IsNullOrWhiteSpace(botToken) || string.IsNullOrWhiteSpace(resolvedChatId))
            {
                _logger.LogWarning("[TelegramAlert] Не удалось отправить сообщение: отсутствует BotToken или ChatId.");
                return false;
            }

            return await TelegramSender.SendAsync(botToken, resolvedChatId, message);
        }

        public async Task<bool> SendExceptionAlertAsync(Exception ex, HttpContext? httpContext = null)
        {
            try
            {
                var sb = new StringBuilder();
                sb.AppendLine("🚨 <b>[Dteam Backend Exception]</b>");
                sb.AppendLine($"⏰ <b>Time (UTC):</b> {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}");

                if (httpContext != null)
                {
                    var request = httpContext.Request;
                    sb.AppendLine($"🌐 <b>Request:</b> <code>{request.Method} {request.Path}</code>");
                    if (request.QueryString.HasValue)
                    {
                        sb.AppendLine($"🔍 <b>Query:</b> <code>{request.QueryString.Value}</code>");
                    }

                    var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
                    sb.AppendLine($"📍 <b>Client IP:</b> <code>{ip}</code>");

                    var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                              ?? httpContext.User.FindFirst("sub")?.Value;
                    if (!string.IsNullOrWhiteSpace(userId))
                    {
                        sb.AppendLine($"👤 <b>User ID:</b> <code>{userId}</code>");
                    }
                }

                sb.AppendLine($"💥 <b>Type:</b> <code>{EscapeHtml(ex.GetType().Name)}</code>");
                sb.AppendLine($"📝 <b>Message:</b> {EscapeHtml(ex.Message)}");

                if (!string.IsNullOrWhiteSpace(ex.StackTrace))
                {
                    var shortTrace = ex.StackTrace.Length > 800
                        ? ex.StackTrace.Substring(0, 800) + "..."
                        : ex.StackTrace;
                    sb.AppendLine($"<pre><code>{EscapeHtml(shortTrace)}</code></pre>");
                }

                return await SendAlertAsync(sb.ToString());
            }
            catch (Exception alertEx)
            {
                _logger.LogError(alertEx, "[TelegramAlert] Ошибка при формировании алерта об исключении");
                return false;
            }
        }

        private async Task<(string BotToken, string ChatId)> ResolveCredentialsAsync(string? explicitChatId)
        {
            var botToken = _configuration["Telegram:BotToken"]
                        ?? Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN")
                        ?? "8821247034:AAHfD3yoOK3dTcVMtzcFJTJILQQi6YY0RpQ";

            if (!string.IsNullOrWhiteSpace(explicitChatId))
            {
                return (botToken, explicitChatId);
            }

            var chatId = _configuration["Telegram:ChatId"]
                      ?? Environment.GetEnvironmentVariable("TELEGRAM_CHAT_ID")
                      ?? _cachedChatId;

            // Если ChatId не задан явно, пробуем автоматически обнаружить последний чат из getUpdates бота
            if (string.IsNullOrWhiteSpace(chatId) && !string.IsNullOrWhiteSpace(botToken))
            {
                chatId = await TryAutoDiscoverChatIdAsync(botToken);
                if (!string.IsNullOrWhiteSpace(chatId))
                {
                    _cachedChatId = chatId;
                }
            }

            return (botToken, chatId ?? string.Empty);
        }

        private async Task<string?> TryAutoDiscoverChatIdAsync(string botToken)
        {
            try
            {
                using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
                var response = await client.GetAsync($"https://api.telegram.org/bot{botToken}/getUpdates");
                if (response.IsSuccessStatusCode)
                {
                    var jsonStr = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(jsonStr);
                    if (doc.RootElement.TryGetProperty("result", out var results) && results.GetArrayLength() > 0)
                    {
                        for (int i = results.GetArrayLength() - 1; i >= 0; i--)
                        {
                            var update = results[i];
                            if (update.TryGetProperty("message", out var msg) && msg.TryGetProperty("chat", out var chat))
                            {
                                if (chat.TryGetProperty("id", out var idProp))
                                {
                                    var discoveredId = idProp.ToString();
                                    _logger.LogInformation("[TelegramAlert] Автоматически обнаружен Chat ID: {ChatId}", discoveredId);
                                    return discoveredId;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug("[TelegramAlert] Автопоиск Chat ID завершился без результата: {Error}", ex.Message);
            }

            return null;
        }

        private static string EscapeHtml(string text)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            return text
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;");
        }
    }
}
