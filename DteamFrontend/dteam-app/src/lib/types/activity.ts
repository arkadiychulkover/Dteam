export enum UserActivityType {
  GamePurchased = 1,       // Покупка гри
  FriendAdded = 2,         // Новий друг
  BalanceDeposited = 3,    // Поповнення балансу (TON)
  ReviewPosted = 4,        // Написано рецензію
  GamePublished = 5,       // Опубліковано нову гру
  AchievementUnlocked = 6, // Досягнення
  WishlistAdded = 7        // Додано у список бажань
}

export interface UserActivity {
  id: string;
  userId: string;
  username: string;
  userAvatarUrl?: string | null;
  type: UserActivityType;
  typeName: string;
  title: string;
  description?: string | null;
  details?: string | null;
  relatedEntityId?: string | null;
  imageUrl?: string | null;
  createdAt: string;
}
