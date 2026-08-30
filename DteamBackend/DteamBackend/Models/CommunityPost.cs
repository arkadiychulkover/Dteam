using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DteamBackend.Models
{
    public class CommunityPost
    {
        public string Id { get; set; } = string.Empty;

        public string GameId { get; set; } = string.Empty;

        public string? GameTitle { get; set; }

        public string? GameBannerUrl { get; set; }

        public Guid? GameGuidId { get; set; }

        [JsonIgnore]
        public Game? Game { get; set; }

        public AuthorDto Author { get; set; } = new AuthorDto();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string Category { get; set; } = "forum";

        public string Title { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public PostMedia Media { get; set; } = new PostMedia();

        public List<string> LikedByUsers { get; set; } = new List<string>();

        [JsonIgnore]
        public ICollection<CommunityComment> Comments { get; set; } = new List<CommunityComment>();
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

        [JsonIgnore]
        public CommunityPost? Post { get; set; }

        public string? ParentCommentId { get; set; }

        [JsonIgnore]
        public CommunityComment? ParentComment { get; set; }

        public AuthorDto Author { get; set; } = new AuthorDto();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string Content { get; set; } = string.Empty;

        public int LikesCount { get; set; }

        public List<string> LikedByUsers { get; set; } = new List<string>();

        public List<CommunityComment> Replies { get; set; } = new List<CommunityComment>();
    }
}
