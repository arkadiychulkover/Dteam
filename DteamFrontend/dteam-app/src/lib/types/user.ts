export enum UserStatus {
  Offline = 0,
  Online = 1,
  InGame = 2,
  Away = 3,
}

export enum FriendshipStatus {
  Pending = 0,
  Accepted = 1,
  Blocked = 2,
}

export interface Duser {
  id: string;
  email: string;
  username: string;
  walletAddress?: string | null;
  hardhatAddress?: string | null;
  balanceInNanoTons: number | string;
  totalEarningsInNanoTons: number | string;
  createdAt: string;
  updatedAt?: string | null;
  lastLoginAt?: string | null;
  isInFamily: boolean;
  familyOwnerId?: string | null;
  isAdmin: boolean;
  isBanned: boolean;
  status: UserStatus;
  avatarUrl?: string | null;
  bannerUrl?: string | null;
  bio?: string | null;
}

export interface UserFriend {
  userId: string;
  user?: Duser;
  friendId: string;
  friend: Duser;
  status: FriendshipStatus;
  createdAt: string;
}

export interface UserGame {
  userId: string;
  gameId: string;
  purchasedAt: string;
  playTimeMinutes: number;
  lastPlayedAt?: string | null;
  isFavorite: boolean;
  game?: import('./game').Game;
}
