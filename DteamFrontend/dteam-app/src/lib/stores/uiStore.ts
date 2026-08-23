import { writable } from 'svelte/store';

export type MainTab = 'store' | 'library' | 'community' | 'profile' | 'developer';

export interface ToastMessage {
  id: string;
  title: string;
  message: string;
  type: 'info' | 'success' | 'warning' | 'error';
}

function createUiStore() {
  const { subscribe, update, set } = writable<{
    activeTab: MainTab;
    isWalletModalOpen: boolean;
    isAddFriendModalOpen: boolean;
    isPublishGameModalOpen: boolean;
    isFriendsSidebarOpen: boolean;
    toasts: ToastMessage[];
  }>({
    activeTab: 'store',
    isWalletModalOpen: false,
    isAddFriendModalOpen: false,
    isPublishGameModalOpen: false,
    isFriendsSidebarOpen: false,
    toasts: [],
  });

  return {
    subscribe,
    setTab: (tab: MainTab) => update((s) => ({ ...s, activeTab: tab })),
    setWalletModal: (isOpen: boolean) => update((s) => ({ ...s, isWalletModalOpen: isOpen })),
    setAddFriendModal: (isOpen: boolean) => update((s) => ({ ...s, isAddFriendModalOpen: isOpen })),
    setPublishGameModal: (isOpen: boolean) => update((s) => ({ ...s, isPublishGameModalOpen: isOpen })),
    toggleFriendsSidebar: () => update((s) => ({ ...s, isFriendsSidebarOpen: !s.isFriendsSidebarOpen })),
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
