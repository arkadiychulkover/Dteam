namespace DteamBackend.Configuration
{
    public class R2Options
    {
        public const string SectionName = "R2";

        public string BucketName { get; set; } = "dteam";
        public string ServiceUrl { get; set; } = "https://fdca1ad0343f1cbce535687fab3674e2.r2.cloudflarestorage.com";
        public string AccountId { get; set; } = "fdca1ad0343f1cbce535687fab3674e2";
        public string AccessKey { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
        public string? PublicUrl { get; set; }
    }
}
