import type { Game } from './game';

export interface CartItem {
  userId: string;
  gameId: string;
  game: Game;
  addedAt: string;
}

export interface CartSummary {
  items: CartItem[];
  totalCount: number;
  totalBasePriceInNanoTons: number;
  totalEffectivePriceInNanoTons: number;
  totalSavingsInNanoTons: number;
}

export interface CheckoutResult {
  success: boolean;
  message: string;
  newBalanceInNanoTons: number;
  totalSpentInNanoTons: number;
  purchasedGamesCount: number;
}
