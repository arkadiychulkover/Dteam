using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using DteamBackend.Configuration;
using DteamBackend.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DteamBackend.Services
{
    public class S3StorageService : IS3StorageService, IDisposable
    {
        private readonly IAmazonS3? _s3Client;
        private readonly string _bucketName;
        private readonly ILogger<S3StorageService> _logger;
        private bool _disposed;

        public S3StorageService(IOptions<R2Options> options, ILogger<S3StorageService> logger)
        {
            _logger = logger;

            var r2 = options.Value ?? new R2Options();

            var accessKey = Environment.GetEnvironmentVariable("R2_ACCESS_KEY") ?? r2.AccessKey;
            var secretKey = Environment.GetEnvironmentVariable("R2_SECRET_KEY") ?? r2.SecretKey;
            var accountId = Environment.GetEnvironmentVariable("R2_ACCOUNT_ID") ?? r2.AccountId;
            _bucketName = Environment.GetEnvironmentVariable("R2_BUCKET_NAME") ?? r2.BucketName;
            var serviceUrl = Environment.GetEnvironmentVariable("R2_SERVICE_URL")
                ?? (!string.IsNullOrWhiteSpace(accountId) ? $"https://{accountId}.r2.cloudflarestorage.com" : r2.ServiceUrl);

            if (string.IsNullOrWhiteSpace(accessKey) || string.IsNullOrWhiteSpace(secretKey))
            {
                _logger.LogWarning("[S3StorageService] R2/S3 credentials are missing. Set R2_ACCESS_KEY and R2_SECRET_KEY environment variables.");
                _s3Client = null;
            }
            else
            {
                var credentials = new BasicAWSCredentials(accessKey, secretKey);
                var config = new AmazonS3Config
                {
                    ServiceURL = serviceUrl,
                    UseHttp = false,
                    AuthenticationRegion = "auto",
                    ForcePathStyle = true
                };

                _s3Client = new AmazonS3Client(credentials, config);
                _logger.LogInformation("[S3StorageService] Initialized with bucket '{Bucket}' and endpoint '{Endpoint}'.", _bucketName, serviceUrl);
            }
        }

        private IAmazonS3 EnsureClient()
        {
            if (_s3Client == null)
            {
                throw new InvalidOperationException("R2/S3 сховище не налаштовано. Перевірте змінні середовища R2_ACCESS_KEY та R2_SECRET_KEY.");
            }
            return _s3Client;
        }

        private static string NormalizeKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return string.Empty;
            return key.Trim().Replace('\\', '/').TrimStart('/');
        }

        public async Task<string> UploadAsync(Stream stream, string key, string contentType, CancellationToken cancellationToken = default)
        {
            var client = EnsureClient();
            var cleanKey = NormalizeKey(key);

            if (string.IsNullOrWhiteSpace(cleanKey))
            {
                throw new ArgumentException("Ключ файлу не може бути порожнім.", nameof(key));
            }

            var putRequest = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = cleanKey,
                InputStream = stream,
                ContentType = string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType,
                DisablePayloadSigning = true
            };

            await client.PutObjectAsync(putRequest, cancellationToken);
            _logger.LogDebug("[S3StorageService] Uploaded object '{Key}' to bucket '{Bucket}'.", cleanKey, _bucketName);
            return cleanKey;
        }

        public async Task<string> UploadAsync(byte[] bytes, string key, string contentType, CancellationToken cancellationToken = default)
        {
            using var ms = new MemoryStream(bytes);
            return await UploadAsync(ms, key, contentType, cancellationToken);
        }

        public async Task<(Stream Stream, string ContentType, long ContentLength)?> GetFileAsync(string key, CancellationToken cancellationToken = default)
        {
            var client = EnsureClient();
            var cleanKey = NormalizeKey(key);

            if (string.IsNullOrWhiteSpace(cleanKey))
            {
                return null;
            }

            try
            {
                var getRequest = new GetObjectRequest
                {
                    BucketName = _bucketName,
                    Key = cleanKey
                };

                var response = await client.GetObjectAsync(getRequest, cancellationToken);
                var streamWrapper = new S3ResponseStream(response);
                var contentType = response.Headers.ContentType ?? "application/octet-stream";
                var contentLength = response.ContentLength;

                return (streamWrapper, contentType, contentLength);
            }
            catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound || ex.ErrorCode == "NoSuchKey")
            {
                _logger.LogDebug("[S3StorageService] Object '{Key}' not found in bucket '{Bucket}'.", cleanKey, _bucketName);
                return null;
            }
        }

        public async Task<bool> DeleteFileAsync(string key, CancellationToken cancellationToken = default)
        {
            var client = EnsureClient();
            var cleanKey = NormalizeKey(key);

            if (string.IsNullOrWhiteSpace(cleanKey))
            {
                return false;
            }

            try
            {
                var deleteRequest = new DeleteObjectRequest
                {
                    BucketName = _bucketName,
                    Key = cleanKey
                };

                await client.DeleteObjectAsync(deleteRequest, cancellationToken);
                _logger.LogDebug("[S3StorageService] Deleted object '{Key}' from bucket '{Bucket}'.", cleanKey, _bucketName);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[S3StorageService] Failed to delete object '{Key}' from bucket '{Bucket}'.", cleanKey, _bucketName);
                return false;
            }
        }

        public async Task<bool> FileExistsAsync(string key, CancellationToken cancellationToken = default)
        {
            var client = EnsureClient();
            var cleanKey = NormalizeKey(key);

            if (string.IsNullOrWhiteSpace(cleanKey))
            {
                return false;
            }

            try
            {
                var metaRequest = new GetObjectMetadataRequest
                {
                    BucketName = _bucketName,
                    Key = cleanKey
                };

                await client.GetObjectMetadataAsync(metaRequest, cancellationToken);
                return true;
            }
            catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound || ex.ErrorCode == "NoSuchKey")
            {
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[S3StorageService] Error checking existence for object '{Key}'.", cleanKey);
                return false;
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _s3Client?.Dispose();
                _disposed = true;
            }
            GC.SuppressFinalize(this);
        }
    }

    internal sealed class S3ResponseStream : Stream
    {
        private readonly GetObjectResponse _response;
        private readonly Stream _innerStream;
        private bool _disposed;

        public S3ResponseStream(GetObjectResponse response)
        {
            _response = response ?? throw new ArgumentNullException(nameof(response));
            _innerStream = response.ResponseStream ?? throw new ArgumentNullException(nameof(response.ResponseStream));
        }

        public override bool CanRead => _innerStream.CanRead;
        public override bool CanSeek => _innerStream.CanSeek;
        public override bool CanWrite => _innerStream.CanWrite;
        public override long Length => _innerStream.Length;
        public override long Position { get => _innerStream.Position; set => _innerStream.Position = value; }

        public override void Flush() => _innerStream.Flush();
        public override Task FlushAsync(CancellationToken cancellationToken) => _innerStream.FlushAsync(cancellationToken);
        public override int Read(byte[] buffer, int offset, int count) => _innerStream.Read(buffer, offset, count);
        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) => _innerStream.ReadAsync(buffer, offset, count, cancellationToken);
        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) => _innerStream.ReadAsync(buffer, cancellationToken);
        public override long Seek(long offset, SeekOrigin origin) => _innerStream.Seek(offset, origin);
        public override void SetLength(long value) => _innerStream.SetLength(value);
        public override void Write(byte[] buffer, int offset, int count) => _innerStream.Write(buffer, offset, count);
        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) => _innerStream.WriteAsync(buffer, offset, count, cancellationToken);
        public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default) => _innerStream.WriteAsync(buffer, cancellationToken);

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _innerStream.Dispose();
                    _response.Dispose();
                }
                _disposed = true;
            }
            base.Dispose(disposing);
        }

        public override async ValueTask DisposeAsync()
        {
            if (!_disposed)
            {
                await _innerStream.DisposeAsync();
                _response.Dispose();
                _disposed = true;
            }
            await base.DisposeAsync();
        }
    }
}
