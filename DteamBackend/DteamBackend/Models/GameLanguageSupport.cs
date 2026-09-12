namespace DteamBackend.Models
{
    public class GameLanguageSupport
    {
        public string Language { get; set; } = string.Empty;
        public bool Interface { get; set; } = true;
        public bool FullAudio { get; set; } = false;
        public bool Subtitles { get; set; } = true;
    }
}
