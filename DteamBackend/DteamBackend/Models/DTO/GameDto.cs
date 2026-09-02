namespace DteamBackend.Models.DTO
{
    public class GameDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? ShortDescription { get; set; }
        public long PriceInNanoTons { get; set; }
        public int DiscountPercentage { get; set; }
        public long OriginalPriceInNanoTons => DiscountPercentage > 0 
            ? (long)Math.Round(PriceInNanoTons / (1.0 - (DiscountPercentage / 100.0))) 
            : PriceInNanoTons;
        public string ServerArchivePath { get; set; } = string.Empty;
        public Guid OwnerId { get; set; }
        public string? OwnerUsername { get; set; }
        public long DownloadCount { get; set; }
        public double AverageRating { get; set; }
        public int ReviewsCount { get; set; }
        public bool IsDlc { get; set; }
        public Guid? ParentGameId { get; set; }
        public string? ParentGameTitle { get; set; }
        public List<GameDto> Dlcs { get; set; } = new();
        public List<string> Genres { get; set; } = new();
        public List<string> Platforms { get; set; } = new();
        public List<string> Features { get; set; } = new();
        public List<GameLanguageSupport> SupportedLanguages { get; set; } = new();
        public List<string> Tags { get; set; } = new();
        public string Version { get; set; } = "1.0.0";
        public long SizeInBytes { get; set; }
        public bool IsPublished { get; set; }
        public string? HeaderImageUrl { get; set; }
        public string? CoverImageUrl { get; set; }
        public List<string> ScreenshotUrls { get; set; } = new();
        public string? TrailerUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
