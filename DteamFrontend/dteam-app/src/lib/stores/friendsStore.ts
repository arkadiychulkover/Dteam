import { writable, get } from 'svelte/store';
import type { UserFriend } from '../types';
import { friendsService } from '../services/friendsService';
import { currentUser } from './authStore';
import { uiStore } from './uiStore';

function createFriendsStore() {
  const { subscribe, set, update } = writable<{
    friends: UserFriend[];
    isLoading: boolean;
    hasLoaded: boolean;
  }>({
    friends: [],
    isLoading: false,
    hasLoaded: false,
  });

  async function fetchFriends() {
    return await friendsService.getFriends();
  }

  return {
    subscribe,

    loadFriends: async () => {
      const me = get(currentUser);
      if (!me?.id) {
        set({ friends: [], isLoading: false, hasLoaded: true });
        return;
      }
      update((s) => ({ ...s, isLoading: true }));
      try {
        const friends = await fetchFriends();
        update((s) => ({ ...s, friends: friends || [], isLoading: false, hasLoaded: true }));
      } catch (err) {
        console.warn('[friendsStore] Не вдалося завантажити друзів:', err);
        update((s) => ({ ...s, friends: [], isLoading: false, hasLoaded: true }));
      }
    },

    clear: () => set({ friends: [], isLoading: false, hasLoaded: false }),

    addFriend: async (usernameOrEmail: string) => {
      try {
        await friendsService.sendFriendRequest(usernameOrEmail);
        uiStore.addToast({
          title: 'Запит надіслано',
          message: `Запит у друзі надіслано користувачеві ${usernameOrEmail}.`,
          type: 'success',
        });
        try {
          const friends = await fetchFriends();
          update((s) => ({ ...s, friends: friends || [] }));
        } catch {
          /* мовчки ігноруємо — список і так покаже попередній стан */
        }
      } catch (err: any) {
        uiStore.addToast({
          title: 'Помилка',
          message: err?.message || 'Не вдалося надіслати запит у друзі.',
          type: 'error',
        });
      }
    },
  };
}

export const friendsStore = createFriendsStore();
