using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace DteamBackend.Interfaces
{
    public interface IS3StorageService
    {
        
        
        
        Task<string> UploadAsync(Stream stream, string key, string contentType, CancellationToken cancellationToken = default);

        
        
        
        Task<string> UploadAsync(byte[] bytes, string key, string contentType, CancellationToken cancellationToken = default);

        
        
        
        Task<(Stream Stream, string ContentType, long ContentLength)?> GetFileAsync(string key, CancellationToken cancellationToken = default);

        
        
        
        Task<bool> DeleteFileAsync(string key, CancellationToken cancellationToken = default);

        
        
        
        Task<bool> FileExistsAsync(string key, CancellationToken cancellationToken = default);
    }
}
