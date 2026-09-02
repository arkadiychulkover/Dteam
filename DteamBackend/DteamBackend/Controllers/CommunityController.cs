using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using DteamBackend.Data;
using DteamBackend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DteamBackend.Services;
using System.ComponentModel.DataAnnotations;

namespace DteamBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommunityController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CommunityController(AppDbContext context)
        {
            _context = context;
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                           ?? User.FindFirst("sub")?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }

        [HttpGet("{gameId}/posts")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPosts(
            string gameId,
            [FromQuery] string category = "all",
            [FromQuery] string search = "",
            [FromQuery] string sortBy = "newest",
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            var userId = GetCurrentUserId().ToString();
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var query = _context.CommunityPosts
                .Include(p => p.Game)
                .AsNoTracking()
                .Where(p => p.GameId.ToLower() == gameId.ToLower());

            if (!string.IsNullOrEmpty(category) && !category.Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(p => p.Category.ToLower() == category.ToLower());
            }

            if (!string.IsNullOrEmpty(search))
            {
                var s = search.ToLower();
                query = query.Where(p => p.Title.ToLower().Contains(s) || p.Content.ToLower().Contains(s));
            }

            List<CommunityPost> posts;
            if (sortBy.Equals("popular", StringComparison.OrdinalIgnoreCase) || sortBy.Equals("rating", StringComparison.OrdinalIgnoreCase))
            {
                var allForSort = await query.ToListAsync();
                posts = allForSort
                    .OrderByDescending(p => p.LikedByUsers.Count)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();
            }
            else
            {
                posts = await query
                    .OrderByDescending(p => p.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }

            var postIds = posts.Select(p => p.Id).ToList();
            var commentCounts = await _context.CommunityComments
                .Where(c => postIds.Contains(c.PostId))
                .GroupBy(c => c.PostId)
                .Select(g => new { PostId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.PostId, x => x.Count);

            var postsDto = posts.Select(p => new
            {
                id = p.Id,
                gameId = p.GameId,
                gameTitle = p.GameTitle ?? p.Game?.Title,
                gameBannerUrl = p.GameBannerUrl ?? p.Game?.HeaderImageUrl ?? p.Game?.CoverImageUrl,
                author = p.Author,
                createdAt = p.CreatedAt.ToString("o"),
                category = p.Category,
                title = p.Title,
                content = p.Content,
                media = p.Media,
                stats = new
                {
                    likesCount = p.LikedByUsers.Count,
                    commentsCount = commentCounts.GetValueOrDefault(p.Id, 0),
                    isLiked = p.LikedByUsers.Contains(userId)
                }
            });

            var game = await _context.Games.FirstOrDefaultAsync(g => g.Id.ToString() == gameId);
            var title = game?.Title ?? "Гра";

            var subscribersCount = Math.Abs(title.GetHashCode() % 15000) + 1200;
            var onlineCount = Math.Abs(title.GetHashCode() % 800) + 45;

            return Ok(new
            {
                gameTitle = title,
                subscribersCount,
                onlineCount,
                posts = postsDto
            });
        }

        [HttpGet("posts")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllPosts(
            [FromQuery] string category = "all",
            [FromQuery] string search = "",
            [FromQuery] string sortBy = "newest")
        {
            var userId = GetCurrentUserId().ToString();
            var query = _context.CommunityPosts
                .Include(p => p.Game)
                .AsQueryable();

            if (!string.IsNullOrEmpty(category) && !category.Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(p => p.Category.ToLower() == category.ToLower());
            }

            if (!string.IsNullOrEmpty(search))
            {
                var s = search.ToLower();
                query = query.Where(p => p.Title.ToLower().Contains(s) || p.Content.ToLower().Contains(s));
            }

            var posts = await query.ToListAsync();

            if (sortBy.Equals("popular", StringComparison.OrdinalIgnoreCase) || sortBy.Equals("rating", StringComparison.OrdinalIgnoreCase))
                posts = posts.OrderByDescending(p => p.LikedByUsers.Count).ToList();
            else
                posts = posts.OrderByDescending(p => p.CreatedAt).ToList();

            var postIds = posts.Select(p => p.Id).ToList();
            var commentCounts = await _context.CommunityComments
                .Where(c => postIds.Contains(c.PostId))
                .GroupBy(c => c.PostId)
                .Select(g => new { PostId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.PostId, x => x.Count);

            var postsDto = posts.Select(p => new
            {
                id = p.Id,
                gameId = p.GameId,
                gameTitle = p.GameTitle ?? p.Game?.Title,
                gameBannerUrl = p.GameBannerUrl ?? p.Game?.HeaderImageUrl ?? p.Game?.CoverImageUrl,
                author = p.Author,
                createdAt = p.CreatedAt.ToString("o"),
                category = p.Category,
                title = p.Title,
                content = p.Content,
                media = p.Media,
                stats = new
                {
                    likesCount = p.LikedByUsers.Count,
                    commentsCount = commentCounts.GetValueOrDefault(p.Id, 0),
                    isLiked = p.LikedByUsers.Contains(userId)
                }
            });

            return Ok(new
            {
                gameTitle = "Спільнота",
                subscribersCount = 0,
                onlineCount = 0,
                posts = postsDto
            });
        }

        [HttpGet("posts/{postId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPostDetails(string postId)
        {
            var userId = GetCurrentUserId().ToString();
            var post = await _context.CommunityPosts
                .Include(p => p.Game)
                .FirstOrDefaultAsync(p => p.Id == postId);

            if (post == null)
            {
                return NotFound(new { message = "Пост не знайдено." });
            }

            var allComments = await _context.CommunityComments
                .Where(c => c.PostId == postId)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();

            var topLevelComments = allComments
                .Where(c => string.IsNullOrEmpty(c.ParentCommentId))
                .OrderByDescending(c => c.CreatedAt)
                .ToList();

            var repliesLookup = allComments
                .Where(c => !string.IsNullOrEmpty(c.ParentCommentId))
                .ToLookup(c => c.ParentCommentId!);

            return Ok(new
            {
                post = new
                {
                    id = post.Id,
                    gameId = post.GameId,
                    gameTitle = post.GameTitle ?? post.Game?.Title,
                    gameBannerUrl = post.GameBannerUrl ?? post.Game?.HeaderImageUrl ?? post.Game?.CoverImageUrl,
                    author = post.Author,
                    createdAt = post.CreatedAt.ToString("o"),
                    category = post.Category,
                    title = post.Title,
                    content = post.Content,
                    media = post.Media,
                    stats = new
                    {
                        likesCount = post.LikedByUsers.Count,
                        commentsCount = allComments.Count,
                        isLiked = post.LikedByUsers.Contains(userId)
                    }
                },
                comments = topLevelComments.Select(c => new
                {
                    id = c.Id,
                    postId = c.PostId,
                    author = c.Author,
                    createdAt = c.CreatedAt.ToString("dd.MM.yyyy HH:mm"),
                    content = c.Content,
                    likesCount = c.LikesCount,
                    isLiked = c.LikedByUsers.Contains(userId),
                    replies = repliesLookup[c.Id].Select(r => new
                    {
                        id = r.Id,
                        author = r.Author,
                        createdAt = r.CreatedAt.ToString("dd.MM.yyyy HH:mm"),
                        content = r.Content,
                        likesCount = r.LikesCount,
                        isLiked = r.LikedByUsers.Contains(userId)
                    })
                })
            });
        }

        private static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
        private static readonly string[] AllowedVideoExtensions = { ".mp4", ".webm", ".mov", ".m4v" };
        private const long MaxImageSizeBytes = 20 * 1024 * 1024;
        private const long MaxVideoSizeBytes = 150 * 1024 * 1024;

        private async Task<string?> SaveCommunityFileAsync(IFormFile? file)
        {
            if (file == null || file.Length == 0) return null;

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedImageExtensions.Contains(ext) && !AllowedVideoExtensions.Contains(ext))
            {
                return null;
            }

            var uniqueFileName = $"{Guid.NewGuid():N}{ext}";
            var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "community");
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            var filePath = Path.Combine(folder, uniqueFileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/community/{uniqueFileName}";
        }

        [HttpPost("upload")]
        [HttpPost("media/upload")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(150L * 1024 * 1024)]
        [RequestFormLimits(MultipartBodyLengthLimit = 150L * 1024 * 1024)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UploadMedia(IFormFile? file)
        {
            var uploadedFile = file ?? Request.Form.Files.FirstOrDefault();
            if (uploadedFile == null || uploadedFile.Length == 0)
            {
                return BadRequest(new { message = "Файл не обрано або він порожній." });
            }

            var ext = Path.GetExtension(uploadedFile.FileName).ToLowerInvariant();
            bool isImage = AllowedImageExtensions.Contains(ext);
            bool isVideo = AllowedVideoExtensions.Contains(ext);

            if (!isImage && !isVideo)
            {
                return BadRequest(new { message = "Непідтримуваний формат файлу." });
            }

            if (isImage && uploadedFile.Length > MaxImageSizeBytes)
            {
                return BadRequest(new { message = $"Розмір зображення не може перевищувати {MaxImageSizeBytes / (1024 * 1024)} МБ." });
            }

            if (isVideo && uploadedFile.Length > MaxVideoSizeBytes)
            {
                return BadRequest(new { message = $"Розмір відео не може перевищувати {MaxVideoSizeBytes / (1024 * 1024)} МБ." });
            }

            var savedUrl = await SaveCommunityFileAsync(uploadedFile);
            if (string.IsNullOrEmpty(savedUrl))
            {
                return StatusCode(500, new { message = "Не вдалося зберегти файл." });
            }

            return Ok(new
            {
                url = savedUrl,
                fileName = Path.GetFileName(savedUrl),
                type = isVideo ? "video" : "image"
            });
        }

        public class CreatePostDto
        {
            [MaxLength(100)]
            public string? GameId { get; set; }

            [Required(ErrorMessage = "Категорія обов'язкова.")]
            [MaxLength(50)]
            public string Category { get; set; } = "forum";

            [Required(ErrorMessage = "Заголовок допису обов'язковий.")]
            [StringLength(200, MinimumLength = 3, ErrorMessage = "Заголовок має містити від 3 до 200 символів.")]
            public string Title { get; set; } = string.Empty;

            [Required(ErrorMessage = "Текст допису обов'язковий.")]
            [MinLength(5, ErrorMessage = "Текст допису має містити щонайменше 5 символів.")]
            public string Content { get; set; } = string.Empty;

            [MaxLength(20)]
            public string MediaType { get; set; } = "none";

            [MaxLength(1000)]
            public string MediaUrl { get; set; } = string.Empty;

            [MaxLength(1000)]
            public string? MediaThumbnailUrl { get; set; }

            public IFormFile? File { get; set; }
            public IFormFile? MediaFile { get; set; }
            public IFormFile? ThumbnailFile { get; set; }
        }

        [HttpPost("{gameId}/posts")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(CommunityPost), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreatePostForm(string gameId, [FromForm] CreatePostDto dto)
        {
            return await CreatePostInternal(gameId, dto);
        }

        [HttpPost("{gameId}/posts")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(CommunityPost), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreatePostJson(string gameId, [FromBody] CreatePostDto dto)
        {
            return await CreatePostInternal(gameId, dto);
        }

        [HttpPost("posts")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(CommunityPost), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateProfilePostForm([FromForm] CreatePostDto dto)
        {
            return await CreatePostInternal(dto.GameId ?? string.Empty, dto);
        }

        [HttpPost("posts")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(CommunityPost), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateProfilePostJson([FromBody] CreatePostDto dto)
        {
            return await CreatePostInternal(dto.GameId ?? string.Empty, dto);
        }

        private async Task<IActionResult> CreatePostInternal(string gameId, CreatePostDto dto)
        {
            var targetGameId = !string.IsNullOrWhiteSpace(gameId) ? gameId : dto.GameId;
            if (string.IsNullOrWhiteSpace(targetGameId))
            {
                return BadRequest(new { message = "Необхідно обрати гру для публікації допису." });
            }

            Game? game = null;
            if (Guid.TryParse(targetGameId, out var parsedGuid))
            {
                game = await _context.Games.FirstOrDefaultAsync(g => g.Id == parsedGuid);
            }
            else
            {
                game = await _context.Games.FirstOrDefaultAsync(g => g.Id.ToString() == targetGameId || g.Title.ToLower() == targetGameId.ToLower());
            }

            if (game == null)
            {
                return BadRequest(new { message = "Вказану гру не знайдено в каталозі. Будь ласка, оберіть діючу гру." });
            }

            var uploadedFile = dto.File ?? dto.MediaFile;
            if (uploadedFile != null && uploadedFile.Length > 0)
            {
                var savedUrl = await SaveCommunityFileAsync(uploadedFile);
                if (!string.IsNullOrEmpty(savedUrl))
                {
                    dto.MediaUrl = savedUrl;
                    var ext = Path.GetExtension(uploadedFile.FileName).ToLowerInvariant();
                    if (AllowedVideoExtensions.Contains(ext))
                    {
                        dto.MediaType = "video";
                    }
                    else
                    {
                        dto.MediaType = "image";
                        if (string.IsNullOrWhiteSpace(dto.MediaThumbnailUrl))
                        {
                            dto.MediaThumbnailUrl = savedUrl;
                        }
                    }
                }
            }

            if (dto.ThumbnailFile != null && dto.ThumbnailFile.Length > 0)
            {
                var savedThumbUrl = await SaveCommunityFileAsync(dto.ThumbnailFile);
                if (!string.IsNullOrEmpty(savedThumbUrl))
                {
                    dto.MediaThumbnailUrl = savedThumbUrl;
                }
            }

            var userId = GetCurrentUserId();
            Duser? user = null;
            if (userId != Guid.Empty)
            {
                user = await _context.Users.FindAsync(userId);
            }

            var author = new AuthorDto
            {
                Id = userId == Guid.Empty ? "guest" : userId.ToString(),
                Username = user?.Username ?? "Гість",
                AvatarUrl = user?.AvatarUrl ?? ""
            };

            string thumbnailUrl;
            if (!string.IsNullOrWhiteSpace(dto.MediaThumbnailUrl))
            {
                thumbnailUrl = dto.MediaThumbnailUrl;
            }
            else if (dto.MediaType == "video")
            {
                var youtubeId = ExtractYoutubeId(dto.MediaUrl);
                thumbnailUrl = !string.IsNullOrEmpty(youtubeId)
                    ? $"https://img.youtube.com/vi/{youtubeId}/hqdefault.jpg"
                    : dto.MediaUrl;
            }
            else
            {
                thumbnailUrl = dto.MediaUrl;
            }

            var newPost = new CommunityPost
            {
                Id = $"post-{Guid.NewGuid():N}",
                GameId = game.Id.ToString(),
                GameGuidId = game.Id,
                GameTitle = game.Title,
                GameBannerUrl = game.HeaderImageUrl ?? game.CoverImageUrl ?? string.Empty,
                Game = game,
                Author = author,
                CreatedAt = DateTime.UtcNow,
                Category = dto.Category,
                Title = dto.Title,
                Content = dto.Content,
                Media = new PostMedia
                {
                    Type = dto.MediaType,
                    Url = dto.MediaUrl,
                    ThumbnailUrl = thumbnailUrl
                },
                LikedByUsers = new List<string>()
            };

            await _context.CommunityPosts.AddAsync(newPost);
            await _context.SaveChangesAsync();

            return Ok(newPost);
        }

        [HttpPost("posts/{postId}/like")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ToggleLikePost(string postId)
        {
            var userId = GetCurrentUserId().ToString();
            if (userId == Guid.Empty.ToString())
            {
                return Unauthorized(new { message = "Необхідно авторизуватися." });
            }

            var post = await _context.CommunityPosts.FirstOrDefaultAsync(p => p.Id == postId);
            if (post == null)
            {
                return NotFound(new { message = "Пост не знайдено." });
            }

            bool liked;
            if (post.LikedByUsers.Contains(userId))
            {
                post.LikedByUsers.Remove(userId);
                liked = false;
            }
            else
            {
                post.LikedByUsers.Add(userId);
                liked = true;
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                liked,
                likesCount = post.LikedByUsers.Count
            });
        }

        public class CreateCommentDto
        {
            [Required(ErrorMessage = "Текст коментаря обов'язковий.")]
            [StringLength(2000, MinimumLength = 1, ErrorMessage = "Коментар має містити від 1 до 2000 символів.")]
            public string Content { get; set; } = string.Empty;
        }

        [HttpPost("posts/{postId}/comments")]
        [ProducesResponseType(typeof(CommunityComment), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddComment(string postId, [FromBody] CreateCommentDto dto)
        {
            var userId = GetCurrentUserId();
            var post = await _context.CommunityPosts.FirstOrDefaultAsync(p => p.Id == postId);
            if (post == null)
            {
                return NotFound(new { message = "Пост не знайдено." });
            }

            Duser? user = null;
            if (userId != Guid.Empty)
            {
                user = await _context.Users.FindAsync(userId);
            }

            var author = new AuthorDto
            {
                Id = userId == Guid.Empty ? "guest" : userId.ToString(),
                Username = user?.Username ?? "Гість",
                AvatarUrl = user?.AvatarUrl ?? ""
            };

            var newComment = new CommunityComment
            {
                Id = $"c-{Guid.NewGuid():N}",
                PostId = postId,
                Author = author,
                CreatedAt = DateTime.UtcNow,
                Content = dto.Content,
                LikesCount = 0,
                LikedByUsers = new List<string>()
            };

            await _context.CommunityComments.AddAsync(newComment);
            await _context.SaveChangesAsync();

            return Ok(newComment);
        }

        [HttpPost("comments/{commentId}/reply")]
        [ProducesResponseType(typeof(CommunityComment), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddReply(string commentId, [FromBody] CreateCommentDto dto)
        {
            var userId = GetCurrentUserId();
            var comment = await _context.CommunityComments.FirstOrDefaultAsync(c => c.Id == commentId);
            if (comment == null)
            {
                return NotFound(new { message = "Коментар не знайдено." });
            }

            Duser? user = null;
            if (userId != Guid.Empty)
            {
                user = await _context.Users.FindAsync(userId);
            }

            var author = new AuthorDto
            {
                Id = userId == Guid.Empty ? "guest" : userId.ToString(),
                Username = user?.Username ?? "Гість",
                AvatarUrl = user?.AvatarUrl ?? ""
            };

            var newReply = new CommunityComment
            {
                Id = $"r-{Guid.NewGuid():N}",
                PostId = comment.PostId,
                ParentCommentId = commentId,
                Author = author,
                CreatedAt = DateTime.UtcNow,
                Content = dto.Content
            };

            await _context.CommunityComments.AddAsync(newReply);
            await _context.SaveChangesAsync();

            return Ok(newReply);
        }

        private string ExtractYoutubeId(string url)
        {
            if (string.IsNullOrEmpty(url)) return string.Empty;
            try
            {
                var uri = new Uri(url);
                var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
                return query["v"] ?? uri.Segments.LastOrDefault() ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
