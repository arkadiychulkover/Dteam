import { api } from './api';
import type { Game, Review } from '../types';

export const gamesService = {
  async getGames(category?: string, search?: string): Promise<Game[]> {
    const params = new URLSearchParams();
    if (category && category !== 'All Games') params.append('category', category);
    if (search) params.append('search', search);

    const query = params.toString() ? `?${params.toString()}` : '';
    return await api.get<Game[]>(`/games${query}`);
  },

  async getGameById(id: string): Promise<Game> {
    return await api.get<Game>(`/games/${id}`);
  },

  async createGame(formData: FormData): Promise<Game> {
    return await api.post<Game>('/games', formData);
  },

  async addReview(gameId: string, payload: { rating: number; content: string; isRecommended: boolean }): Promise<Review> {
    return await api.post<Review>(`/games/${gameId}/reviews`, payload);
  },

  async buyGame(gameId: string): Promise<{ success: boolean; txHash?: string }> {
    return await api.post(`/games/${gameId}/buy`);
  },
};
