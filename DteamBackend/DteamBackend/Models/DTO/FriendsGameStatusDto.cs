namespace DteamBackend.Models.DTO
{
    public class FriendsGameStatusDto
    {
        public List<FriendDto> FriendsWhoOwn { get; set; } = new();
        public List<FriendDto> FriendsWhoWishlist { get; set; } = new();
    }
}
