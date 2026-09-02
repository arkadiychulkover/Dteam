import { api } from './api';
import type { Game, Review, GameFilterParams, GameRecommendation  } from '../types';

export interface UploadImageResponse {
  url: string;
  fileName: string;
  size: number;
  message: string;
}

export const gamesService = {
  async getGames(filters?: GameFilterParams): Promise<Game[]> {
    const params = new URLSearchParams();
    if (filters) {
      if (filters.search) params.append('search', filters.search);
      if (filters.genre && filters.genre !== 'All Games' && filters.genre !== 'Все') {
        params.append('genre', filters.genre);
      }
      if (filters.isDlc !== undefined) params.append('isDlc', String(filters.isDlc));
      if (filters.isDiscounted) params.append('isDiscounted', 'true');
      if (filters.isFree) params.append('isFree', 'true');
      if (filters.minPrice !== undefined) params.append('minPrice', String(filters.minPrice));
      if (filters.maxPrice !== undefined) params.append('maxPrice', String(filters.maxPrice));
      if (filters.platform) params.append('platform', filters.platform);
      if (filters.feature) params.append('feature', filters.feature);
      if (filters.tag) params.append('tag', filters.tag);
      if (filters.sortBy) params.append('sortBy', filters.sortBy);
    }

    const query = params.toString() ? `?${params.toString()}` : '';
    return await api.get<Game[]>(`/games${query}`);
  },

  async getGameById(id: string): Promise<Game> {
    return await api.get<Game>(`/games/${id}`);
  },

  async getGenres(): Promise<string[]> {
    return await api.get<string[]>('/games/genres');
  },

  async uploadImage(file: File): Promise<UploadImageResponse> {
    const formData = new FormData();
    formData.append('file', file);
    return await api.post<UploadImageResponse>('/games/upload-image', formData);
  },

  async getReviews(gameId: string): Promise<Review[]> {
    return await api.get<Review[]>(`/games/${gameId}/reviews`);
  },

  async getDlcs(gameId: string): Promise<Game[]> {
    return await api.get<Game[]>(`/games/${gameId}/dlcs`);
  },

  async getReviewDetails(gameId: string, reviewId: string): Promise<Review> {
    return await api.get<Review>(`/games/${gameId}/reviews/${reviewId}`);
  },

  async addReview(gameId: string, payload: { rating?: number; content: string; isRecommended?: boolean; parentReviewId?: string }): Promise<Review> {
    return await api.post<Review>(`/games/${gameId}/reviews`, {
      rating: payload.rating ?? 5,
      content: payload.content,
      isRecommended: payload.isRecommended ?? true,
      parentReviewId: payload.parentReviewId
    });
  },

  async addReviewReply(gameId: string, parentReviewId: string, content: string): Promise<Review> {
    return await api.post<Review>(`/games/${gameId}/reviews`, {
      content,
      parentReviewId,
      rating: 5,
      isRecommended: true
    });
  },

  async addReviewComment(gameId: string, reviewId: string, content: string): Promise<Review> {
    return await api.post<Review>(`/games/${gameId}/reviews/${reviewId}/comments`, { content });
  },

  async toggleReviewLike(gameId: string, reviewId: string): Promise<{ liked: boolean; likesCount: number }> {
    return await api.post<{ liked: boolean; likesCount: number }>(`/games/${gameId}/reviews/${reviewId}/like`);
  },

  async buyGame(gameId: string): Promise<{ success: boolean; txHash?: string }> {
    return await api.post(`/games/${gameId}/buy`);
  },

  async getRecommendations(query = '', limit = 10): Promise<GameRecommendation[]> {
    const params = new URLSearchParams();
    if (query) params.append('query', query);
    if (limit) params.append('limit', String(limit));
    const qs = params.toString() ? `?${params.toString()}` : '';
    return await api.get<GameRecommendation[]>(`/recommendation${qs}`);
  },
};
