import { api } from './api';
import type { WishlistItem, AddToWishlistPayload, UpdateWishlistItemPayload } from '../types';

export const wishlistService = {
  async getWishlist(userId: string, search?: string, sortBy?: string): Promise<WishlistItem[]> {
    const params = new URLSearchParams({ userId });
    if (search) params.append('search', search);
    if (sortBy) params.append('sortBy', sortBy);
    return await api.get<WishlistItem[]>(`/wishlist?${params.toString()}`);
  },

  async checkInWishlist(gameId: string, userId: string): Promise<{ isInWishlist: boolean; item: WishlistItem | null }> {
    return await api.get<{ isInWishlist: boolean; item: WishlistItem | null }>(`/wishlist/${gameId}?userId=${userId}`);
  },

  async addToWishlist(userId: string, payload: AddToWishlistPayload): Promise<WishlistItem> {
    return await api.post<WishlistItem>(`/wishlist?userId=${userId}`, payload);
  },

  async updateWishlistItem(gameId: string, userId: string, payload: UpdateWishlistItemPayload): Promise<WishlistItem> {
    return await api.put<WishlistItem>(`/wishlist/${gameId}?userId=${userId}`, payload);
  },

  async removeFromWishlist(gameId: string, userId: string): Promise<{ message: string; gameId: string }> {
    return await api.delete<{ message: string; gameId: string }>(`/wishlist/${gameId}?userId=${userId}`);
  },

  async clearWishlist(userId: string): Promise<{ message: string; count: number }> {
    return await api.delete<{ message: string; count: number }>(`/wishlist?userId=${userId}`);
  },
};
