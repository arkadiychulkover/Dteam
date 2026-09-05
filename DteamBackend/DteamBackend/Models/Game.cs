using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DteamBackend.Models
{
    public class Game
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required(ErrorMessage = "Title is required")]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? ShortDescription { get; set; }

        public long PriceInNanoTons { get; set; } = 0;

        [Range(0, 100)]
        public int DiscountPercentage { get; set; } = 0;

        [Required]
        [MaxLength(500)]
        public string ServerArchivePath { get; set; } = string.Empty;

        public Guid OwnerId { get; set; }
        public Duser Owner { get; set; } = null!;

        public long DownloadCount { get; set; } = 0;

        public double AverageRating { get; set; } = 0.0;

        public int ReviewsCount { get; set; } = 0;

        public bool IsDlc { get; set; } = false;

        public Guid? ParentGameId { get; set; }
        public Game? ParentGame { get; set; }

        public ICollection<Game> Dlcs { get; set; } = new List<Game>();

        public List<string> Genres { get; set; } = new();

        public List<string> Platforms { get; set; } = new() { "Windows" };

        public List<string> Features { get; set; } = new();

        public List<GameLanguageSupport> SupportedLanguages { get; set; } = new();

        public List<string> Tags { get; set; } = new();

        [MaxLength(50)]
        public string Version { get; set; } = "1.0.0";

        public long SizeInBytes { get; set; } = 0;

        public bool IsPublished { get; set; } = true;

        [MaxLength(500)]
        public string? HeaderImageUrl { get; set; }

        [MaxLength(500)]
        public string? CoverImageUrl { get; set; }

        public List<string> ScreenshotUrls { get; set; } = new();

        [MaxLength(500)]
        public string? TrailerUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public ICollection<Review> Reviews { get; set; } = new List<Review>();

        [JsonIgnore]
        public ICollection<CommunityPost> CommunityPosts { get; set; } = new List<CommunityPost>();

        [NotMapped]
        public ICollection<GameNews> News => CommunityPosts
            .Where(p => p.Category == "news")
            .Select(GameNews.FromCommunityPost)
            .ToList();

        public ICollection<UserGame> Owners { get; set; } = new List<UserGame>();

        public ICollection<UserWishlist> WishlistedBy { get; set; } = new List<UserWishlist>();

        public ICollection<UserCartItem> InCartsOf { get; set; } = new List<UserCartItem>();

        public string TasteVectorJson { get; set; } = JsonSerializer.Serialize(TasteCategories.Empty());

        [NotMapped]
        public float[] TasteVector
        {
            get => JsonSerializer.Deserialize<float[]>(TasteVectorJson) ?? TasteCategories.Empty();
            set => TasteVectorJson = JsonSerializer.Serialize(value);
        }

        public void RecalculateTasteVector()
        {
            TasteVector = TasteCategories.BuildGameVector(
                Genres,
                Tags,
                Features);
        }
    }
}
