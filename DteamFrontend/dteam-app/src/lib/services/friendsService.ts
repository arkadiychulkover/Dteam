import { api } from './api';
import type { 
  FriendDto, 
  FriendRequestDto, 
  SendFriendRequestDto, 
  FriendActionResponseDto 
} from '../types/friend';

export const friendsService = {
  async getFriends(): Promise<FriendDto[]> {
    return await api.get<FriendDto[]>('/friends/list');
  },

  async getFriendRequests(type: 'incoming' | 'outgoing' | 'all' = 'incoming'): Promise<FriendRequestDto[]> {
    return await api.get<FriendRequestDto[]>(`/friends/requests?type=${type}`);
  },

  async sendFriendRequest(dto: SendFriendRequestDto): Promise<FriendRequestDto> {
    return await api.post<FriendRequestDto>('/friends/requests', dto);
  },

  async acceptFriendRequest(requestId: string): Promise<FriendActionResponseDto> {
    return await api.post<FriendActionResponseDto>(`/friends/requests/${requestId}/accept`, {});
  },

  async rejectFriendRequest(requestId: string): Promise<FriendActionResponseDto> {
    return await api.post<FriendActionResponseDto>(`/friends/requests/${requestId}/reject`, {});
  },

  async cancelFriendRequest(requestId: string): Promise<FriendActionResponseDto> {
    return await api.post<FriendActionResponseDto>(`/friends/requests/${requestId}/cancel`, {});
  },

  async removeFriend(friendId: string): Promise<FriendActionResponseDto> {
    return await api.delete<FriendActionResponseDto>(`/friends/${friendId}`);
  },

  async blockUser(friendId: string): Promise<FriendActionResponseDto> {
    return await api.post<FriendActionResponseDto>(`/friends/${friendId}/block`, {});
  },

  async unblockUser(friendId: string): Promise<FriendActionResponseDto> {
    return await api.post<FriendActionResponseDto>(`/friends/${friendId}/unblock`, {});
  },

  async getBlocked(): Promise<FriendDto[]> {
    return await api.get<FriendDto[]>('/friends/blocked');
  }
};
