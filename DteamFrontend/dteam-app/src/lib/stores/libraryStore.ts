import { writable } from 'svelte/store';
import type { UserGame, Game } from '../types';
import { gamesStore } from './gamesStore';
import { authStore } from './authStore';
import { uiStore } from './uiStore';

const INITIAL_LIBRARY: UserGame[] = [
  {
    userId: 'd199452b-23fa-4cf3-a633-8fe5932599fa',
    gameId: '1',
    purchasedAt: '2026-02-10T15:00:00Z',
    playTimeMinutes: 450, // 7.5 hours
    lastPlayedAt: '2026-08-20T21:30:00Z',
    isFavorite: true,
  },
  {
    userId: 'd199452b-23fa-4cf3-a633-8fe5932599fa',
    gameId: '3',
    purchasedAt: '2026-03-12T11:00:00Z',
    playTimeMinutes: 120, // 2 hours
    lastPlayedAt: '2026-08-18T18:15:00Z',
    isFavorite: false,
  },
];

function createLibraryStore() {
  const { subscribe, set, update } = writable<{
    items: UserGame[];
    selectedGameId: string | null;
  }>({
    items: INITIAL_LIBRARY,
    selectedGameId: '1',
  });

  return {
    subscribe,
    selectGame: (gameId: string | null) => update((s) => ({ ...s, selectedGameId: gameId })),
    toggleFavorite: (gameId: string) => {
      update((s) => ({
        ...s,
        items: s.items.map((item) =>
          item.gameId === gameId ? { ...item, isFavorite: !item.isFavorite } : item
        ),
      }));
    },
    buyGame: (game: Game) => {
      let isAlreadyOwned = false;
      update((s) => {
        if (s.items.some((i) => i.gameId === game.id)) {
          isAlreadyOwned = true;
          return s;
        }
        const price = Number(game.priceInNanoTons);
        authStore.updateBalance(-price);
        uiStore.addToast({
          title: 'Purchase Successful!',
          message: `${game.title} has been added to your Library.`,
          type: 'success',
        });
        return {
          ...s,
          items: [
            ...s.items,
            {
              userId: 'd199452b-23fa-4cf3-a633-8fe5932599fa',
              gameId: game.id,
              purchasedAt: new Date().toISOString(),
              playTimeMinutes: 0,
              lastPlayedAt: null,
              isFavorite: false,
            },
          ],
        };
      });
      return !isAlreadyOwned;
    },
  };
}

export const libraryStore = createLibraryStore();
