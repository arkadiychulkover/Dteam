import { api } from './api';
import type { UserGame, UserFriend, UserStatus } from '../types';

export interface UserBanStatus {
  userId: string;
  username: string;
  email: string;
  isBanned: boolean;
  isAdmin: boolean;
}

export interface PublicGameSummary {
  id: string;
  title: string;
  coverImageUrl?: string | null;
  priceInNanoTons: number | string;
  discountPercentage: number;
  isDlc?: boolean;
}

export interface PublicProfile {
  id: string;
  username: string;
  avatarUrl?: string | null;
  bannerUrl?: string | null;
  bio?: string | null;
  status: number;
  isInFamily: boolean;
  isAdmin: boolean;
  createdAt: string;
  friendsCount: number;
  gamesCount: number;
  publishedGames: PublicGameSummary[];
  libraryGames: PublicGameSummary[];
  isOwnProfile: boolean;
  friendshipStatus: 'none' | 'pending' | 'friends';
  isIncomingRequest: boolean;
}

export interface PublicFriend {
  id: string;
  username: string;
  avatarUrl?: string | null;
  status: number;
}

export interface MyReview {
  id: string;
  gameId: string;
  gameTitle: string;
  gameCoverImageUrl?: string | null;
  rating: number;
  content: string;
  isRecommended: boolean;
  playTimeHoursAtReview: number;
  createdAt: string;
  updatedAt?: string | null;
}

export const userService = {
  async checkIsBanned(userId: string): Promise<UserBanStatus> {
    return await api.get<UserBanStatus>(`/users/is-banned?userId=${userId}`);
  },

  async getLibrary(): Promise<UserGame[]> {
    return await api.get<UserGame[]>('/users/library');
  },

  async getFriends(): Promise<UserFriend[]> {
    return await api.get<UserFriend[]>('/users/friends');
  },

  async addFriend(friendUsernameOrEmail: string): Promise<UserFriend> {
    return await api.post<UserFriend>('/users/friends/request', { query: friendUsernameOrEmail });
  },

  async updateStatus(status: UserStatus): Promise<void> {
    await api.put('/users/status', { status });
  },

  async toggleFavorite(gameId: string): Promise<void> {
    await api.post(`/users/library/${gameId}/favorite`);
  },

  async getPublicProfile(userId: string): Promise<PublicProfile> {
    return await api.get<PublicProfile>(`/users/${userId}/profile`);
  },

  async getPublicFriends(userId: string): Promise<PublicFriend[]> {
    return await api.get<PublicFriend[]>(`/users/${userId}/friends`);
  },

  async getMyReviews(): Promise<MyReview[]> {
    return await api.get<MyReview[]>('/users/me/reviews');
  },

  async updateMyProfile(patch: { bio?: string; avatarUrl?: string; bannerUrl?: string }): Promise<{ id: string; username: string; avatarUrl?: string | null; bannerUrl?: string | null; bio?: string | null }> {
    return await api.put('/users/me', patch);
  },
};

