namespace DteamBackend.Models.DTO
{
    public class DailyMetricPointDto
    {
        public string Date { get; set; } = string.Empty;
        public int Downloads { get; set; } = 0;
        public decimal EarningsInTon { get; set; } = 0;
    }

    public class DeveloperStatsDto
    {
        public long TotalEarningsInNanoTons { get; set; } = 0;
        public long TotalDownloads { get; set; } = 0;
        public int TotalGames { get; set; } = 0;
        public double AverageRating { get; set; } = 0.0;
        public int TotalReviews { get; set; } = 0;

        public long EarningsLast30DaysInNanoTons { get; set; } = 0;
        public long EarningsLast7DaysInNanoTons { get; set; } = 0;
        public long EarningsLast24HoursInNanoTons { get; set; } = 0;

        public long DownloadsLast30Days { get; set; } = 0;
        public long DownloadsLast7Days { get; set; } = 0;
        public long DownloadsLast24Hours { get; set; } = 0;

        public List<DailyMetricPointDto> DailyDynamics { get; set; } = new();
    }
}
