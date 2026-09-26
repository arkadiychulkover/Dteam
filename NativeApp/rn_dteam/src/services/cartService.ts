import { api } from './api';
import type { CartItem, CartSummary, CheckoutResult } from '../types';

export const cartService = {
  /**
   * Отримати вміст поточного кошика користувача
   */
  async getCart(): Promise<CartSummary> {
    return await api.get<CartSummary>('/cart');
  },

  /**
   * Додати гру до кошика на бекенді
   */
  async addToCart(gameId: string): Promise<CartItem> {
    return await api.post<CartItem>('/cart', { gameId });
  },

  /**
   * Видалити гру з кошика
   */
  async removeFromCart(gameId: string): Promise<{ message: string; gameId: string }> {
    return await api.delete<{ message: string; gameId: string }>(`/cart/${gameId}`);
  },

  /**
   * Очистити весь кошик
   */
  async clearCart(): Promise<{ message: string; count: number }> {
    return await api.delete<{ message: string; count: number }>('/cart');
  },

  /**
   * Перемістити гру з кошика до списку бажаного
   */
  async moveToWishlist(gameId: string): Promise<{ message: string; gameId: string }> {
    return await api.post<{ message: string; gameId: string }>(`/cart/move-to-wishlist/${gameId}`, {});
  },

  /**
   * Оформити покупку та оплатити кошик з балансу користувача
   */
  async checkout(): Promise<CheckoutResult> {
    return await api.post<CheckoutResult>('/cart/checkout', {});
  },
};
