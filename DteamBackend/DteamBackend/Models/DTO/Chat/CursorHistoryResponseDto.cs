namespace DteamBackend.Models.DTO.Chat
{
    public class CursorHistoryResponseDto
    {
        public List<ChatMessageDto> Items { get; set; } = new();
        public string? NextCursor { get; set; }
        public bool HasMore { get; set; }
        public int TotalCount { get; set; }
    }
}
