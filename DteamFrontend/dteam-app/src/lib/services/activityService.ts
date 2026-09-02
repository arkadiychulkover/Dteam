import { api } from './api';
import type { UserActivity } from '../types/activity';

export const activityService = {
  async getFriendsActivities(limit = 50, offset = 0): Promise<UserActivity[]> {
    try {
      return await api.get<UserActivity[]>(`/activity/friends?limit=${limit}&offset=${offset}`);
    } catch (e) {
      console.warn('[activityService] Failed to load friends activities:', e);
      return [];
    }
  },

  async getUserActivities(userId: string, limit = 50, offset = 0): Promise<UserActivity[]> {
    try {
      return await api.get<UserActivity[]>(`/activity/user/${userId}?limit=${limit}&offset=${offset}`);
    } catch (e) {
      console.warn(`[activityService] Failed to load activities for user ${userId}:`, e);
      return [];
    }
  },

  async getGlobalFeed(limit = 50, offset = 0): Promise<UserActivity[]> {
    try {
      return await api.get<UserActivity[]>(`/activity/global?limit=${limit}&offset=${offset}`);
    } catch (e) {
      console.warn('[activityService] Failed to load global activities:', e);
      return [];
    }
  }
};
