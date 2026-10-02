import { api } from './api';
import type { WishlistItem } from '../types';

export const wishlistService = {
  

  async getWishlist(): Promise<WishlistItem[]> {
    try {
      const items = await api.get<WishlistItem[]>('/wishlist');
      return Array.isArray(items) ? items : [];
    } catch (err) {
      console.warn('[wishlistService] Failed to load wishlist:', err);
      return [];
    }
  },

  

  async addToWishlist(gameId: string): Promise<WishlistItem> {
    return await api.post<WishlistItem>('/wishlist', { gameId });
  },

  

  async removeFromWishlist(gameId: string): Promise<any> { return await api.delete(`/wishlist/${gameId}`); },

  

  async isWishlisted(gameId: string): Promise<boolean> {
    try {
      const wishlist = await this.getWishlist();
      return wishlist.some((item) => item.gameId === gameId || item.game?.id === gameId);
    } catch {
      return false;
    }
  },
};
