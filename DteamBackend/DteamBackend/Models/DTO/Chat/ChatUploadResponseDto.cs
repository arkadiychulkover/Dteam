namespace DteamBackend.Models.DTO.Chat
{
    public class ChatUploadResponseDto
    {
        public Guid UploadId { get; set; }
        public string OriginalFileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public int? Duration { get; set; }
        public string PreviewUrl { get; set; } = string.Empty;
    }
}
