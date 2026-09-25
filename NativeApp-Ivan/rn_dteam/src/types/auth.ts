import type { Game } from './game';

export interface UserDto {
  id: string;
  email: string;
  username: string;
  avatarUrl?: string | null;
  bannerUrl?: string | null;
  bio?: string | null;
  walletAddress?: string | null;
  hardhatAddress?: string | null;
  status?: number;
  isAdmin?: boolean;
}

export interface AuthResponseDto {
  accessToken: string;
  refreshToken: string;
  user: UserDto;
}

export interface UserGame {
  userId: string;
  gameId: string;
  purchasedAt: string;
  playTimeMinutes: number;
  lastPlayedAt?: string | null;
  isFavorite: boolean;
  game?: Game;
}
