export enum UserActivityType {
  GamePurchased = 1,
  FriendAdded = 2,
  BalanceDeposited = 3,
  ReviewPosted = 4,
  GamePublished = 5,
  AchievementUnlocked = 6,
  WishlistAdded = 7
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
