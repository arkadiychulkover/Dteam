import { api } from './api';
import type { WishlistItem, AddToWishlistPayload, UpdateWishlistItemPayload } from '../types';

export const wishlistService = {
  async getWishlist(search?: string, sortBy?: string): Promise<WishlistItem[]> {
    const params = new URLSearchParams();
    if (search) params.append('search', search);
    if (sortBy) params.append('sortBy', sortBy);
    const queryString = params.toString() ? `?${params.toString()}` : '';
    return await api.get<WishlistItem[]>(`/wishlist${queryString}`);
  },

  async checkInWishlist(gameId: string): Promise<{ isInWishlist: boolean; item: WishlistItem | null }> {
    return await api.get<{ isInWishlist: boolean; item: WishlistItem | null }>(`/wishlist/${gameId}`);
  },

  async addToWishlist(payload: AddToWishlistPayload): Promise<WishlistItem> {
    return await api.post<WishlistItem>('/wishlist', payload);
  },

  async updateWishlistItem(gameId: string, payload: UpdateWishlistItemPayload): Promise<WishlistItem> {
    return await api.put<WishlistItem>(`/wishlist/${gameId}`, payload);
  },

  async removeFromWishlist(gameId: string): Promise<{ message: string; gameId: string }> {
    return await api.delete<{ message: string; gameId: string }>(`/wishlist/${gameId}`);
  },

  async clearWishlist(): Promise<{ message: string; count: number }> {
    return await api.delete<{ message: string; count: number }>('/wishlist');
  },
};

