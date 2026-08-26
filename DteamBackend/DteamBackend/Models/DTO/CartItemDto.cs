using System.ComponentModel.DataAnnotations;

namespace DteamBackend.Models.DTO
{
    public class CartItemDto
    {
        public Guid UserId { get; set; }
        public Guid GameId { get; set; }
        public GameDto Game { get; set; } = null!;
        public DateTime AddedAt { get; set; }
    }

    public class AddToCartDto
    {
        [Required(ErrorMessage = "GameId is required")]
        public Guid GameId { get; set; }
    }

    public class CartSummaryDto
    {
        public List<CartItemDto> Items { get; set; } = new();
        public int TotalCount => Items.Count;
        public long TotalBasePriceInNanoTons => Items.Sum(i => i.Game.PriceInNanoTons);
        public long TotalEffectivePriceInNanoTons => Items.Sum(i => 
            i.Game.DiscountPercentage > 0 
                ? (long)Math.Round((double)i.Game.PriceInNanoTons * (100 - i.Game.DiscountPercentage) / 100.0) 
                : i.Game.PriceInNanoTons);
        public long TotalSavingsInNanoTons => TotalBasePriceInNanoTons - TotalEffectivePriceInNanoTons;
    }
}
