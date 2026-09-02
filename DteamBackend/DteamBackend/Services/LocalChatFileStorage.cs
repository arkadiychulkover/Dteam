using DteamBackend.Configuration;
using DteamBackend.Interfaces;
using Microsoft.Extensions.Options;

namespace DteamBackend.Services
{
    public class LocalChatFileStorage : IChatFileStorage
    {
        private readonly string _storageDirectory;
        private readonly ILogger<LocalChatFileStorage> _logger;

        public LocalChatFileStorage(IWebHostEnvironment env, IOptions<ChatOptions> options, ILogger<LocalChatFileStorage> logger)
        {
            _logger = logger;
            var configuredPath = options.Value?.StoragePath ?? "App_Data/ChatStorage";
            _storageDirectory = Path.IsPathRooted(configuredPath)
                ? configuredPath
                : Path.Combine(env.ContentRootPath, configuredPath);

            if (!Directory.Exists(_storageDirectory))
            {
                Directory.CreateDirectory(_storageDirectory);
            }
        }

        public async Task<string> SaveFileAsync(Stream stream, string originalFileName, string contentType, CancellationToken cancellationToken = default)
        {
            var ext = Path.GetExtension(originalFileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(ext))
            {
                ext = ".bin";
            }

            var safeExt = ext.Replace("/", "").Replace("\\", "").Replace("..", "");
            var storageKey = $"{Guid.NewGuid():N}{safeExt}";
            var destinationPath = Path.Combine(_storageDirectory, storageKey);

            var fullPath = Path.GetFullPath(destinationPath);
            if (!fullPath.StartsWith(Path.GetFullPath(_storageDirectory), StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Path traversal violation detected.");
            }

            await using var fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true);
            await stream.CopyToAsync(fileStream, cancellationToken);

            var metaPath = destinationPath + ".meta";
            await File.WriteAllTextAsync(metaPath, contentType ?? "application/octet-stream", cancellationToken);

            return storageKey;
        }

        public Task<(Stream Stream, string ContentType)?> GetFileStreamAsync(string storageKey, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(storageKey) || storageKey.Contains("..") || storageKey.Contains('/') || storageKey.Contains('\\'))
            {
                return Task.FromResult<(Stream Stream, string ContentType)?>(null);
            }

            var filePath = Path.Combine(_storageDirectory, storageKey);
            var fullPath = Path.GetFullPath(filePath);

            if (!fullPath.StartsWith(Path.GetFullPath(_storageDirectory), StringComparison.OrdinalIgnoreCase) || !File.Exists(filePath))
            {
                return Task.FromResult<(Stream Stream, string ContentType)?>(null);
            }

            var contentType = "application/octet-stream";
            var metaPath = filePath + ".meta";
            if (File.Exists(metaPath))
            {
                try
                {
                    contentType = File.ReadAllText(metaPath).Trim();
                }
                catch
                {
                    // Fallback to default
                }
            }

            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, useAsync: true);
            return Task.FromResult<(Stream Stream, string ContentType)?>((fileStream, contentType));
        }

        public Task<bool> DeleteFileAsync(string storageKey, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(storageKey) || storageKey.Contains("..") || storageKey.Contains('/') || storageKey.Contains('\\'))
            {
                return Task.FromResult(false);
            }

            var filePath = Path.Combine(_storageDirectory, storageKey);
            var metaPath = filePath + ".meta";

            bool deleted = false;
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                deleted = true;
            }

            if (File.Exists(metaPath))
            {
                File.Delete(metaPath);
            }

            return Task.FromResult(deleted);
        }
    }
}
