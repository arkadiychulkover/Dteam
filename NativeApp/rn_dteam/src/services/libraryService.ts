import { api } from './api';
import type { UserGame } from '../types';

export const libraryService = {
  async getUserLibrary(): Promise<UserGame[]> {
    return api.get<UserGame[]>('/users/library');
  },

  async toggleFavorite(gameId: string): Promise<{ isFavorite: boolean }> {
    return api.post<{ isFavorite: boolean }>(`/users/library/${gameId}/favorite`);
  },
};
