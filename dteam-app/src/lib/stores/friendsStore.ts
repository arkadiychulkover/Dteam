import { writable } from 'svelte/store';
import type { UserFriend, Duser } from '../types';
import { UserStatus, FriendshipStatus } from '../types';

const SAMPLE_FRIENDS: UserFriend[] = [
  {
    userId: 'd199452b-23fa-4cf3-a633-8fe5932599fa',
    friendId: 'f1',
    status: FriendshipStatus.Accepted,
    createdAt: '2026-01-10T12:00:00Z',
    friend: {
      id: 'f1',
      username: 'NeoValkyrie',
      email: 'valk@dteam.io',
      balanceInNanoTons: 0,
      totalEarningsInNanoTons: 0,
      createdAt: '',
      isInFamily: false,
      isAdmin: false,
      isBanned: false,
      status: UserStatus.InGame,
      avatarUrl: 'https://images.unsplash.com/photo-1534528741775-53994a69daeb?w=150&auto=format&fit=crop&q=80',
      bio: 'Playing Neon Odyssey 2088',
    },
  },
  {
    userId: 'd199452b-23fa-4cf3-a633-8fe5932599fa',
    friendId: 'f2',
    status: FriendshipStatus.Accepted,
    createdAt: '2026-01-12T12:00:00Z',
    friend: {
      id: 'f2',
      username: 'TonWhale99',
      email: 'whale@dteam.io',
      balanceInNanoTons: 0,
      totalEarningsInNanoTons: 0,
      createdAt: '',
      isInFamily: true,
      isAdmin: false,
      isBanned: false,
      status: UserStatus.Online,
      avatarUrl: 'https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=150&auto=format&fit=crop&q=80',
    },
  },
  {
    userId: 'd199452b-23fa-4cf3-a633-8fe5932599fa',
    friendId: 'f3',
    status: FriendshipStatus.Accepted,
    createdAt: '2026-01-20T12:00:00Z',
    friend: {
      id: 'f3',
      username: 'ShadowNinja',
      email: 'ninja@dteam.io',
      balanceInNanoTons: 0,
      totalEarningsInNanoTons: 0,
      createdAt: '',
      isInFamily: false,
      isAdmin: false,
      isBanned: false,
      status: UserStatus.Offline,
      avatarUrl: 'https://images.unsplash.com/photo-1500648767791-00dcc994a43e?w=150&auto=format&fit=crop&q=80',
    },
  },
];

function createFriendsStore() {
  const { subscribe, set, update } = writable<{
    friends: UserFriend[];
  }>({
    friends: SAMPLE_FRIENDS,
  });

  return {
    subscribe,
    addFriend: (usernameOrEmail: string) => {
      const newFriend: UserFriend = {
        userId: 'd199452b-23fa-4cf3-a633-8fe5932599fa',
        friendId: Math.random().toString(36).substring(2, 9),
        status: FriendshipStatus.Accepted,
        createdAt: new Date().toISOString(),
        friend: {
          id: Math.random().toString(36).substring(2, 9),
          username: usernameOrEmail.split('@')[0],
          email: usernameOrEmail,
          balanceInNanoTons: 0,
          totalEarningsInNanoTons: 0,
          createdAt: new Date().toISOString(),
          isInFamily: false,
          isAdmin: false,
          isBanned: false,
          status: UserStatus.Online,
          avatarUrl: 'https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?w=150&auto=format&fit=crop&q=80',
        },
      };
      update((s) => ({ ...s, friends: [newFriend, ...s.friends] }));
    },
  };
}

export const friendsStore = createFriendsStore();
