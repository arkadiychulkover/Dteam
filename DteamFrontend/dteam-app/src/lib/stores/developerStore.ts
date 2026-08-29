import { writable } from 'svelte/store';
import type { Game } from '../types';
import type { DeveloperStats, CreateDeveloperGameRequest, UpdateDeveloperGameRequest } from '../types/developer';
import { developerService } from '../services/developerService';
import { uiStore } from './uiStore';
import { gamesStore } from './gamesStore';

interface DeveloperState {
  games: Game[];
  stats: DeveloperStats | null;
  isLoading: boolean;
  isSaving: boolean;
  error: string | null;
}

function createDeveloperStore() {
  const { subscribe, set, update } = writable<DeveloperState>({
    games: [],
    stats: null,
    isLoading: false,
    isSaving: false,
    error: null,
  });

  return {
    subscribe,

    async loadAll() {
      update((s) => ({ ...s, isLoading: true, error: null }));
      try {
        const [games, stats] = await Promise.all([
          developerService.getMyGames(),
          developerService.getStats(),
        ]);
        update((s) => ({ ...s, games, stats, isLoading: false }));
      } catch (err: any) {
        update((s) => ({
          ...s,
          isLoading: false,
          error: err?.message || 'Не вдалося завантажити дані кабінету розробника',
        }));
      }
    },

    async loadMyGames() {
      try {
        const games = await developerService.getMyGames();
        update((s) => ({ ...s, games }));
      } catch (err: any) {
        console.error('[DeveloperStore] Error loading games:', err);
      }
    },

    async loadStats() {
      try {
        const stats = await developerService.getStats();
        update((s) => ({ ...s, stats }));
      } catch (err: any) {
        console.error('[DeveloperStore] Error loading stats:', err);
      }
    },

    async createGame(data: CreateDeveloperGameRequest): Promise<Game | null> {
      update((s) => ({ ...s, isSaving: true }));
      try {
        const newGame = await developerService.createGame(data);
        update((s) => ({
          ...s,
          games: [newGame, ...s.games],
          stats: s.stats ? { ...s.stats, totalGames: s.stats.totalGames + 1 } : null,
          isSaving: false,
        }));
        uiStore.addToast({
          title: 'Гру опубліковано!',
          message: `Гру "${newGame.title}" успішно збережено в каталозі.`,
          type: 'success',
        });
        gamesStore.loadGames();
        return newGame;
      } catch (err: any) {
        update((s) => ({ ...s, isSaving: false }));
        uiStore.addToast({
          title: 'Помилка публікації',
          message: err?.message || 'Не вдалося створити гру.',
          type: 'error',
        });
        throw err;
      }
    },

    async updateGame(id: string, data: UpdateDeveloperGameRequest): Promise<Game | null> {
      update((s) => ({ ...s, isSaving: true }));
      try {
        const updated = await developerService.updateGame(id, data);
        update((s) => ({
          ...s,
          games: s.games.map((g) => (g.id === id ? updated : g)),
          isSaving: false,
        }));
        uiStore.addToast({
          title: 'Оновлено!',
          message: `Дані гри "${updated.title}" успішно збережено.`,
          type: 'success',
        });
        gamesStore.loadGames();
        return updated;
      } catch (err: any) {
        update((s) => ({ ...s, isSaving: false }));
        uiStore.addToast({
          title: 'Помилка оновлення',
          message: err?.message || 'Не вдалося оновити гру.',
          type: 'error',
        });
        throw err;
      }
    },

    async togglePublishStatus(id: string) {
      try {
        const updated = await developerService.togglePublishStatus(id);
        update((s) => ({
          ...s,
          games: s.games.map((g) => (g.id === id ? updated : g)),
        }));
        uiStore.addToast({
          title: updated.isPublished ? 'Опубліковано' : 'Знято з публікації',
          message: `Статус гри "${updated.title}": ${updated.isPublished ? 'Опубліковано в каталозі' : 'Чернетка (приховано)'}`,
          type: 'info',
        });
        gamesStore.loadGames();
      } catch (err: any) {
        uiStore.addToast({
          title: 'Помилка зміни статусу',
          message: err?.message || 'Не вдалося змінити статус гри.',
          type: 'error',
        });
      }
    },

    async deleteGame(id: string) {
      try {
        await developerService.deleteGame(id);
        update((s) => ({
          ...s,
          games: s.games.filter((g) => g.id !== id),
          stats: s.stats ? { ...s.stats, totalGames: Math.max(0, s.stats.totalGames - 1) } : null,
        }));
        uiStore.addToast({
          title: 'Видалено',
          message: 'Гру успішно видалено з платформи.',
          type: 'info',
        });
        gamesStore.loadGames();
      } catch (err: any) {
        uiStore.addToast({
          title: 'Помилка видалення',
          message: err?.message || 'Не вдалося видалити гру.',
          type: 'error',
        });
      }
    },
  };
}

export const developerStore = createDeveloperStore();
