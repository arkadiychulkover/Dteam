using System.ComponentModel.DataAnnotations;

namespace DteamBackend.Models.DTO
{
    public class CreateGameDto
    {
        [Required(ErrorMessage = "Title is required")]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? ShortDescription { get; set; }

        public long PriceInNanoTons { get; set; } = 0;

        [Range(0, 100)]
        public int DiscountPercentage { get; set; } = 0;

        [Required(ErrorMessage = "Server archive path is required")]
        [MaxLength(500)]
        public string ServerArchivePath { get; set; } = string.Empty;

        public Guid? OwnerId { get; set; }

        public bool IsDlc { get; set; } = false;

        public Guid? ParentGameId { get; set; }

        public List<string> Genres { get; set; } = new();

        public List<string> Platforms { get; set; } = new() { "Windows" };

        public List<string> Features { get; set; } = new();

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
    }
}
