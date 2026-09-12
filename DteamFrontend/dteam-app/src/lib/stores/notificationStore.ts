import { writable, get } from 'svelte/store';
import { api } from '../services/api';
import { uiStore } from './uiStore';
import { chatStore } from './chatStore';
import { notificationHubService } from '../services/notificationHubService';
import type { AppNotification, NotificationFeedResponse } from '../types/notification';

interface NotificationState {
  notifications: AppNotification[];
  unreadCount: number;
  isLoading: boolean;
  hasMore: boolean;
  nextCursor: string | null;
  isDropdownOpen: boolean;
}

const initialState: NotificationState = {
  notifications: [],
  unreadCount: 0,
  isLoading: false,
  hasMore: false,
  nextCursor: null,
  isDropdownOpen: false,
};

function stripEmojis(text: string): string {
  if (!text) return '';
  return text
    .replace(/[\u{1F300}-\u{1FAD6}\u{1F600}-\u{1F64F}\u{1F680}-\u{1F6FF}\u{2600}-\u{26FF}\u{2700}-\u{27BF}\u{1F900}-\u{1F9FF}\u{1F1E0}-\u{1F1FF}♥❤🎉🛒⚠️⚠💡🚀✨]/gu, '')
    .trim();
}

function sanitizeNotification(notification: AppNotification): AppNotification {
  return {
    ...notification,
    title: stripEmojis(notification.title),
    message: stripEmojis(notification.message),
  };
}

function createNotificationStore() {
  const { subscribe, set, update } = writable<NotificationState>(initialState);
  let initialized = false;

  const initRealtimeListener = () => {
    notificationHubService.onReceiveNotification((rawNotification: AppNotification) => {
      const notification = sanitizeNotification(rawNotification);

      update(state => {

        if (state.notifications.some(n => n.id === notification.id)) {
          return state;
        }

        const nextNotifications = [notification, ...state.notifications];
        const nextUnreadCount = notification.isRead ? state.unreadCount : state.unreadCount + 1;

        return {
          ...state,
          notifications: nextNotifications,
          unreadCount: nextUnreadCount,
        };
      });

      const chatState = get(chatStore);
      const uiState = get(uiStore);
      const isChattingWithActor =
        notification.type === 'chat_message' &&
        uiState.activeTab === 'chat' &&
        chatState.activeFriendId === notification.actor?.id;

      if (!isChattingWithActor) {
        let toastType: 'info' | 'success' | 'warning' = 'info';
        if (notification.type === 'wallet_deposit' || notification.type === 'friend_accepted') {
          toastType = 'success';
        }

        uiStore.addToast({
          type: toastType,
          title: notification.title,
          message: notification.message,
        });
      }
    });
  };

  return {
    subscribe,

    init: async () => {
      if (!initialized) {
        initialized = true;
        initRealtimeListener();
      }

      await notificationHubService.start();
      await notificationStore.loadFeed();
    },

    reset: () => {
      notificationHubService.stop();
      set(initialState);
    },

    loadFeed: async () => {
      update(s => ({ ...s, isLoading: true }));
      try {
        const response = await api.get<NotificationFeedResponse>('/notifications?limit=30');
        const items = (response.items || []).map(sanitizeNotification);
        update(s => ({
          ...s,
          notifications: items,
          unreadCount: response.unreadCount || 0,
          hasMore: response.hasMore || false,
          nextCursor: response.nextCursor || null,
          isLoading: false,
        }));
      } catch (err) {
        console.warn('[NotificationStore] Failed to load notifications:', err);
        update(s => ({ ...s, isLoading: false }));
      }
    },

    loadMore: async () => {
      const state = get({ subscribe });
      if (state.isLoading || !state.hasMore || !state.nextCursor) {
        return;
      }

      update(s => ({ ...s, isLoading: true }));
      try {
        const url = `/notifications?limit=30&before=${encodeURIComponent(state.nextCursor)}`;
        const response = await api.get<NotificationFeedResponse>(url);
        const newItems = (response.items || []).map(sanitizeNotification);

        update(s => ({
          ...s,
          notifications: [...s.notifications, ...newItems],
          unreadCount: response.unreadCount ?? s.unreadCount,
          hasMore: response.hasMore || false,
          nextCursor: response.nextCursor || null,
          isLoading: false,
        }));
      } catch (err) {
        console.warn('[NotificationStore] Failed to load more notifications:', err);
        update(s => ({ ...s, isLoading: false }));
      }
    },

    markAsRead: async (id: string) => {
      update(s => {
        const target = s.notifications.find(n => n.id === id);
        const wasUnread = target && !target.isRead;

        return {
          ...s,
          unreadCount: wasUnread ? Math.max(0, s.unreadCount - 1) : s.unreadCount,
          notifications: s.notifications.map(n =>
            n.id === id ? { ...n, isRead: true, readAt: new Date().toISOString() } : n
          ),
        };
      });

      try {
        await api.put(`/notifications/${id}/read`);
      } catch (err) {
        console.warn(`[NotificationStore] Failed to mark ${id} as read:`, err);
      }
    },

    markAllAsRead: async () => {
      update(s => ({
        ...s,
        unreadCount: 0,
        notifications: s.notifications.map(n => ({
          ...n,
          isRead: true,
          readAt: n.readAt || new Date().toISOString(),
        })),
      }));

      try {
        await api.put('/notifications/read-all');
      } catch (err) {
        console.warn('[NotificationStore] Failed to mark all as read:', err);
      }
    },

    deleteNotification: async (id: string) => {
      update(s => {
        const target = s.notifications.find(n => n.id === id);
        const wasUnread = target && !target.isRead;

        return {
          ...s,
          unreadCount: wasUnread ? Math.max(0, s.unreadCount - 1) : s.unreadCount,
          notifications: s.notifications.filter(n => n.id !== id),
        };
      });

      try {
        await api.delete(`/notifications/${id}`);
      } catch (err) {
        console.warn(`[NotificationStore] Failed to delete notification ${id}:`, err);
      }
    },

    toggleDropdown: () => {
      update(s => ({ ...s, isDropdownOpen: !s.isDropdownOpen }));
    },

    closeDropdown: () => {
      update(s => ({ ...s, isDropdownOpen: false }));
    },
  };
}

export const notificationStore = createNotificationStore();
