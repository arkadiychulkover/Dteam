import { writable, get } from 'svelte/store';
import type { UserGame, Game, GameCollection } from '../types';
import { gamesStore } from './gamesStore';
import { authStore } from './authStore';
import { uiStore } from './uiStore';
import { userService } from '../services/userService';
import { collectionsService } from '../services/collectionsService';

interface LibraryState {
  items: UserGame[];
  collections: GameCollection[];
  selectedGameId: string | null;
  activeCollectionId: string | 'all' | 'favorites';
  activePrimaryTab: 'games' | 'feed';
  isLoading: boolean;
  hasLoaded: boolean;
}

function createLibraryStore() {
  const { subscribe, set, update } = writable<LibraryState>({
    items: [],
    collections: [],
    selectedGameId: null,
    activeCollectionId: 'all',
    activePrimaryTab: 'games',
    isLoading: false,
    hasLoaded: false,
  });

  return {
    subscribe,

    loadLibrary: async () => {
      update((s) => ({ ...s, isLoading: true }));
      try {
        const [items, collections] = await Promise.allSettled([
          userService.getLibrary(),
          collectionsService.getCollections(),
        ]);

        const resolvedItems = items.status === 'fulfilled' ? (items.value || []) : [];
        const resolvedCollections = collections.status === 'fulfilled' ? (collections.value || []) : [];

        update((s) => {
          const stillExists = s.selectedGameId && resolvedItems.some((i) => i.gameId === s.selectedGameId);
          return {
            ...s,
            items: resolvedItems,
            collections: resolvedCollections,
            selectedGameId: stillExists ? s.selectedGameId : (resolvedItems[0]?.gameId ?? null),
            isLoading: false,
            hasLoaded: true,
          };
        });
      } catch (err) {
        console.warn('[libraryStore] Не вдалося завантажити бібліотеку:', err);
        update((s) => ({ ...s, items: [], collections: [], isLoading: false, hasLoaded: true }));
      }
    },

    loadCollections: async () => {
      try {
        const collections = await collectionsService.getCollections();
        update((s) => ({ ...s, collections: collections || [] }));
      } catch (err) {
        console.warn('[libraryStore] Помилка завантаження колекцій:', err);
      }
    },

    clear: () => set({
      items: [],
      collections: [],
      selectedGameId: null,
      activeCollectionId: 'all',
      activePrimaryTab: 'games',
      isLoading: false,
      hasLoaded: false,
    }),

    selectGame: (gameId: string | null) => update((s) => ({ ...s, selectedGameId: gameId })),

    setActiveCollection: (id: string | 'all' | 'favorites') => update((s) => ({ ...s, activeCollectionId: id })),

    setActivePrimaryTab: (tab: 'games' | 'feed') => update((s) => ({ ...s, activePrimaryTab: tab })),

    createCollection: async (name: string, description?: string, gameIds?: string[]) => {
      try {
        const created = await collectionsService.createCollection({ name, description, gameIds });
        update((s) => ({
          ...s,
          collections: [created, ...s.collections],
          activeCollectionId: created.id,
        }));
        uiStore.addToast({
          title: 'Колекцію створено',
          message: `Колекція "${name}" успішно створена.`,
          type: 'success',
        });
        return created;
      } catch (err: any) {
        uiStore.addToast({
          title: 'Помилка створення колекції',
          message: err?.message || 'Не вдалося створити колекцію.',
          type: 'error',
        });
        throw err;
      }
    },

    updateCollection: async (id: string, name: string, description?: string) => {
      try {
        const updated = await collectionsService.updateCollection(id, { name, description });
        update((s) => ({
          ...s,
          collections: s.collections.map((c) => (c.id === id ? updated : c)),
        }));
        uiStore.addToast({
          title: 'Оновлено',
          message: `Колекцію оновлено.`,
          type: 'success',
        });
        return updated;
      } catch (err: any) {
        uiStore.addToast({
          title: 'Помилка оновлення',
          message: err?.message || 'Не вдалося оновити колекцію.',
          type: 'error',
        });
        throw err;
      }
    },

    deleteCollection: async (id: string) => {
      try {
        await collectionsService.deleteCollection(id);
        update((s) => ({
          ...s,
          collections: s.collections.filter((c) => c.id !== id),
          activeCollectionId: s.activeCollectionId === id ? 'all' : s.activeCollectionId,
        }));
        uiStore.addToast({
          title: 'Видалено',
          message: 'Колекцію успішно видалено.',
          type: 'success',
        });
      } catch (err: any) {
        uiStore.addToast({
          title: 'Помилка видалення',
          message: err?.message || 'Не вдалося видалити колекцію.',
          type: 'error',
        });
      }
    },

    toggleGameInCollection: async (collectionId: string, gameId: string) => {
      const state = get({ subscribe });
      const collection = state.collections.find((c) => c.id === collectionId);
      if (!collection) return;

      const isInCollection = (collection.items ?? []).some((i) => i.gameId === gameId);
      try {
        let updated: GameCollection;
        if (isInCollection) {
          updated = await collectionsService.removeGameFromCollection(collectionId, gameId);
          uiStore.addToast({
            title: 'Видалено з колекції',
            message: `Гру прибрано з колекції "${collection.name}".`,
            type: 'info',
          });
        } else {
          updated = await collectionsService.addGameToCollection(collectionId, gameId);
          uiStore.addToast({
            title: 'Додано до колекції',
            message: `Гру додано до колекції "${collection.name}".`,
            type: 'success',
          });
        }
        update((s) => ({
          ...s,
          collections: s.collections.map((c) => (c.id === collectionId ? updated : c)),
        }));
      } catch (err: any) {
        uiStore.addToast({
          title: 'Помилка',
          message: err?.message || 'Не вдалося оновити колекцію.',
          type: 'error',
        });
      }
    },

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
