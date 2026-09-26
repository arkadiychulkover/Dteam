import { create } from 'zustand';

interface PresenceState {
  onlineUserIds: Set<string>;
  isOnline: (userId?: string | null) => boolean;
  setInitialOnlineFriends: (userIds: string[]) => void;
  setUserOnline: (userId: string) => void;
  setUserOffline: (userId: string) => void;
  clear: () => void;
}

export const usePresenceStore = create<PresenceState>((set, get) => ({
  onlineUserIds: new Set<string>(),

  isOnline: (userId?: string | null) => {
    if (!userId) return false;
    return get().onlineUserIds.has(userId.toLowerCase());
  },

  setInitialOnlineFriends: (userIds: string[]) => {
    const nextSet = new Set<string>();
    userIds.forEach((id) => {
      if (id) nextSet.add(id.toLowerCase());
    });
    set({ onlineUserIds: nextSet });
  },

  setUserOnline: (userId: string) => {
    if (!userId) return;
    const lower = userId.toLowerCase();
    if (get().onlineUserIds.has(lower)) return;
    const updated = new Set(get().onlineUserIds);
    updated.add(lower);
    set({ onlineUserIds: updated });
  },

  setUserOffline: (userId: string) => {
    if (!userId) return;
    const lower = userId.toLowerCase();
    if (!get().onlineUserIds.has(lower)) return;
    const updated = new Set(get().onlineUserIds);
    updated.delete(lower);
    set({ onlineUserIds: updated });
  },

  clear: () => {
    set({ onlineUserIds: new Set<string>() });
  },
}));
