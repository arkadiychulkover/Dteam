namespace DteamBackend.Models.DTO.Chat
{
    public class ChatMediaItemDto
    {
        public Guid MessageId { get; set; }
        public Guid SenderId { get; set; }
        public string SenderUsername { get; set; } = string.Empty;
        public string MediaUrl { get; set; } = string.Empty;
        public string OriginalFileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public int? Duration { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }

    public class ChatMonthGroupDto
    {
        public string MonthLabel { get; set; } = string.Empty; // e.g. "Травень 2024"
        public int Year { get; set; }
        public int Month { get; set; }
        public List<ChatMediaItemDto> Items { get; set; } = new();
    }

    public class ChatMediaSummaryDto
    {
        public int TotalPhotos { get; set; }
        public int TotalFiles { get; set; }
        public int TotalVoiceMessages { get; set; }
        public List<ChatMonthGroupDto> PhotosByMonth { get; set; } = new();
        public List<ChatMonthGroupDto> FilesByMonth { get; set; } = new();
        public List<ChatMonthGroupDto> VoiceByMonth { get; set; } = new();
    }
}
