using System.ComponentModel.DataAnnotations;
using DteamBackend.Validation;

namespace DteamBackend.Models.DTO.Library
{
    public class GameCollectionItemDto
    {
        public Guid GameId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? CoverImageUrl { get; set; }
        public DateTime AddedAt { get; set; }
    }

    public class GameCollectionDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int GamesCount { get; set; }
        public List<GameDto> Games { get; set; } = new();
        public List<GameCollectionItemDto> Items { get; set; } = new();
    }

    public class CreateGameCollectionDto
    {
        [RequiredNonWhiteSpace(ErrorMessage = "Назва колекції обов'язкова")]
        [MaxLength(100, ErrorMessage = "Назва колекції не може перевищувати 100 символів")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "Опис не може перевищувати 500 символів")]
        public string? Description { get; set; }

        public List<Guid>? InitialGameIds { get; set; }
    }

    public class UpdateGameCollectionDto
    {
        [RequiredNonWhiteSpace(ErrorMessage = "Назва колекції обов'язкова")]
        [MaxLength(100, ErrorMessage = "Назва колекції не може перевищувати 100 символів")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "Опис не може перевищувати 500 символів")]
        public string? Description { get; set; }
    }

    public class AddGameToCollectionDto
    {
        [Required(ErrorMessage = "ID гри обов'язковий")]
        public Guid GameId { get; set; }
    }
}
