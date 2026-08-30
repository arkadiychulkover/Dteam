import { api } from './api';
import type { Game } from '../types';
import type { DeveloperStats, CreateDeveloperGameRequest, UpdateDeveloperGameRequest } from '../types/developer';
import type { CommunityPost } from './communityService';

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

  async getGameNews(gameId: string): Promise<CommunityPost[]> {
    return await api.get<CommunityPost[]>(`/developer/games/${gameId}/news`);
  },

  async getMyNews(): Promise<CommunityPost[]> {
    return await api.get<CommunityPost[]>('/developer/news');
  },

  async createGameNews(
    gameId: string,
    payload: {
      title: string;
      content: string;
      mediaUrl?: string;
      mediaThumbnailUrl?: string;
      mediaType?: 'image' | 'video' | 'none';
      file?: File;
    }
  ): Promise<CommunityPost> {
    const formData = new FormData();
    formData.append('title', payload.title);
    formData.append('content', payload.content);
    if (payload.mediaUrl) formData.append('mediaUrl', payload.mediaUrl);
    if (payload.mediaThumbnailUrl) formData.append('mediaThumbnailUrl', payload.mediaThumbnailUrl);
    if (payload.mediaType) formData.append('mediaType', payload.mediaType);
    if (payload.file) formData.append('file', payload.file);
    return await api.post<CommunityPost>(`/developer/games/${gameId}/news`, formData);
  },

  async deleteGameNews(newsId: string): Promise<{ message: string; newsId: string }> {
    return await api.delete<{ message: string; newsId: string }>(`/developer/news/${newsId}`);
  },
};
