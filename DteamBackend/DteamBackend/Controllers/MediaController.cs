using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DteamBackend.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DteamBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MediaController : ControllerBase
    {
        private static readonly string[] ImageExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
        private static readonly string[] VideoExtensions = { ".mp4", ".webm", ".mov", ".m4v" };

        private const long MaxImageSizeBytes = 20L * 1024 * 1024;
        private const long MaxVideoSizeBytes = 150L * 1024 * 1024;

        private readonly IS3StorageService _storageService;

        public MediaController(IS3StorageService storageService)
        {
            _storageService = storageService;
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
            await _storageService.UploadAsync(stream, key, file.ContentType, cancellationToken);

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
            var s3Result = await _storageService.GetFileAsync(cleanKey, cancellationToken);
            if (s3Result == null)
            {
                return NotFound(new { message = $"Файл '{cleanKey}' не знайдено." });
            }

            return File(s3Result.Value.Stream, s3Result.Value.ContentType, enableRangeProcessing: true);
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
