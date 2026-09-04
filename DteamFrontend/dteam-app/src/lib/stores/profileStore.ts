import { writable } from 'svelte/store';
import { userService, type PublicProfile, type PublicFriend } from '../services/userService';
import { uiStore } from './uiStore';

function createProfileStore() {
  const { subscribe, update, set } = writable<{
    viewedUserId: string | null;
    profile: PublicProfile | null;
    friends: PublicFriend[];
    isLoading: boolean;
    error: string | null;
  }>({
    viewedUserId: null,
    profile: null,
    friends: [],
    isLoading: false,
    error: null,
  });

  return {
    subscribe,

    viewProfile: (userId: string) => {
      set({ viewedUserId: userId, profile: null, friends: [], isLoading: true, error: null });
      uiStore.setTab('profile');
      loadProfile(userId);
    },

    reload: () => {
      update((s) => {
        if (s.viewedUserId) loadProfile(s.viewedUserId);
        return s;
      });
    },

    clear: () => set({ viewedUserId: null, profile: null, friends: [], isLoading: false, error: null }),
  };

  async function loadProfile(userId: string) {
    try {
      const [profile, friends] = await Promise.all([
        userService.getPublicProfile(userId),
        userService.getPublicFriends(userId).catch(() => []),
      ]);
      const uniqueFriends = Array.from(new Map((friends || []).map((f) => [f.id, f])).values());
      const updatedProfile = profile ? { ...profile, friendsCount: uniqueFriends.length } : null;
      update((s) =>
        s.viewedUserId === userId
          ? { ...s, profile: updatedProfile, friends: uniqueFriends, isLoading: false, error: null }
          : s
      );
    } catch (err: any) {
      update((s) =>
        s.viewedUserId === userId
          ? { ...s, isLoading: false, error: err?.message || 'Не вдалося завантажити профіль.' }
          : s
      );
    }
  }
}

export const profileStore = createProfileStore();

