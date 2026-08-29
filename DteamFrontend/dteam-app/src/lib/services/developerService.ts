import { api } from './api';
import type { Game } from '../types';
import type { DeveloperStats, CreateDeveloperGameRequest, UpdateDeveloperGameRequest } from '../types/developer';

export const developerService = {
  async getStats(): Promise<DeveloperStats> {
    return await api.get<DeveloperStats>('/developer/stats');
  },

  async getMyGames(): Promise<Game[]> {
    return await api.get<Game[]>('/developer/games');
  },

  async getMyGame(id: string): Promise<Game> {
    return await api.get<Game>(`/developer/games/${id}`);
  },

  async createGame(data: CreateDeveloperGameRequest): Promise<Game> {
    return await api.post<Game>('/developer/games', data);
  },

  async updateGame(id: string, data: UpdateDeveloperGameRequest): Promise<Game> {
    return await api.put<Game>(`/developer/games/${id}`, data);
  },

  async togglePublishStatus(id: string): Promise<Game> {
    return await api.request<Game>(`/developer/games/${id}/status`, { method: 'PATCH' });
  },

  async deleteGame(id: string): Promise<{ message: string; gameId: string }> {
    return await api.delete<{ message: string; gameId: string }>(`/developer/games/${id}`);
  },
};
