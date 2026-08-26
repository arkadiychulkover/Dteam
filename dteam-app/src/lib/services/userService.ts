import { api } from './api';
import type { UserGame, UserFriend, UserStatus } from '../types';

export const userService = {
  async getLibrary(): Promise<UserGame[]> {
    return await api.get<UserGame[]>('/users/library');
  },

  async getFriends(): Promise<UserFriend[]> {
    return await api.get<UserFriend[]>('/users/friends');
  },

  async addFriend(friendUsernameOrEmail: string): Promise<UserFriend> {
    return await api.post<UserFriend>('/users/friends/request', { query: friendUsernameOrEmail });
  },

  async updateStatus(status: UserStatus): Promise<void> {
    await api.put('/users/status', { status });
  },

  async toggleFavorite(gameId: string): Promise<void> {
    await api.post(`/users/library/${gameId}/favorite`);
  },
};
