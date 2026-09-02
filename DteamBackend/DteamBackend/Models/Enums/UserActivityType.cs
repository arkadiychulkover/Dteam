namespace DteamBackend.Models.Enums
{
    public enum UserActivityType
    {
        GamePurchased = 1,       // Покупка гри
        FriendAdded = 2,         // Новий друг
        BalanceDeposited = 3,    // Поповнення балансу (TON)
        ReviewPosted = 4,        // Написано рецензію
        GamePublished = 5,       // Опубліковано нову гру
        AchievementUnlocked = 6, // Отримано досягнення
        WishlistAdded = 7        // Додано у список бажань
    }
}
