import { get } from 'svelte/store';
import { uiStore, type MainTab } from '../stores/uiStore';
import { gamesStore } from '../stores/gamesStore';
import { profileStore } from '../stores/profileStore';
import { gamesService } from './gamesService';
import type { Game } from '../types';

let isNavigatingFromUrl = false;
let isInitialized = false;

export interface RouteState {
  tab: MainTab;
  gameId?: string;
  userId?: string;
}

function getPathForState(tab: MainTab, game?: Game | null, userId?: string | null): string {
  switch (tab) {
    case 'store':
      return '/';
    case 'catalog':
      return '/catalog';
    case 'library':
      return '/library';
    case 'community':
      return '/community';
    case 'friends':
      return '/friends';
    case 'wishlist':
      return '/wishlist';
    case 'cart':
      return '/cart';
    case 'admin':
      return '/admin';
    case 'developer':
      return '/developer';
    case 'my-profile':
      return '/my-profile';
    case 'login':
      return '/login';
    case 'register':
      return '/register';
    case 'forgot-password':
      return '/forgot-password';
    case 'confirm-code':
      return '/confirm-code';
    case 'reset-password':
      return '/reset-password';
    case 'game':
      return game?.id ? `/game/${game.id}` : '/game';
    case 'all-dlcs':
      return game?.id ? `/all-dlcs/${game.id}` : '/all-dlcs';
    case 'profile':
      return userId ? `/profile/${userId}` : '/profile';
    default:
      return '/';
  }
}

function parseLocation(): RouteState {
  if (typeof window === 'undefined') {
    return { tab: 'store' };
  }

  // Support both HTML5 path and Hash fallback
  let path = window.location.pathname;
  if (window.location.hash && window.location.hash.startsWith('#/')) {
    path = window.location.hash.slice(1);
  }

  // Remove trailing slashes
  path = path.replace(/\/+$/, '') || '/';

  const segments = path.split('/').filter(Boolean);
  const first = segments[0] || '';
  const second = segments[1] || '';

  // Check URL query parameters for fallback (e.g. ?gameId=... or ?id=...)
  const params = new URLSearchParams(window.location.search);
  const queryGameId = params.get('gameId') || params.get('id') || undefined;
  const queryUserId = params.get('userId') || undefined;

  switch (first) {
    case '':
    case 'store':
      return { tab: 'store' };
    case 'catalog':
      return { tab: 'catalog' };
    case 'library':
      return { tab: 'library' };
    case 'community':
      return { tab: 'community', gameId: second || queryGameId };
    case 'friends':
      return { tab: 'friends' };
    case 'wishlist':
      return { tab: 'wishlist' };
    case 'cart':
      return { tab: 'cart' };
    case 'admin':
      return { tab: 'admin' };
    case 'developer':
      return { tab: 'developer' };
    case 'my-profile':
      return { tab: 'my-profile' };
    case 'login':
      return { tab: 'login' };
    case 'register':
      return { tab: 'register' };
    case 'forgot-password':
      return { tab: 'forgot-password' };
    case 'confirm-code':
      return { tab: 'confirm-code' };
    case 'reset-password':
      return { tab: 'reset-password' };
    case 'game':
      return { tab: 'game', gameId: second || queryGameId };
    case 'all-dlcs':
      return { tab: 'all-dlcs', gameId: second || queryGameId };
    case 'profile':
      return { tab: 'profile', userId: second || queryUserId };
    default:
      return { tab: 'store' };
  }
}

async function applyRoute(route: RouteState) {
  isNavigatingFromUrl = true;

  try {
    // 1. Handle Game loading if route is 'game' or 'all-dlcs'
    if ((route.tab === 'game' || route.tab === 'all-dlcs') && route.gameId) {
      const currentSelected = get(gamesStore).selectedGame;
      if (!currentSelected || currentSelected.id !== route.gameId) {
        try {
          const loadedGame = await gamesService.getGameById(route.gameId);
          if (loadedGame) {
            gamesStore.selectGame(loadedGame);
          }
        } catch (e) {
          console.warn('[Router] Could not fetch game by ID:', route.gameId, e);
        }
      }
    }

    // 2. Handle Profile loading if route is 'profile'
    if (route.tab === 'profile' && route.userId) {
      profileStore.viewProfile(route.userId);
    }

    // 3. Set the active tab in uiStore
    uiStore.setTab(route.tab);
  } finally {
    isNavigatingFromUrl = false;
  }
}

export const router = {
  init() {
    if (isInitialized || typeof window === 'undefined') return;
    isInitialized = true;

    // Handle initial page load
    const initialRoute = parseLocation();
    applyRoute(initialRoute);

    // Listen to browser Back/Forward navigation
    window.addEventListener('popstate', () => {
      const route = parseLocation();
      applyRoute(route);
    });

    // Listen to tab and selected game changes to synchronize URL
    let lastUrl = window.location.pathname;

    uiStore.subscribe(($ui) => {
      if (isNavigatingFromUrl) return;

      const $games = get(gamesStore);
      const $profile = get(profileStore);
      const targetPath = getPathForState($ui.activeTab, $games.selectedGame, $profile.viewedUserId);

      if (window.location.pathname !== targetPath && targetPath !== lastUrl) {
        lastUrl = targetPath;
        window.history.pushState({ tab: $ui.activeTab }, '', targetPath);
      }
    });

    gamesStore.subscribe(($games) => {
      if (isNavigatingFromUrl) return;

      const $ui = get(uiStore);
      if ($ui.activeTab === 'game' || $ui.activeTab === 'all-dlcs') {
        const targetPath = getPathForState($ui.activeTab, $games.selectedGame);
        if (window.location.pathname !== targetPath && targetPath !== lastUrl) {
          lastUrl = targetPath;
          window.history.pushState({ tab: $ui.activeTab, gameId: $games.selectedGame?.id }, '', targetPath);
        }
      }
    });
  },

  navigate(tab: MainTab) {
    uiStore.setTab(tab);
    if (typeof window !== 'undefined') {
      window.scrollTo({ top: 0, behavior: 'smooth' });
    }
  },

  navigateToGame(game: Game) {
    gamesStore.selectGame(game);
    uiStore.setTab('game');
    if (typeof window !== 'undefined') {
      window.scrollTo({ top: 0, behavior: 'smooth' });
    }
  },

  navigateToAllDlcs(game: Game | string) {
    if (typeof game === 'object' && game !== null) {
      gamesStore.selectGame(game);
    }
    uiStore.setTab('all-dlcs');
    if (typeof window !== 'undefined') {
      window.scrollTo({ top: 0, behavior: 'smooth' });
    }
  },

  navigateToProfile(userId: string) {
    profileStore.viewProfile(userId);
    if (typeof window !== 'undefined') {
      window.scrollTo({ top: 0, behavior: 'smooth' });
    }
  }
};
