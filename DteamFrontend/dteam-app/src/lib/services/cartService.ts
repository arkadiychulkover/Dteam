import { api } from './api';
import type { CartItem, CartSummary } from '../types';

export const cartService = {
  async getCart(): Promise<CartSummary> {
    return await api.get<CartSummary>('/cart');
  },

  async addToCart(gameId: string): Promise<CartItem> {
    return await api.post<CartItem>('/cart', { gameId });
  },

  async removeFromCart(gameId: string): Promise<{ message: string; gameId: string }> {
    return await api.delete<{ message: string; gameId: string }>(`/cart/${gameId}`);
  },

  async clearCart(): Promise<{ message: string; count: number }> {
    return await api.delete<{ message: string; count: number }>('/cart');
  },

  async moveToWishlist(gameId: string): Promise<{ message: string; gameId: string }> {
    return await api.post<{ message: string; gameId: string }>(`/cart/move-to-wishlist/${gameId}`, {});
  },
};
