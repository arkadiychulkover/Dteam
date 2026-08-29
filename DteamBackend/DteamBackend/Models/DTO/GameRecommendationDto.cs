namespace DteamBackend.Models.DTO
{
    public class GameRecommendationDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Banner { get; set; } = string.Empty;
        public string BannerUrl { get; set; } = string.Empty;
        public string? HeaderImageUrl { get; set; }
        public string? CoverImageUrl { get; set; }
    }
}

