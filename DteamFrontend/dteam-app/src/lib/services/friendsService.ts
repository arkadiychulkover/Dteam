import { api } from './api';
import type { UserFriend } from '../types';

export const friendsService = {
  getFriends: async (): Promise<UserFriend[]> => {
    return await api.get<UserFriend[]>('/api/friends');
  },

  sendFriendRequest: async (usernameOrEmail: string): Promise<any> => {
    return await api.post('/api/friends/request', { usernameOrEmail });
  },

  acceptFriendRequest: async (friendId: string): Promise<any> => {
    return await api.post(`/api/friends/accept/${friendId}`);
  },

  declineOrRemoveFriend: async (friendId: string): Promise<any> => {
    return await api.post(`/api/friends/decline/${friendId}`);
  },
};
