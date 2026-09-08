using System.ComponentModel.DataAnnotations;

namespace DteamBackend.Models
{
    public class GameCollection
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid UserId { get; set; }

        public Duser User { get; set; } = null!;

        [Required(ErrorMessage = "Назва колекції обов'язкова")]
        [MaxLength(100, ErrorMessage = "Назва не може перевищувати 100 символів")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "Опис не може перевищувати 500 символів")]
        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<GameCollectionItem> Items { get; set; } = new List<GameCollectionItem>();
    }

    public class GameCollectionItem
    {
        public Guid CollectionId { get; set; }

        public GameCollection Collection { get; set; } = null!;

        public Guid GameId { get; set; }

        public Game Game { get; set; } = null!;

        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    }
}
