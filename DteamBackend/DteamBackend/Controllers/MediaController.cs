using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DteamBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MediaController : ControllerBase
    {
        private static readonly string[] ImageExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
        private static readonly string[] VideoExtensions = { ".mp4", ".webm", ".mov", ".m4v" };

        private const long MaxImageSizeBytes = 20L * 1024 * 1024;
        private const long MaxVideoSizeBytes = 150L * 1024 * 1024;

        [HttpPost("upload")]
        [RequestSizeLimit(MaxVideoSizeBytes)]
        [RequestFormLimits(MultipartBodyLengthLimit = MaxVideoSizeBytes)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<object>> Upload(IFormFile? file)
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

            var webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var uploadsFolder = Path.Combine(webRootPath, "uploads", subfolder);

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var uniqueFileName = $"{Guid.NewGuid():N}{extension}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var relativeUrl = $"/uploads/{subfolder}/{uniqueFileName}";

            return Ok(new
            {
                url = relativeUrl,
                fileName = uniqueFileName,
                size = file.Length,
                type = mediaType,
                message = "Файл успішно завантажено"
            });
        }
    }
}
