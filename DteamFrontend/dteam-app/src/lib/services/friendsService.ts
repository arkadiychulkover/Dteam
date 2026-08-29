import { api } from './api';
import type { UserFriend } from '../types';

export const friendsService = {
  getFriends: async (): Promise<UserFriend[]> => {
    return await api.get<UserFriend[]>('/friends');
  },

  sendFriendRequest: async (usernameOrEmail: string): Promise<any> => {
    return await api.post('/friends/request', { usernameOrEmail });
  },

  acceptFriendRequest: async (friendId: string): Promise<any> => {
    return await api.post(`/friends/accept/${friendId}`);
  },

  declineOrRemoveFriend: async (friendId: string): Promise<any> => {
    return await api.post(`/friends/decline/${friendId}`);
  },
};
