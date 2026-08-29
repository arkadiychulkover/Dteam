import { writable, get } from 'svelte/store';
import type { UserGame, Game } from '../types';
import { gamesStore } from './gamesStore';
import { authStore } from './authStore';
import { uiStore } from './uiStore';
import { userService } from '../services/userService';

function createLibraryStore() {
  const { subscribe, set, update } = writable<{
    items: UserGame[];
    selectedGameId: string | null;
    isLoading: boolean;
    hasLoaded: boolean;
  }>({
    items: [],
    selectedGameId: null,
    isLoading: false,
    hasLoaded: false,
  });

  return {
    subscribe,

    loadLibrary: async () => {
      update((s) => ({ ...s, isLoading: true }));
      try {
        const items = await userService.getLibrary();
        update((s) => {
          const stillExists = s.selectedGameId && items.some((i) => i.gameId === s.selectedGameId);
          return {
            ...s,
            items: items || [],
            selectedGameId: stillExists ? s.selectedGameId : (items[0]?.gameId ?? null),
            isLoading: false,
            hasLoaded: true,
          };
        });
      } catch (err) {
        console.warn('[libraryStore] Не вдалося завантажити бібліотеку:', err);
        update((s) => ({ ...s, items: [], isLoading: false, hasLoaded: true }));
      }
    },

    clear: () => set({ items: [], selectedGameId: null, isLoading: false, hasLoaded: false }),

    selectGame: (gameId: string | null) => update((s) => ({ ...s, selectedGameId: gameId })),

    toggleFavorite: async (gameId: string) => {
      update((s) => ({
        ...s,
        items: s.items.map((item) =>
          item.gameId === gameId ? { ...item, isFavorite: !item.isFavorite } : item
        ),
      }));

      try {
        await userService.toggleFavorite(gameId);
      } catch (err: any) {
        update((s) => ({
          ...s,
          items: s.items.map((item) =>
            item.gameId === gameId ? { ...item, isFavorite: !item.isFavorite } : item
          ),
        }));
        uiStore.addToast({
          title: 'Помилка',
          message: err?.message || 'Не вдалося оновити обране.',
          type: 'error',
        });
      }
    },

    buyGame: async (game: Game) => {
      const alreadyOwned = get({ subscribe }).items.some((i) => i.gameId === game.id);
      if (alreadyOwned) return false;

      uiStore.addToast({
        title: 'Purchase Successful!',
        message: `${game.title} has been added to your Library.`,
        type: 'success',
      });

      await userService.getLibrary().then((items) => {
        update((s) => ({ ...s, items: items || [], hasLoaded: true }));
      }).catch(() => {});

      return true;
    },
  };
}

export const libraryStore = createLibraryStore();
