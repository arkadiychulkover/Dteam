using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DteamBackend.Configuration;
using DteamBackend.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DteamBackend.Services
{
    public class S3ChatFileStorage : IChatFileStorage
    {
        private readonly IS3StorageService _s3Storage;
        private readonly string _legacyStorageDirectory;
        private readonly ILogger<S3ChatFileStorage> _logger;

        public S3ChatFileStorage(
            IS3StorageService s3Storage,
            IWebHostEnvironment env,
            IOptions<ChatOptions> options,
            ILogger<S3ChatFileStorage> logger)
        {
            _s3Storage = s3Storage;
            _logger = logger;

            var configuredPath = options.Value?.StoragePath ?? "App_Data/ChatStorage";
            _legacyStorageDirectory = Path.IsPathRooted(configuredPath)
                ? configuredPath
                : Path.Combine(env.ContentRootPath, configuredPath);
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
            var s3Key = $"chat/{storageKey}";

            Directory.CreateDirectory(_legacyStorageDirectory);
            var localFilePath = Path.Combine(_legacyStorageDirectory, storageKey);
            var metaPath = localFilePath + ".meta";

            await using (var fileStream = new FileStream(localFilePath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true))
            {
                await stream.CopyToAsync(fileStream, cancellationToken);
            }

            await File.WriteAllTextAsync(metaPath, contentType ?? "application/octet-stream", cancellationToken);

            try
            {
                await using var s3Stream = new FileStream(localFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, useAsync: true);
                await _s3Storage.UploadAsync(s3Stream, s3Key, contentType ?? "application/octet-stream", cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[S3ChatFileStorage] Failed to upload chat file '{Key}' to S3. Local fallback saved at '{LocalPath}'.", s3Key, localFilePath);
            }

            return storageKey;
        }

        public async Task<(Stream Stream, string ContentType)?> GetFileStreamAsync(string storageKey, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(storageKey) || storageKey.Contains("..") || storageKey.Contains('/') || storageKey.Contains('\\'))
            {
                return null;
            }

            var s3Key = $"chat/{storageKey}";
            try
            {
                var s3Result = await _s3Storage.GetFileAsync(s3Key, cancellationToken);
                if (s3Result != null)
                {
                    return (s3Result.Value.Stream, s3Result.Value.ContentType);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[S3ChatFileStorage] Error fetching '{Key}' from S3. Falling back to local storage.", s3Key);
            }

            if (Directory.Exists(_legacyStorageDirectory))
            {
                var localFilePath = Path.Combine(_legacyStorageDirectory, storageKey);
                if (File.Exists(localFilePath))
                {
                    var contentType = "application/octet-stream";
                    var metaPath = localFilePath + ".meta";
                    if (File.Exists(metaPath))
                    {
                        try
                        {
                            contentType = (await File.ReadAllTextAsync(metaPath, cancellationToken)).Trim();
                        }
                        catch
                        {
                        }
                    }

                    var fileStream = new FileStream(localFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, useAsync: true);
                    return (fileStream, contentType);
                }
            }

            return null;
        }

        public async Task<bool> DeleteFileAsync(string storageKey, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(storageKey) || storageKey.Contains("..") || storageKey.Contains('/') || storageKey.Contains('\\'))
            {
                return false;
            }

            var s3Key = $"chat/{storageKey}";
            var deletedFromS3 = await _s3Storage.DeleteFileAsync(s3Key, cancellationToken);

            try
            {
                if (Directory.Exists(_legacyStorageDirectory))
                {
                    var localFilePath = Path.Combine(_legacyStorageDirectory, storageKey);
                    if (File.Exists(localFilePath))
                    {
                        File.Delete(localFilePath);
                    }
                    var metaPath = localFilePath + ".meta";
                    if (File.Exists(metaPath))
                    {
                        File.Delete(metaPath);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[S3ChatFileStorage] Error deleting legacy local chat file: {StorageKey}", storageKey);
            }

            return deletedFromS3;
        }
    }
}
