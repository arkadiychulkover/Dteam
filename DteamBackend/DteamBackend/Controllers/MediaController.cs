using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DteamBackend.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DteamBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [EnableCors("DteamCorsPolicy")]
    public class MediaController : ControllerBase
    {
        private static readonly string[] ImageExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
        private static readonly string[] VideoExtensions = { ".mp4", ".webm", ".mov", ".m4v" };

        private const long MaxImageSizeBytes = 20L * 1024 * 1024;
        private const long MaxVideoSizeBytes = 150L * 1024 * 1024;

        private readonly IS3StorageService _storageService;
        private readonly Microsoft.AspNetCore.Hosting.IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<MediaController> _logger;

        public MediaController(
            IS3StorageService storageService,
            Microsoft.AspNetCore.Hosting.IWebHostEnvironment webHostEnvironment,
            ILogger<MediaController> logger)
        {
            _storageService = storageService;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }

        [HttpPost("upload")]
        [Authorize]
        [RequestSizeLimit(MaxVideoSizeBytes)]
        [RequestFormLimits(MultipartBodyLengthLimit = MaxVideoSizeBytes)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<object>> Upload(IFormFile? file, CancellationToken cancellationToken)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "Файл не передано або він порожній." });
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            string mediaType;
            string subfolder;
            long maxSize;

            if (ImageExtensions.Contains(extension))
            {
                mediaType = "image";
                subfolder = "images";
                maxSize = MaxImageSizeBytes;
            }
            else if (VideoExtensions.Contains(extension))
            {
                mediaType = "video";
                subfolder = "videos";
                maxSize = MaxVideoSizeBytes;
            }
            else
            {
                return BadRequest(new
                {
                    message = "Непідтримуваний формат файлу. Дозволені зображення (.jpg, .jpeg, .png, .webp, .gif) або відео (.mp4, .webm, .mov, .m4v)."
                });
            }

            if (file.Length > maxSize)
            {
                return BadRequest(new { message = $"Розмір файлу перевищує ліміт {maxSize / (1024 * 1024)} МБ." });
            }

            var uniqueFileName = $"{Guid.NewGuid():N}{extension}";
            var key = $"uploads/{subfolder}/{uniqueFileName}";

            await using var stream = file.OpenReadStream();
            try
            {
                await _storageService.UploadAsync(stream, key, file.ContentType, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[MediaController] S3 upload failed for '{Key}', falling back to local wwwroot.", key);
                var webRoot = _webHostEnvironment.WebRootPath ?? Path.Combine(AppContext.BaseDirectory, "wwwroot");
                var localFilePath = Path.Combine(webRoot, "uploads", subfolder, uniqueFileName);
                Directory.CreateDirectory(Path.GetDirectoryName(localFilePath)!);

                if (stream.CanSeek)
                {
                    stream.Position = 0;
                }

                await using var localFileStream = new FileStream(localFilePath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true);
                await stream.CopyToAsync(localFileStream, cancellationToken);
            }

            var mediaUrl = $"/api/media/{key}";

            return Ok(new
            {
                url = mediaUrl,
                key = key,
                fileName = uniqueFileName,
                size = file.Length,
                type = mediaType,
                message = "Файл успішно завантажено"
            });
        }

        [AllowAnonymous]
        [HttpGet("{*key}")]
        public async Task<IActionResult> GetFile(string key, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return BadRequest(new { message = "Ключ файлу не вказано." });
            }

            var cleanKey = key.TrimStart('/', '\\');

            var webRoot = _webHostEnvironment.WebRootPath ?? Path.Combine(AppContext.BaseDirectory, "wwwroot");
            var localFilePath = Path.Combine(webRoot, cleanKey.Replace('/', Path.DirectorySeparatorChar));

            if (!System.IO.File.Exists(localFilePath))
            {
                string? altKey = null;
                if (cleanKey.Contains("deepsea", StringComparison.OrdinalIgnoreCase))
                {
                    altKey = cleanKey.Replace("deepsea", "deep-sea", StringComparison.OrdinalIgnoreCase);
                }
                else if (cleanKey.Contains("deep-sea", StringComparison.OrdinalIgnoreCase))
                {
                    altKey = cleanKey.Replace("deep-sea", "deepsea", StringComparison.OrdinalIgnoreCase);
                }
                else if (cleanKey.StartsWith("community/", StringComparison.OrdinalIgnoreCase))
                {
                    altKey = "comunity/" + cleanKey.Substring("community/".Length);
                }
                else if (cleanKey.StartsWith("comunity/", StringComparison.OrdinalIgnoreCase))
                {
                    altKey = "community/" + cleanKey.Substring("comunity/".Length);
                }

                if (altKey != null)
                {
                    var altPath = Path.Combine(webRoot, altKey.Replace('/', Path.DirectorySeparatorChar));
                    if (System.IO.File.Exists(altPath))
                    {
                        localFilePath = altPath;
                    }
                }
            }

            if (System.IO.File.Exists(localFilePath))
            {
                var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
                if (!provider.TryGetContentType(localFilePath, out var contentType))
                {
                    contentType = "application/octet-stream";
                }
                return PhysicalFile(Path.GetFullPath(localFilePath), contentType, enableRangeProcessing: true);
            }

            try
            {
                var s3Result = await _storageService.GetFileAsync(cleanKey, cancellationToken);
                if (s3Result != null)
                {
                    return File(s3Result.Value.Stream, s3Result.Value.ContentType, enableRangeProcessing: true);
                }
            }
            catch
            {

            }

            if (cleanKey.StartsWith("avatar_", StringComparison.OrdinalIgnoreCase))
            {
                var iconKey = $"icons/{cleanKey}";
                var localIconPath = Path.Combine(webRoot, iconKey.Replace('/', Path.DirectorySeparatorChar));
                if (System.IO.File.Exists(localIconPath))
                {
                    var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
                    if (!provider.TryGetContentType(localIconPath, out var contentType))
                    {
                        contentType = "application/octet-stream";
                    }
                    return PhysicalFile(Path.GetFullPath(localIconPath), contentType, enableRangeProcessing: true);
                }

                try
                {
                    var s3IconResult = await _storageService.GetFileAsync(iconKey, cancellationToken);
                    if (s3IconResult != null)
                    {
                        return File(s3IconResult.Value.Stream, s3IconResult.Value.ContentType, enableRangeProcessing: true);
                    }
                }
                catch
                {
                }
            }

            if (cleanKey.Contains("avatar", StringComparison.OrdinalIgnoreCase))
            {
                var svg = "<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 100 100\"><defs><linearGradient id=\"g\" x1=\"0%\" y1=\"0%\" x2=\"100%\" y2=\"100%\"><stop offset=\"0%\" stop-color=\"#0891b2\"/><stop offset=\"100%\" stop-color=\"#0284c7\"/></linearGradient></defs><rect width=\"100\" height=\"100\" rx=\"24\" fill=\"#082029\"/><circle cx=\"50\" cy=\"40\" r=\"18\" fill=\"url(#g)\"/><path d=\"M22 84 c0-15 12-26 28-26 s28 11 28 26\" fill=\"url(#g)\"/></svg>";
                return File(System.Text.Encoding.UTF8.GetBytes(svg), "image/svg+xml");
            }

            return NotFound(new { message = $"Файл '{cleanKey}' не знайдено." });
        }

        [AllowAnonymous]
        [HttpGet("/icons/{*key}")]
        public Task<IActionResult> GetIconFile(string key, CancellationToken cancellationToken)
        {
            return GetFile($"icons/{key}", cancellationToken);
        }

        [AllowAnonymous]
        [HttpGet("/avatar_{fileName}")]
        public Task<IActionResult> GetAvatarFile(string fileName, CancellationToken cancellationToken)
        {
            return GetFile($"icons/avatar_{fileName}", cancellationToken);
        }

        [HttpDelete("{*key}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeleteFile(string key, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return BadRequest(new { message = "Ключ файлу не вказано." });
            }

            var cleanKey = key.TrimStart('/', '\\');
            var deleted = await _storageService.DeleteFileAsync(cleanKey, cancellationToken);
            return Ok(new { success = deleted, key = cleanKey, message = deleted ? "Файл успішно видалено." : "Файл не знайдено або не вдалося видалити." });
        }
    }
}
