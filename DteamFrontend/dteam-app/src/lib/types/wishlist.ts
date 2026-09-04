import type { Game } from './game';

export interface WishlistItem {
  userId: string;
  gameId: string;
  game: Game;
  addedAt: string;
  priority: number;
  notes?: string | null;
}

export interface AddToWishlistPayload {
  gameId: string;
  priority?: number;
  notes?: string;
}

export interface UpdateWishlistItemPayload {
  priority?: number;
  notes?: string;
}

