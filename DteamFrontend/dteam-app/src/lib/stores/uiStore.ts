import { writable } from 'svelte/store';
import type { Game } from '../types';

export type MainTab = 
  | 'store' 
  | 'library'
  | 'community'
  | 'friends'
  | 'catalog' 
  | 'game'
  | 'wishlist' 
  | 'cart'
  | 'admin'
  | 'developer'
  | 'profile'
  | 'my-profile'
  | 'login'
  | 'register'
  | 'forgot-password'
  | 'confirm-code'
  | 'reset-password';

export interface ToastMessage {
  id: string;
  title: string;
  message: string;
  type: 'info' | 'success' | 'warning' | 'error';
}

function createUiStore() {
  const { subscribe, update, set } = writable<{
    activeTab: MainTab;
    isLoginModalOpen: boolean;
    isConfirmCodeModalOpen: boolean;
    isDepositModalOpen: boolean;
    isFriendsSidebarOpen: boolean;
    isWalletModalOpen: boolean;
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
    isWalletModalOpen: false,
    isPublishGameModalOpen: false,
    isEditGameModalOpen: false,
    editingGame: null,
    toasts: [],
  });

  return {
    subscribe,
    setTab: (tab: MainTab) => update((s) => ({ ...s, activeTab: tab })),
    setLoginModal: (isOpen: boolean) => update((s) => ({ ...s, isLoginModalOpen: isOpen })),
    setConfirmCodeModal: (isOpen: boolean) => update((s) => ({ ...s, isConfirmCodeModalOpen: isOpen })),
    setDepositModal: (isOpen: boolean) => update((s) => ({ ...s, isDepositModalOpen: isOpen })),
    toggleFriendsSidebar: () => update((s) => ({ ...s, isFriendsSidebarOpen: !s.isFriendsSidebarOpen })),
    setWalletModal: (isOpen: boolean) => update((s) => ({ ...s, isWalletModalOpen: isOpen })),
    setPublishGameModal: (isOpen: boolean) => update((s) => ({ ...s, isPublishGameModalOpen: isOpen })),
    setEditGameModal: (isOpen: boolean, game: Game | null = null) =>
      update((s) => ({ ...s, isEditGameModalOpen: isOpen, editingGame: game })),
    addToast: (toast: Omit<ToastMessage, 'id'>) => {
      const id = Math.random().toString(36).substring(2, 9);
      update((s) => ({
        ...s,
        toasts: [...s.toasts, { ...toast, id }],
      }));
      setTimeout(() => {
        update((s) => ({
          ...s,
          toasts: s.toasts.filter((t) => t.id !== id),
        }));
      }, 4000);
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
