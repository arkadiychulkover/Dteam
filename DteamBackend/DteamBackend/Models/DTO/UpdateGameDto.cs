using System.ComponentModel.DataAnnotations;

namespace DteamBackend.Models.DTO
{
    public class UpdateGameDto
    {
        [MaxLength(200)]
        public string? Title { get; set; }

        public string? Description { get; set; }

        [MaxLength(500)]
        public string? ShortDescription { get; set; }

        [Range(0, long.MaxValue, ErrorMessage = "Price must be non-negative")]
        public long? PriceInNanoTons { get; set; }

        [Range(0, 100)]
        public int? DiscountPercentage { get; set; }

        [MaxLength(500)]
        public string? ServerArchivePath { get; set; }

        public Guid? OwnerId { get; set; }

        public bool? IsDlc { get; set; }

        public Guid? ParentGameId { get; set; }

        public List<string>? Genres { get; set; }

        public List<string>? Platforms { get; set; }

        public List<string>? Features { get; set; }

        public List<GameLanguageSupport>? SupportedLanguages { get; set; }

        public List<string>? Tags { get; set; }

        [MaxLength(50)]
        public string? Version { get; set; }

        public long? SizeInBytes { get; set; }

        public bool? IsPublished { get; set; }

        [MaxLength(500)]
        public string? HeaderImageUrl { get; set; }

        [MaxLength(500)]
        public string? CoverImageUrl { get; set; }

        public List<string>? ScreenshotUrls { get; set; }

        [MaxLength(500)]
        public string? TrailerUrl { get; set; }
    }
}
