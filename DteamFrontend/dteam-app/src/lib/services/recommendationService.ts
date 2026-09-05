import { api } from './api';
import type { Game } from '../types/game';

export type TasteAction = 'ViewGame' | 'AddToCart' | 'AddToWishlist' | 'Purchase';

export const recommendationService = {
  async getRecommended(take = 24, skip = 0): Promise<Game[]> {
    return api.get<Game[]>(`/recommendations?take=${take}&skip=${skip}`);
  },

  track(gameId: string, action: TasteAction): void {
    api.post(`/recommendations/track/${gameId}?action=${action}`).catch(() => {
    });
  },
};