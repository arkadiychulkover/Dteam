using System.ComponentModel.DataAnnotations;

namespace DteamBackend.Models.DTO
{
    public class CreateGameDto : IValidatableObject
    {
        [Required(ErrorMessage = "Title is required")]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? ShortDescription { get; set; }

        [Range(0, long.MaxValue, ErrorMessage = "Price must be non-negative")]
        public long PriceInNanoTons { get; set; } = 0;

        [Range(0, 100)]
        public int DiscountPercentage { get; set; } = 0;

        [MaxLength(500)]
        public string? ServerArchivePath { get; set; }

        public Guid? OwnerId { get; set; }

        public bool IsDlc { get; set; } = false;

        public Guid? ParentGameId { get; set; }

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

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (IsPublished && string.IsNullOrWhiteSpace(ServerArchivePath))
            {
                yield return new ValidationResult(
                    "Для публікації гри в каталозі необхідно завантажити файл білду гри (.zip). Без файлу білду проект можна зберегти лише як чернетку.",
                    new[] { nameof(ServerArchivePath) });
            }
        }
    }
}

