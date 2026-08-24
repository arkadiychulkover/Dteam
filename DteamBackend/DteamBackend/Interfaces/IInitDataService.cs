using DteamBackend.Data;

namespace DteamBackend.Interfaces
{
    public interface IInitDataService
    {
        Task InitializeAsync(AppDbContext context);
    }
}
