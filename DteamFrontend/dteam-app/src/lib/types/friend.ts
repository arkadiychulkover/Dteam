import type { UserStatus } from './user';

export enum FriendRequestStatus {
  Pending = 0,
  Accepted = 1,
  Rejected = 2,
  Cancelled = 3
}

export interface FriendDto {
  id: string;
  username: string;
  email: string;
  avatarUrl?: string | null;
  bio?: string | null;
  status: UserStatus;
  lastLoginAt?: string | null;
  friendsSince: string;
  currentGame?: string | null;
  level?: number;
}

export interface FriendRequestDto {
  id: string;
  senderId: string;
  senderUsername: string;
  senderAvatarUrl?: string | null;
  receiverId: string;
  receiverUsername: string;
  receiverAvatarUrl?: string | null;
  status: FriendRequestStatus;
  createdAt: string;
}

export interface SendFriendRequestDto {
  receiverId?: string;
  receiverUsername?: string;
}

export interface FriendActionResponseDto {
  success: boolean;
  message: string;
}

export interface FriendsGameStatusDto {
  friendsWhoOwn: FriendDto[];
  friendsWhoWishlist: FriendDto[];
}
