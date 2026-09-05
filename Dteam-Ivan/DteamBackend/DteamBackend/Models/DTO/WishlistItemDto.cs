namespace DteamBackend.Models.DTO
{
    public class WishlistItemDto
    {
        public Guid UserId { get; set; }
        public Guid GameId { get; set; }
        public GameDto Game { get; set; } = null!;
        public DateTime AddedAt { get; set; }
        public int Priority { get; set; }
        public string? Notes { get; set; }
    }
}

