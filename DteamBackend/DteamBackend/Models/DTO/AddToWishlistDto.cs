using System.ComponentModel.DataAnnotations;

namespace DteamBackend.Models.DTO
{
    public class AddToWishlistDto
    {
        [Required(ErrorMessage = "GameId is required")]
        public Guid GameId { get; set; }

        [Range(1, 10, ErrorMessage = "Priority must be between 1 and 10")]
        public int? Priority { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}
