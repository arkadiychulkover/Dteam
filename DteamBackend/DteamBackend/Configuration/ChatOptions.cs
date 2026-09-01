namespace DteamBackend.Configuration
{
    public class ChatOptions
    {
        public const string SectionName = "Chat";

        public long MaxImageSizeBytes { get; set; } = 25 * 1024 * 1024; // 25 MB
        public long MaxFileSizeBytes { get; set; } = 200 * 1024 * 1024; // 200 MB
        public long MaxVoiceSizeBytes { get; set; } = 15 * 1024 * 1024; // 15 MB
        public int MaxVoiceDurationSeconds { get; set; } = 300; // 5 min

        public string StoragePath { get; set; } = "App_Data/ChatStorage";

        public string[] AllowedImageExtensions { get; set; } = [".jpg", ".jpeg", ".png", ".webp", ".gif", ".bmp"];
        public string[] AllowedVoiceExtensions { get; set; } = [".webm", ".ogg", ".mp3", ".wav", ".m4a"];
        public string[] AllowedFileExtensions { get; set; } = [
            ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".txt", ".zip", ".rar", ".7z", ".tar", ".gz",
            ".jpg", ".jpeg", ".png", ".webp", ".gif", ".bmp", ".webm", ".ogg", ".mp3", ".wav", ".json", ".csv"
        ];
    }
}
