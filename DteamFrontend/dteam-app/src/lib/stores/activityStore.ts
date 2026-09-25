import { writable } from 'svelte/store';
import type { UserActivity } from '../types/activity';
import { activityService } from '../services/activityService';
import { api } from '../services/api';

function createActivityStore() {
  const { subscribe, update, set } = writable<{
    friendsActivities: UserActivity[];
    userActivities: Record<string, UserActivity[]>;
    globalActivities: UserActivity[];
    isLoading: boolean;
    error: string | null;
  }>({
    friendsActivities: [],
    userActivities: {},
    globalActivities: [],
    isLoading: false,
    error: null,
  });

  return {
    subscribe,

    loadFriendsActivities: async (force = false) => {
      if (!api.getToken()) return;

      update((s) => ({ ...s, isLoading: true, error: null }));
      try {
        const list = await activityService.getFriendsActivities(50, 0);
        update((s) => ({ ...s, friendsActivities: list, isLoading: false }));
      } catch (err: any) {
        update((s) => ({ ...s, isLoading: false, error: err?.message || 'Не вдалося завантажити активності друзів' }));
      }
    },

    loadUserActivities: async (userId: string) => {
      if (!userId) return;

      try {
        const list = await activityService.getUserActivities(userId, 50, 0);
        update((s) => ({
          ...s,
          userActivities: {
            ...s.userActivities,
            [userId.toLowerCase()]: list
          }
        }));
      } catch (err: any) {
        console.warn(`[activityStore] Failed to load activities for user ${userId}:`, err);
      }
    },

    loadGlobalActivities: async () => {
      try {
        const list = await activityService.getGlobalFeed(50, 0);
        update((s) => ({ ...s, globalActivities: list }));
      } catch (err: any) {
        console.warn('[activityStore] Failed to load global activities:', err);
      }
    },

    appendActivity: (activity: UserActivity) => {
      update((s) => ({
        ...s,
        friendsActivities: [activity, ...s.friendsActivities.filter((a) => a.id !== activity.id)],
        globalActivities: [activity, ...s.globalActivities.filter((a) => a.id !== activity.id)]
      }));
    }
  };
}

export const activityStore = createActivityStore();
