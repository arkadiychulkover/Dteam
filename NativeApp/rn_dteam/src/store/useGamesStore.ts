import { create } from 'zustand';
import { gamesService } from '../services/gamesService';
import type { Game, GameFilterParams } from '../types';

interface GamesStoreState {
  games: Game[];
  selectedGame: Game | null;
  selectedGenre: string;
  isLoading: boolean;
  isRefreshing: boolean;
  error: string | null;

  loadGames: (filters?: GameFilterParams) => Promise<void>;
  refresh: () => Promise<void>;
  selectGame: (game: Game | null) => void;
  loadGameDetails: (id: string) => Promise<Game | null>;
  setSelectedGenre: (genre: string) => void;
}

export const useGamesStore = create<GamesStoreState>((set, get) => ({
  games: [],
  selectedGame: null,
  selectedGenre: 'Всі ігри',
  isLoading: false,
  isRefreshing: false,
  error: null,

  loadGames: async (filters?: GameFilterParams) => {
    set({ isLoading: true, error: null });
    try {
      const activeGenre = filters?.genre ?? get().selectedGenre;
      const appliedFilters: GameFilterParams = {
        ...filters,
        genre: activeGenre === 'Всі ігри' ? undefined : activeGenre,
      };

      const games = await gamesService.getGames(appliedFilters);
      set({ games, isLoading: false, error: null });
    } catch (err: any) {
      console.warn('[GamesStore] Error loading games:', err.message);
      set({
        isLoading: false,
        error: err.message || 'Не вдалося завантажити ігри. Перевірте зʼєднання.',
      });
    }
  },

  refresh: async () => {
    set({ isRefreshing: true });
    try {
      const activeGenre = get().selectedGenre;
      const filters = activeGenre !== 'Всі ігри' ? { genre: activeGenre } : undefined;
      const games = await gamesService.getGames(filters);
      set({ games, isRefreshing: false, error: null });
    } catch (err: any) {
      console.warn('[GamesStore] Error refreshing games:', err.message);
      set({ isRefreshing: false });
    }
  },

  selectGame: (game: Game | null) => {
    set({ selectedGame: game });
  },

  loadGameDetails: async (id: string) => {
    try {
      const game = await gamesService.getGameById(id);
      set({ selectedGame: game });
      return game;
    } catch (err: any) {
      console.warn('[GamesStore] Error loading game details:', err.message);
      return null;
    }
  },

  setSelectedGenre: (genre: string) => {
    set({ selectedGenre: genre });
    get().loadGames({ genre });
  },
}));
