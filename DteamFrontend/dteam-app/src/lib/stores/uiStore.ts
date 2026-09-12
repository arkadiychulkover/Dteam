import { writable, get } from 'svelte/store';
import type { Game } from '../types';
import { soundService } from '../services/soundService';
import { gamesService } from '../services/gamesService';
import { gamesStore } from './gamesStore';

export type MainTab =
  | 'store'
  | 'library'
  | 'chat'
  | 'community'
  | 'friends'
  | 'catalog'
  | 'game'
  | 'all-dlcs'
  | 'wishlist'
  | 'cart'
  | 'admin'
  | 'developer'
  | 'profile'
  | 'my-profile'
  | 'wallet'
  | 'terms'
  | 'privacy'
  | 'refund'
  | 'login'
  | 'register'
  | 'forgot-password'
  | 'confirm-code'
  | 'reset-password'
  | 'settings';

export interface ToastMessage {
  id: string;
  title: string;
  message: string;
  type: 'info' | 'success' | 'warning' | 'error';
  duration?: number;
  createdAt?: number;
}

function stripEmojis(text: string): string {
  if (!text) return '';
  return text
    .replace(/[\u{1F300}-\u{1FAD6}\u{1F600}-\u{1F64F}\u{1F680}-\u{1F6FF}\u{2600}-\u{26FF}\u{2700}-\u{27BF}\u{1F900}-\u{1F9FF}\u{1F1E0}-\u{1F1FF}♥❤🎉🛒⚠️⚠💡🚀✨]/gu, '')
    .trim();
}

function createUiStore() {
  const { subscribe, update, set } = writable<{
    activeTab: MainTab;
    isLoginModalOpen: boolean;
    isConfirmCodeModalOpen: boolean;
    isDepositModalOpen: boolean;
    isFriendsSidebarOpen: boolean;
    isPublishGameModalOpen: boolean;
    isEditGameModalOpen: boolean;
    editingGame: Game | null;
    toasts: ToastMessage[];
  }>({
    activeTab: 'store',
    isLoginModalOpen: false,
    isConfirmCodeModalOpen: false,
    isDepositModalOpen: false,
    isFriendsSidebarOpen: false,
    isPublishGameModalOpen: false,
    isEditGameModalOpen: false,
    editingGame: null,
    toasts: [],
  });

  return {
    subscribe,
    setTab: (tab: MainTab) => update((s) => ({ ...s, activeTab: tab })),
    openGameDetails: async (gameId: string) => {
      try {
        const fullGame = await gamesService.getGameById(gameId);
        gamesStore.selectGame(fullGame);
      } catch {
        const currentGames = get(gamesStore).games;
        const cached = currentGames.find((g) => g.id === gameId);
        if (cached) {
          gamesStore.selectGame(cached);
        }
      }
      update((s) => ({ ...s, activeTab: 'game' }));
      if (typeof window !== 'undefined') {
        window.scrollTo({ top: 0, behavior: 'smooth' });
      }
    },
    setLoginModal: (isOpen: boolean) => update((s) => ({ ...s, isLoginModalOpen: isOpen })),
    setConfirmCodeModal: (isOpen: boolean) => update((s) => ({ ...s, isConfirmCodeModalOpen: isOpen })),
    setDepositModal: (isOpen: boolean) => update((s) => ({ ...s, isDepositModalOpen: isOpen })),
    toggleFriendsSidebar: () => update((s) => ({ ...s, isFriendsSidebarOpen: !s.isFriendsSidebarOpen })),
    setPublishGameModal: (isOpen: boolean) => update((s) => ({ ...s, isPublishGameModalOpen: isOpen })),
    setEditGameModal: (isOpen: boolean, game: Game | null = null) =>
      update((s) => ({ ...s, isEditGameModalOpen: isOpen, editingGame: game })),
    addToast: (toast: Omit<ToastMessage, 'id'> & { duration?: number }) => {
      const id = Math.random().toString(36).substring(2, 9);
      const cleanTitle = stripEmojis(toast.title || '');
      const cleanMessage = stripEmojis(toast.message || '');

      const textLen = cleanTitle.length + cleanMessage.length;
      const duration = toast.duration ?? Math.max(7500, Math.min(14000, 6500 + textLen * 50));

      const newToast: ToastMessage = {
        ...toast,
        id,
        title: cleanTitle,
        message: cleanMessage,
        duration,
        createdAt: Date.now(),
      };

      soundService.playNotification(toast.type);

      update((s) => ({
        ...s,
        toasts: [...s.toasts, newToast],
      }));

      setTimeout(() => {
        update((s) => ({
          ...s,
          toasts: s.toasts.filter((t) => t.id !== id),
        }));
      }, duration + 10000);
    },
    removeToast: (id: string) => {
      update((s) => ({
        ...s,
        toasts: s.toasts.filter((t) => t.id !== id),
      }));
    },
  };
}

export const uiStore = createUiStore();
