using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DteamBackend.Models
{
    [NotMapped]
    public class GameNews
    {
        public string Id { get; set; } = string.Empty;

        public string GameId { get; set; } = string.Empty;

        public string? GameTitle { get; set; }

        public string? GameBannerUrl { get; set; }

        public Guid? GameGuidId => Guid.TryParse(GameId, out var g) ? g : null;

        [JsonIgnore]
        public Game? Game { get; set; }

        public AuthorDto Author { get; set; } = new AuthorDto();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string Category { get; set; } = "news";

        public string Title { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public PostMedia Media { get; set; } = new PostMedia();

        public List<string> LikedByUsers { get; set; } = new List<string>();

        public CommunityPost ToCommunityPost()
        {
            return new CommunityPost
            {
                Id = this.Id,
                GameId = this.GameId,
                GameTitle = this.GameTitle,
                GameBannerUrl = this.GameBannerUrl,
                Game = this.Game,
                Author = this.Author,
                CreatedAt = this.CreatedAt,
                Category = "news",
                Title = this.Title,
                Content = this.Content,
                Media = this.Media,
                LikedByUsers = this.LikedByUsers
            };
        }

        public static GameNews FromCommunityPost(CommunityPost post)
        {
            return new GameNews
            {
                Id = post.Id,
                GameId = post.GameId,
                GameTitle = post.GameTitle,
                GameBannerUrl = post.GameBannerUrl,
                Game = post.Game,
                Author = post.Author,
                CreatedAt = post.CreatedAt,
                Category = "news",
                Title = post.Title,
                Content = post.Content,
                Media = post.Media,
                LikedByUsers = post.LikedByUsers
            };
        }
    }
}
