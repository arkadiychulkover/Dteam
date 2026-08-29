import { writable, derived } from 'svelte/store';
import type { Game, GameFilterParams } from '../types';
import { gamesService } from '../services/gamesService';

export interface CatalogFilterState {
  search: string;
  genre: string;
  isDlc?: boolean;
  isDiscounted: boolean;
  priceFilter: 'all' | 'free' | 'under1' | 'under3' | 'under6' | 'under9';
  platform: string;
  feature: string;
  tagQuery: string;
  sortBy: 'relevance' | 'price_asc' | 'price_desc' | 'rating' | 'newest';
  viewMode: 'grid' | 'list';
}

function createGamesStore() {
  const { subscribe, update, set } = writable<{
    games: Game[];
    catalogGames: Game[];
    selectedGame: Game | null;
    genresList: string[];
    isLoading: boolean;
    catalogLoading: boolean;
    filters: CatalogFilterState;
  }>({
    games: [],
    catalogGames: [],
    selectedGame: null,
    genresList: [],
    isLoading: false,
    catalogLoading: false,
    filters: {
      search: '',
      genre: 'All Games',
      isDiscounted: false,
      priceFilter: 'all',
      platform: '',
      feature: '',
      tagQuery: '',
      sortBy: 'relevance',
      viewMode: 'grid',
    },
  });

  return {
    subscribe,
    selectGame: (game: Game | null) => update((s) => ({ ...s, selectedGame: game })),

    loadGames: async () => {
      update((s) => ({ ...s, isLoading: true }));
      try {
        const [fetched, genres] = await Promise.all([
          gamesService.getGames(),
          gamesService.getGenres().catch(() => [
            'Action', 'RPG', 'Strategy', 'Adventure', 'Shooter',
            'Indie', 'Simulation', 'MMO', 'Horror', 'Sports', 'Racing'
          ]),
        ]);
        update((s) => ({ ...s, games: fetched || [], genresList: genres, isLoading: false }));
      } catch (err) {
        console.warn('[gamesStore] Failed to fetch real games:', err);
        update((s) => ({ ...s, games: [], isLoading: false }));
      }
    },

    setFilters: (newFilters: Partial<CatalogFilterState>) => {
      update((s) => ({ ...s, filters: { ...s.filters, ...newFilters } }));
    },

    resetFilters: () => {
      update((s) => ({
        ...s,
        filters: {
          search: '',
          genre: 'All Games',
          isDiscounted: false,
          priceFilter: 'all',
          platform: '',
          feature: '',
          tagQuery: '',
          sortBy: 'relevance',
          viewMode: s.filters.viewMode,
        },
      }));
    },

    loadCatalogGames: async (customFilters?: Partial<CatalogFilterState>) => {
      update((s) => ({ ...s, catalogLoading: true }));
      let currentFilters: CatalogFilterState;
      update((s) => {
        if (customFilters) {
          s.filters = { ...s.filters, ...customFilters };
        }
        currentFilters = s.filters;
        return s;
      });

      try {
        const queryParams: GameFilterParams = {
          search: currentFilters!.search || undefined,
          genre: currentFilters!.genre !== 'All Games' ? currentFilters!.genre : undefined,
          isDlc: currentFilters!.isDlc,
          isDiscounted: currentFilters!.isDiscounted || undefined,
          platform: currentFilters!.platform || undefined,
          feature: currentFilters!.feature || undefined,
          tag: currentFilters!.tagQuery || undefined,
          sortBy: currentFilters!.sortBy,
        };

        if (currentFilters!.priceFilter === 'free') {
          queryParams.isFree = true;
        } else if (currentFilters!.priceFilter === 'under1') {
          queryParams.maxPrice = 1_000_000_000;
        } else if (currentFilters!.priceFilter === 'under3') {
          queryParams.maxPrice = 3_000_000_000;
        } else if (currentFilters!.priceFilter === 'under6') {
          queryParams.maxPrice = 6_000_000_000;
        } else if (currentFilters!.priceFilter === 'under9') {
          queryParams.maxPrice = 9_000_000_000;
        }

        const fetched = await gamesService.getGames(queryParams);
        update((s) => ({ ...s, catalogGames: fetched || [], catalogLoading: false }));
      } catch (err) {
        console.warn('[gamesStore] Failed to fetch catalog games:', err);
        update((s) => ({ ...s, catalogGames: [], catalogLoading: false }));
      }
    },
  };
}

export const gamesStore = createGamesStore();

