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

namespace DteamBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommunityController : ControllerBase
    {
        private static readonly string DataFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "community_data.json");
        private static readonly object FileLock = new object();
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

        private class CommunityStore
        {
            public List<CommunityPost> Posts { get; set; } = new List<CommunityPost>();
            public List<CommunityComment> Comments { get; set; } = new List<CommunityComment>();
        }

        private CommunityStore LoadStore()
        {
            lock (FileLock)
            {
                if (!System.IO.File.Exists(DataFilePath))
                {
                    var initialStore = CreateInitialStore();
                    SaveStore(initialStore);
                    return initialStore;
                }

                try
                {
                    var json = System.IO.File.ReadAllText(DataFilePath);
                    return JsonSerializer.Deserialize<CommunityStore>(json) ?? new CommunityStore();
                }
                catch
                {
                    return new CommunityStore();
                }
            }
        }

        private void SaveStore(CommunityStore store)
        {
            lock (FileLock)
            {
                var json = JsonSerializer.Serialize(store, new JsonSerializerOptions { WriteIndented = true });
                System.IO.File.WriteAllText(DataFilePath, json);
            }
        }

        private CommunityStore CreateInitialStore()
        {
            var store = new CommunityStore();

            string[] gameIds = { "lib-1", "lib-2", "lib-3", "lib-4" };

            var author1 = new AuthorDto { Id = "a1", Username = "GamerPro_UA", AvatarUrl = "https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?w=150" };
            var author2 = new AuthorDto { Id = "a2", Username = "cyber_ukraine", AvatarUrl = "https://images.unsplash.com/photo-1570295999919-56ceb5ecca61?w=150" };
            var author3 = new AuthorDto { Id = "a3", Username = "SvelteDev", AvatarUrl = "https://images.unsplash.com/photo-1494790108377-be9c29b29330?w=150" };

            int postIndex = 1;
            foreach (var gId in gameIds)
            {
                store.Posts.Add(new CommunityPost
                {
                    Id = $"post-{postIndex++}",
                    GameId = gId,
                    Author = author1,
                    CreatedAt = DateTime.UtcNow.AddDays(-2),
                    Category = "forum",
                    Title = "Кращі білди для початківців",
                    Content = "Всім привіт! Я зібрав кілька крутих збалансованих білдів для комфортного старту гри. Сподіваюся, це допоможе новачкам швидко розібратися.",
                    Media = new PostMedia { Type = "none" },
                    LikedByUsers = new List<string> { "a2" }
                });

                store.Posts.Add(new CommunityPost
                {
                    Id = $"post-{postIndex++}",
                    GameId = gId,
                    Author = author2,
                    CreatedAt = DateTime.UtcNow.AddDays(-1),
                    Category = "screenshots",
                    Title = "Мій новий рекорд!",
                    Content = "Неймовірні краєвиди та чудова графіка. Просто подивіться на це освітлення!",
                    Media = new PostMedia { Type = "image", Url = "https://images.unsplash.com/photo-1542751371-adc38448a05e?w=1200" },
                    LikedByUsers = new List<string> { "a1", "a3" }
                });

                store.Posts.Add(new CommunityPost
                {
                    Id = $"post-{postIndex++}",
                    GameId = gId,
                    Author = author3,
                    CreatedAt = DateTime.UtcNow.AddHours(-12),
                    Category = "videos",
                    Title = "Детальний відеоогляд механік",
                    Content = "Записав повний розбір бойової системи та крафту. Дивіться відео!",
                    Media = new PostMedia { Type = "video", Url = "https://www.youtube.com/watch?v=dQw4w9WgXcQ" },
                    LikedByUsers = new List<string>()
                });

                store.Posts.Add(new CommunityPost
                {
                    Id = $"post-{postIndex++}",
                    GameId = gId,
                    Author = author1,
                    CreatedAt = DateTime.UtcNow.AddHours(-4),
                    Category = "guides",
                    Title = "Гайд по секретних квестах та ачівках",
                    Content = "Повний перелік прихованих завдань, які легко пропустити при першому проходженні. Читайте та зберігайте собі!",
                    Media = new PostMedia { Type = "image", Url = "https://images.unsplash.com/photo-1550745165-9bc0b252726f?w=1200" },
                    LikedByUsers = new List<string> { "a2" }
                });

                store.Posts.Add(new CommunityPost
                {
                    Id = $"post-{postIndex++}",
                    GameId = gId,
                    Author = new AuthorDto { Id = "admin", Username = "Розробники", AvatarUrl = "https://images.unsplash.com/photo-1614728894747-a83421e2b9c9?w=150" },
                    CreatedAt = DateTime.UtcNow.AddHours(-2),
                    Category = "news",
                    Title = "Офіційне оновлення: Патч 1.2 вже доступний!",
                    Content = "Ми раді представити велике оновлення гри. Виправлено баланс зброї, покращено продуктивність на слабких ПК та додано нові рівні складності.",
                    Media = new PostMedia { Type = "image", Url = "https://images.unsplash.com/photo-1511512578047-dfb367046420?w=1200" },
                    LikedByUsers = new List<string> { "a1", "a2", "a3" }
                });
            }

            store.Comments.Add(new CommunityComment
            {
                Id = "c-1",
                PostId = "post-1",
                Author = author2,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                Content = "Дуже дякую за корисні білди! Другий варіант підійшов ідеально.",
                LikesCount = 5,
                Replies = new List<CommunityComment>
                {
                    new CommunityComment
                    {
                        Id = "c-1-r1",
                        PostId = "post-1",
                        Author = author1,
                        CreatedAt = DateTime.UtcNow.AddHours(-18),
                        Content = "Радий, що це стало в пригоді! Звертайся, якщо виникнуть питання."
                    }
                }
            });

            return store;
        }

        [HttpGet("{gameId}/posts")]
        public async Task<IActionResult> GetPosts(
            string gameId,
            [FromQuery] string category = "all",
            [FromQuery] string search = "",
            [FromQuery] string sortBy = "newest")
        {
            var store = LoadStore();
            var userId = GetCurrentUserId().ToString();

            var query = store.Posts.Where(p => p.GameId.Equals(gameId, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(category) && !category.Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(p => p.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrEmpty(search))
            {
                var s = search.ToLower();
                query = query.Where(p => p.Title.ToLower().Contains(s) || p.Content.ToLower().Contains(s));
            }

            var list = query.ToList();
            if (sortBy.Equals("popular", StringComparison.OrdinalIgnoreCase) || sortBy.Equals("rating", StringComparison.OrdinalIgnoreCase))
            {
                list = list.OrderByDescending(p => p.LikedByUsers.Count).ToList();
            }
            else
            {
                list = list.OrderByDescending(p => p.CreatedAt).ToList();
            }

            var postsDto = list.Select(p => new
            {
                id = p.Id,
                gameId = p.GameId,
                author = p.Author,
                createdAt = p.CreatedAt.ToString("dd.MM.yyyy"),
                category = p.Category,
                title = p.Title,
                content = p.Content,
                media = p.Media,
                stats = new
                {
                    likesCount = p.LikedByUsers.Count,
                    commentsCount = store.Comments.Count(c => c.PostId == p.Id) + store.Comments.Where(c => c.PostId == p.Id).Sum(c => c.Replies.Count),
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
        public IActionResult GetAllPosts(
            [FromQuery] string category = "all",
            [FromQuery] string search = "",
            [FromQuery] string sortBy = "newest")
        {
            var store = LoadStore();
            var userId = GetCurrentUserId().ToString();

            var query = store.Posts.AsQueryable();

            if (!string.IsNullOrEmpty(category) && !category.Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(p => p.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrEmpty(search))
            {
                var s = search.ToLower();
                query = query.Where(p => p.Title.ToLower().Contains(s) || p.Content.ToLower().Contains(s));
            }

            var list = query.ToList();
            if (sortBy.Equals("popular", StringComparison.OrdinalIgnoreCase) || sortBy.Equals("rating", StringComparison.OrdinalIgnoreCase))
                list = list.OrderByDescending(p => p.LikedByUsers.Count).ToList();
            else
                list = list.OrderByDescending(p => p.CreatedAt).ToList();

            var postsDto = list.Select(p => new
            {
                id = p.Id,
                gameId = p.GameId,
                author = p.Author,
                createdAt = p.CreatedAt.ToString("dd.MM.yyyy"),
                category = p.Category,
                title = p.Title,
                content = p.Content,
                media = p.Media,
                stats = new
                {
                    likesCount = p.LikedByUsers.Count,
                    commentsCount = store.Comments.Count(c => c.PostId == p.Id) + store.Comments.Where(c => c.PostId == p.Id).Sum(c => c.Replies.Count),
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
        public IActionResult GetPostDetails(string postId)
        {
            var store = LoadStore();
            var userId = GetCurrentUserId().ToString();

            var post = store.Posts.FirstOrDefault(p => p.Id == postId);
            if (post == null)
            {
                return NotFound(new { message = "Пост не знайдено." });
            }

            var postComments = store.Comments
                .Where(c => c.PostId == postId)
                .OrderByDescending(c => c.CreatedAt)
                .ToList();

            return Ok(new
            {
                post = new
                {
                    id = post.Id,
                    gameId = post.GameId,
                    author = post.Author,
                    createdAt = post.CreatedAt.ToString("dd.MM.yyyy"),
                    category = post.Category,
                    title = post.Title,
                    content = post.Content,
                    media = post.Media,
                    stats = new
                    {
                        likesCount = post.LikedByUsers.Count,
                        commentsCount = postComments.Count + postComments.Sum(c => c.Replies.Count),
                        isLiked = post.LikedByUsers.Contains(userId)
                    }
                },
                comments = postComments.Select(c => new
                {
                    id = c.Id,
                    postId = c.PostId,
                    author = c.Author,
                    createdAt = c.CreatedAt.ToString("dd.MM.yyyy HH:mm"),
                    content = c.Content,
                    likesCount = c.LikesCount,
                    isLiked = c.LikedByUsers.Contains(userId),
                    replies = c.Replies.Select(r => new
                    {
                        id = r.Id,
                        author = r.Author,
                        createdAt = r.CreatedAt.ToString("dd.MM.yyyy HH:mm"),
                        content = r.Content
                    })
                })
            });
        }

        private static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
        private static readonly string[] AllowedVideoExtensions = { ".mp4", ".webm", ".mov", ".m4v" };

        private async Task<string?> SaveCommunityFileAsync(IFormFile? file)
        {
            if (file == null || file.Length == 0) return null;

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedImageExtensions.Contains(ext) && !AllowedVideoExtensions.Contains(ext))
            {
                return null;
            }

            var uniqueFileName = $"{Guid.NewGuid():N}{ext}";
            var folders = new HashSet<string>
            {
                Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "comunity"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot", "comunity")
            };

            foreach (var folder in folders)
            {
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                var filePath = Path.Combine(folder, uniqueFileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
            }

            return $"/comunity/{uniqueFileName}";
        }

        [HttpPost("upload")]
        [RequestSizeLimit(150L * 1024 * 1024)]
        [RequestFormLimits(MultipartBodyLengthLimit = 150L * 1024 * 1024)]
        public async Task<IActionResult> UploadMedia(IFormFile? file)
        {
            var uploadedFile = file ?? Request.Form.Files.FirstOrDefault();
            if (uploadedFile == null || uploadedFile.Length == 0)
            {
                return BadRequest(new { message = "Файл не передано або він порожній." });
            }

            var savedUrl = await SaveCommunityFileAsync(uploadedFile);
            if (string.IsNullOrEmpty(savedUrl))
            {
                return BadRequest(new { message = "Непідтримуваний формат файлу. Дозволені зображення та відео." });
            }

            var ext = Path.GetExtension(uploadedFile.FileName).ToLowerInvariant();
            var isVideo = AllowedVideoExtensions.Contains(ext);

            return Ok(new
            {
                url = savedUrl,
                fileName = Path.GetFileName(savedUrl),
                type = isVideo ? "video" : "image"
            });
        }

        public class CreatePostDto
        {
            public string Category { get; set; } = "forum";
            public string Title { get; set; } = string.Empty;
            public string Content { get; set; } = string.Empty;
            public string MediaType { get; set; } = "none";
            public string MediaUrl { get; set; } = string.Empty;
            public string? MediaThumbnailUrl { get; set; }
            public IFormFile? File { get; set; }
            public IFormFile? MediaFile { get; set; }
            public IFormFile? ThumbnailFile { get; set; }
        }

        [HttpPost("{gameId}/posts")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CreatePostForm(string gameId, [FromForm] CreatePostDto dto)
        {
            return await CreatePostInternal(gameId, dto);
        }

        [HttpPost("{gameId}/posts")]
        [Consumes("application/json")]
        public async Task<IActionResult> CreatePostJson(string gameId, [FromBody] CreatePostDto dto)
        {
            return await CreatePostInternal(gameId, dto);
        }

        [HttpPost("posts")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CreateProfilePostForm([FromForm] CreatePostDto dto)
        {
            return await CreatePostInternal(string.Empty, dto);
        }

        [HttpPost("posts")]
        [Consumes("application/json")]
        public async Task<IActionResult> CreateProfilePostJson([FromBody] CreatePostDto dto)
        {
            return await CreatePostInternal(string.Empty, dto);
        }

        private async Task<IActionResult> CreatePostInternal(string gameId, CreatePostDto dto)
        {
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
            var store = LoadStore();

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
                GameId = gameId,
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

            store.Posts.Add(newPost);
            SaveStore(store);

            return Ok(newPost);
        }

        [HttpPost("posts/{postId}/like")]
        public IActionResult ToggleLikePost(string postId)
        {
            var userId = GetCurrentUserId().ToString();
            if (userId == Guid.Empty.ToString())
            {
                return Unauthorized(new { message = "Необхідно авторизуватися." });
            }

            var store = LoadStore();
            var post = store.Posts.FirstOrDefault(p => p.Id == postId);
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

            SaveStore(store);

            return Ok(new
            {
                liked,
                likesCount = post.LikedByUsers.Count
            });
        }

        public class CreateCommentDto
        {
            public string Content { get; set; } = string.Empty;
        }

        [HttpPost("posts/{postId}/comments")]
        public async Task<IActionResult> AddComment(string postId, [FromBody] CreateCommentDto dto)
        {
            var userId = GetCurrentUserId();
            var store = LoadStore();

            var post = store.Posts.FirstOrDefault(p => p.Id == postId);
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
                LikedByUsers = new List<string>(),
                Replies = new List<CommunityComment>()
            };

            store.Comments.Add(newComment);
            SaveStore(store);

            return Ok(newComment);
        }

        [HttpPost("comments/{commentId}/reply")]
        public async Task<IActionResult> AddReply(string commentId, [FromBody] CreateCommentDto dto)
        {
            var userId = GetCurrentUserId();
            var store = LoadStore();

            var comment = store.Comments.FirstOrDefault(c => c.Id == commentId);
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
                Author = author,
                CreatedAt = DateTime.UtcNow,
                Content = dto.Content
            };

            comment.Replies.Add(newReply);
            SaveStore(store);

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

    public class CommunityPost
    {
        public string Id { get; set; } = string.Empty;
        public string GameId { get; set; } = string.Empty;
        public AuthorDto Author { get; set; } = new AuthorDto();
        public DateTime CreatedAt { get; set; }
        public string Category { get; set; } = "forum";
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public PostMedia Media { get; set; } = new PostMedia();
        public List<string> LikedByUsers { get; set; } = new List<string>();
    }

    public class AuthorDto
    {
        public string Id { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string AvatarUrl { get; set; } = string.Empty;
    }

    public class PostMedia
    {
        public string Type { get; set; } = "none";
        public string Url { get; set; } = string.Empty;
        public string ThumbnailUrl { get; set; } = string.Empty;
    }

    public class PostStats
    {
        public int LikesCount { get; set; }
        public int CommentsCount { get; set; }
        public bool IsLiked { get; set; }
    }

    public class CommunityComment
    {
        public string Id { get; set; } = string.Empty;
        public string PostId { get; set; } = string.Empty;
        public AuthorDto Author { get; set; } = new AuthorDto();
        public DateTime CreatedAt { get; set; }
        public string Content { get; set; } = string.Empty;
        public int LikesCount { get; set; }
        public List<string> LikedByUsers { get; set; } = new List<string>();
        public List<CommunityComment> Replies { get; set; } = new List<CommunityComment>();
    }
}
