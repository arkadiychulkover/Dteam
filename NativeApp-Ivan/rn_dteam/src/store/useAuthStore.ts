import { create } from 'zustand';
import { authService } from '../services/authService';
import { tokenStorage } from '../services/tokenStorage';
import type { UserDto } from '../types';

interface AuthStoreState {
  user: UserDto | null;
  isAuthenticated: boolean;
  isLoading: boolean;

  checkAuth: () => Promise<void>;
  setUser: (user: UserDto | null) => void;
  loginSuccess: (user: UserDto) => void;
  logout: () => Promise<void>;
}

export const useAuthStore = create<AuthStoreState>((set) => ({
  user: null,
  isAuthenticated: false,
  isLoading: true,

  checkAuth: async () => {
    set({ isLoading: true });
    try {
      await tokenStorage.init();
      const token = await tokenStorage.getToken();
      if (!token || tokenStorage.isTokenExpired(token)) {
        set({ user: null, isAuthenticated: false, isLoading: false });
        return;
      }

      const user = await authService.getMe();
      set({ user, isAuthenticated: true, isLoading: false });
    } catch (err) {
      console.warn('[AuthStore] Failed to check current user session:', err);
      set({ user: null, isAuthenticated: false, isLoading: false });
    }
  },

  setUser: (user) => {
    set({ user, isAuthenticated: !!user });
  },

  loginSuccess: (user) => {
    set({ user, isAuthenticated: true, isLoading: false });
  },

  logout: async () => {
    try {
      await authService.logout();
    } catch (e) {
      console.warn('[AuthStore] Logout error:', e);
    } finally {
      set({ user: null, isAuthenticated: false, isLoading: false });
    }
  },
}));
