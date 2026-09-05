using System.ComponentModel.DataAnnotations;

namespace DteamBackend.Models.DTO
{
    public class UpdateWishlistItemDto
    {
        public int? Priority { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}

