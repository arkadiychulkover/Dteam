import { api } from './api';
import type { WishlistItem } from '../types';

export const wishlistService = {
  /**
   * Отримати список бажаних ігор поточного користувача
   */
  async getWishlist(): Promise<WishlistItem[]> {
    try {
      const items = await api.get<WishlistItem[]>('/wishlist');
      return Array.isArray(items) ? items : [];
    } catch (err) {
      console.warn('[wishlistService] Failed to load wishlist:', err);
      return [];
    }
  },

  /**
   * Додати гру до списку бажаного
   */
  async addToWishlist(gameId: string): Promise<WishlistItem> {
    return await api.post<WishlistItem>('/wishlist', { gameId });
  },

  /**
   * Видалити гру зі списку бажаного
   */
  async removeFromWishlist(gameId: string): Promise<any> {
    return await api.delete(`/wishlist/${gameId}`);
  },

  /**
   * Перевірити чи гра у списку бажаного
   */
  async isWishlisted(gameId: string): Promise<boolean> {
    try {
      const wishlist = await this.getWishlist();
      return wishlist.some((item) => item.gameId === gameId || item.game?.id === gameId);
    } catch {
      return false;
    }
  },
};
