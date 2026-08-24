import { writable, derived } from 'svelte/store';
import type { Duser, UserStatus } from '../types';
import { authService } from '../services/authService';

const MOCK_USER: Duser = {
  id: '3f7fc92f-8a38-4c48-98ff-cff02335e850',
  email: 'cyber_gamer@dteam.io',
  username: 'CyberPlayer',
  walletAddress: 'EQBvW8Z5huBkMJYdn3PBRnVDLyTO2_OTHTuP4asMb_Fton',
  balanceInNanoTons: 15_500_000_000,
  totalEarningsInNanoTons: 42_000_000_000,
  createdAt: new Date().toISOString(),
  isInFamily: true,
  isAdmin: true,
  isBanned: false,
  status: 1 as UserStatus,
  avatarUrl: 'https://images.unsplash.com/photo-1566492031773-4f4e44671857?w=150&auto=format&fit=crop&q=80',
  bio: 'Web3 game dev & collector. Exploring the decentralized metaverse 🚀',
};

function createAuthStore() {
  const { subscribe, set, update } = writable<{
    user: Duser | null;
    token: string | null;
    isLoading: boolean;
    error: string | null;
  }>({
    user: MOCK_USER,
    token: 'mock-jwt-token-dteam',
    isLoading: false,
    error: null,
  });

  return {
    subscribe,
    setUser: (user: Duser | null) => update((s) => ({ ...s, user })),
    setStatus: (status: UserStatus) => {
      update((s) => {
        if (!s.user) return s;
        return { ...s, user: { ...s.user, status } };
      });
    },
    updateBalance: (deltaNanoTon: number | bigint) => {
      update((s) => {
        if (!s.user) return s;
        const current = Number(s.user.balanceInNanoTons);
        const newBalance = Math.max(0, current + Number(deltaNanoTon));
        return { ...s, user: { ...s.user, balanceInNanoTons: newBalance } };
      });
    },
    login: async (emailOrUsername: string, password: string) => {
      update((s) => ({ ...s, isLoading: true, error: null }));
      try {
        const res = await authService.login({ emailOrUsername, password });
        set({ user: res.user, token: res.token, isLoading: false, error: null });
      } catch (err: any) {
        update((s) => ({
          ...s,
          isLoading: false,
          user: { ...MOCK_USER, username: emailOrUsername },
          error: null,
        }));
      }
    },
    logout: async () => {
      await authService.logout();
      set({ user: null, token: null, isLoading: false, error: null });
    },
  };
}

export const authStore = createAuthStore();
export const isAuthenticated = derived(authStore, ($auth) => !!$auth.user);
export const currentUser = derived(authStore, ($auth) => $auth.user);
