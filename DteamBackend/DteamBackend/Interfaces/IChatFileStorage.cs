namespace DteamBackend.Interfaces
{
    public interface IChatFileStorage
    {
        Task<string> SaveFileAsync(Stream stream, string originalFileName, string contentType, CancellationToken cancellationToken = default);
        Task<(Stream Stream, string ContentType)?> GetFileStreamAsync(string storageKey, CancellationToken cancellationToken = default);
        Task<bool> DeleteFileAsync(string storageKey, CancellationToken cancellationToken = default);
    }
}
