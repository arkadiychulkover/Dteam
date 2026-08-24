import { writable, derived, get } from 'svelte/store';
import type { Game, WishlistItem } from '../types';
import { wishlistService } from '../services/wishlistService';
import { authStore, currentUser } from './authStore';
import { uiStore } from './uiStore';

function createWishlistStore() {
  const { subscribe, set, update } = writable<{
    items: WishlistItem[];
    wishlistGameIds: Set<string>;
    isLoading: boolean;
    isModalOpen: boolean;
  }>({
    items: [],
    wishlistGameIds: new Set<string>(),
    isLoading: false,
    isModalOpen: false,
  });

  return {
    subscribe,

    openModal: () => update((s) => ({ ...s, isModalOpen: true })),
    closeModal: () => update((s) => ({ ...s, isModalOpen: false })),

    loadWishlist: async (userId?: string) => {
      const uid = userId || get(currentUser)?.id;
      if (!uid) return;

      update((s) => ({ ...s, isLoading: true }));
      try {
        const items = await wishlistService.getWishlist(uid);
        const gameIds = new Set(items.map((i) => i.gameId));
        update((s) => ({
          ...s,
          items: items || [],
          wishlistGameIds: gameIds,
          isLoading: false,
        }));
      } catch (err) {
        console.warn('[wishlistStore] Failed to load wishlist:', err);
        update((s) => ({ ...s, isLoading: false }));
      }
    },

    toggleWishlist: async (game: Game, userId?: string) => {
      const uid = userId || get(currentUser)?.id;
      if (!uid) {
        uiStore.addToast({
          title: 'Увійдіть в акаунт',
          message: 'Для додавання до списку бажань потрібен активний акаунт.',
          type: 'warning',
        });
        return;
      }

      const state = get({ subscribe });
      const isAlreadyIn = state.wishlistGameIds.has(game.id);

      if (isAlreadyIn) {
        update((s) => {
          const nextGameIds = new Set(s.wishlistGameIds);
          nextGameIds.delete(game.id);
          return {
            ...s,
            items: s.items.filter((i) => i.gameId !== game.id),
            wishlistGameIds: nextGameIds,
          };
        });

        try {
          await wishlistService.removeFromWishlist(game.id, uid);
          uiStore.addToast({
            title: 'Видалено зі списку бажань',
            message: `Гру '${game.title}' видалено зі списку бажань.`,
            type: 'info',
          });
        } catch (err: any) {
          uiStore.addToast({ title: 'Помилка', message: err.message, type: 'error' });
          const items = await wishlistService.getWishlist(uid);
          update((s) => ({ ...s, items, wishlistGameIds: new Set(items.map((i) => i.gameId)) }));
        }
      } else {
        const dummyItem: WishlistItem = {
          userId: uid,
          gameId: game.id,
          game,
          addedAt: new Date().toISOString(),
          priority: 0,
        };

        update((s) => {
          const nextGameIds = new Set(s.wishlistGameIds);
          nextGameIds.add(game.id);
          return {
            ...s,
            items: [dummyItem, ...s.items],
            wishlistGameIds: nextGameIds,
          };
        });

        try {
          const newItem = await wishlistService.addToWishlist(uid, { gameId: game.id });
          update((s) => ({
            ...s,
            items: [newItem, ...s.items.filter((i) => i.gameId !== game.id)],
          }));

          uiStore.addToast({
            title: 'Додано до бажаного ♥',
            message: `Гру '${game.title}' додано до вашого списку бажань!`,
            type: 'success',
          });
        } catch (err: any) {
          uiStore.addToast({ title: 'Помилка', message: err.message, type: 'error' });
          update((s) => {
            const nextGameIds = new Set(s.wishlistGameIds);
            nextGameIds.delete(game.id);
            return {
              ...s,
              items: s.items.filter((i) => i.gameId !== game.id),
              wishlistGameIds: nextGameIds,
            };
          });
        }
      }
    },

    clearWishlist: async (userId?: string) => {
      const uid = userId || get(currentUser)?.id;
      if (!uid) return;

      try {
        await wishlistService.clearWishlist(uid);
        update((s) => ({ ...s, items: [], wishlistGameIds: new Set() }));
        uiStore.addToast({
          title: 'Список бажань очищено',
          message: 'Усі ігри видалено зі списку бажань.',
          type: 'info',
        });
      } catch (err: any) {
        uiStore.addToast({ title: 'Помилка', message: err.message, type: 'error' });
      }
    },
  };
}

export const wishlistStore = createWishlistStore();

export const isGameInWishlist = (gameId: string) => {
  return derived(wishlistStore, ($s) => $s.wishlistGameIds.has(gameId));
};
