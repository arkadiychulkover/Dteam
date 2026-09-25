namespace DteamBackend.Interfaces
{
    public interface ITelegramAlertService
    {
        Task<bool> SendAlertAsync(string message);
        Task<bool> SendAlertAsync(string message, string? chatId);
        Task<bool> SendExceptionAlertAsync(Exception ex, HttpContext? httpContext = null);
    }
}
