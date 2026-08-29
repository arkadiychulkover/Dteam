import { writable, get } from 'svelte/store';
import type { FriendDto, FriendRequestDto } from '../types/friend';
import { UserStatus } from '../types/user';
import { friendsService } from '../services/friendsService';
import { currentUser } from './authStore';
import { uiStore } from './uiStore';

function createFriendsStore() {
  const { subscribe, set, update } = writable<{
    friends: FriendDto[];
    requests: FriendRequestDto[];
    blocked: FriendDto[];
    onlineUserIds: Set<string>;
    isLoading: boolean;
    error: string | null;
  }>({
    friends: [],
    requests: [],
    blocked: [],
    onlineUserIds: new Set<string>(),
    isLoading: false,
    error: null,
  });

  return {
    subscribe,

    loadAll: async () => {
      const user = get(currentUser);
      if (!user?.id) return;

      update((s) => ({ ...s, isLoading: true, error: null }));
      try {
        const [friendsList, requestsList, blockedList] = await Promise.all([
          friendsService.getFriends().catch(() => []),
          friendsService.getFriendRequests('incoming').catch(() => []),
          friendsService.getBlocked().catch(() => [])
        ]);

        const uniqueFriendsList = Array.from(new Map(friendsList.map((f) => [f.id, f])).values());
        update((s) => ({
          ...s,
          friends: uniqueFriendsList.map((f) => ({
            ...f,
            status: s.onlineUserIds.has(f.id.toLowerCase()) ? UserStatus.Online : f.status
          })),
          requests: requestsList,
          blocked: blockedList,
          isLoading: false
        }));
      } catch (err: any) {
        update((s) => ({ ...s, isLoading: false, error: err.message }));
      }
    },

    loadFriends: async () => {
      const user = get(currentUser);
      if (!user?.id) return;

      try {
        const list = await friendsService.getFriends();
        const uniqueList = Array.from(new Map(list.map((f) => [f.id, f])).values());
        update((s) => ({ ...s, friends: uniqueList }));
      } catch (err: any) {
        console.warn('Failed to load friends', err);
      }
    },

    loadRequests: async () => {
      const user = get(currentUser);
      if (!user?.id) return;

      try {
        const list = await friendsService.getFriendRequests('incoming');
        update((s) => ({ ...s, requests: list }));
      } catch (err: any) {
        console.warn('Failed to load friend requests', err);
      }
    },

    sendRequest: async (usernameOrId: string) => {
      const trimmed = usernameOrId.trim();
      if (!trimmed) return false;

      const isGuid = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(trimmed);
      try {
        const res = await friendsService.sendFriendRequest(
          isGuid ? { receiverId: trimmed } : { receiverUsername: trimmed }
        );
        uiStore.addToast({
          title: 'Запит надіслано!',
          message: `Запит у друзі для '${res.receiverUsername || trimmed}' успішно надіслано.`,
          type: 'success'
        });
        return true;
      } catch (err: any) {
        uiStore.addToast({
          title: 'Помилка надсилання',
          message: err.message || 'Не вдалося надіслати запит у друзі',
          type: 'error'
        });
        return false;
      }
    },

    acceptRequest: async (requestId: string, username?: string) => {
      try {
        const res = await friendsService.acceptFriendRequest(requestId);
        update((s) => ({
          ...s,
          requests: s.requests.filter((r) => r.id !== requestId)
        }));

        const freshFriends = await friendsService.getFriends();
        update((s) => ({ ...s, friends: freshFriends }));

        uiStore.addToast({
          title: 'Запит прийнято!',
          message: res.message || `Ви тепер друзі з ${username || 'користувачем'}!`,
          type: 'success'
        });
        return true;
      } catch (err: any) {
        uiStore.addToast({
          title: 'Помилка',
          message: err.message || 'Не вдалося прийняти запит',
          type: 'error'
        });
        return false;
      }
    },

    rejectRequest: async (requestId: string, username?: string) => {
      try {
        const res = await friendsService.rejectFriendRequest(requestId);
        update((s) => ({
          ...s,
          requests: s.requests.filter((r) => r.id !== requestId)
        }));

        uiStore.addToast({
          title: 'Запит відхилено',
          message: res.message || `Запит від ${username || 'користувача'} відхилено.`,
          type: 'info'
        });
        return true;
      } catch (err: any) {
        uiStore.addToast({
          title: 'Помилка',
          message: err.message || 'Не вдалося відхилити запит',
          type: 'error'
        });
        return false;
      }
    },

    removeFriend: async (friendId: string, username?: string) => {
      try {
        const res = await friendsService.removeFriend(friendId);
        update((s) => ({
          ...s,
          friends: s.friends.filter((f) => f.id !== friendId)
        }));

        uiStore.addToast({
          title: 'Видалено з друзів',
          message: res.message || `${username || 'Користувача'} видалено зі списку друзів.`,
          type: 'info'
        });
        return true;
      } catch (err: any) {
        uiStore.addToast({
          title: 'Помилка',
          message: err.message || 'Не вдалося видалити з друзів',
          type: 'error'
        });
        return false;
      }
    },

    blockUser: async (friend: FriendDto) => {
      try {
        await friendsService.blockUser(friend.id);
        update((s) => ({
          ...s,
          friends: s.friends.filter((f) => f.id !== friend.id),
          blocked: [{ ...friend, status: UserStatus.Offline }, ...s.blocked.filter((b) => b.id !== friend.id)]
        }));

        uiStore.addToast({
          title: 'Користувача заблоковано',
          message: `Користувача '${friend.username}' заблоковано.`,
          type: 'warning'
        });
      } catch (err: any) {
        console.warn('Block error:', err);
        uiStore.addToast({
          title: 'Помилка',
          message: err.message || 'Не вдалося заблокувати користувача',
          type: 'error'
        });
      }
    },

    unblockUser: async (userId: string) => {
      try {
        await friendsService.unblockUser(userId);
        let unblockedUser: FriendDto | undefined;

        update((s) => {
          unblockedUser = s.blocked.find((b) => b.id.toLowerCase() === userId.toLowerCase());
          const newBlocked = s.blocked.filter((b) => b.id.toLowerCase() !== userId.toLowerCase());

          if (unblockedUser) {
            const restoredFriend: FriendDto = {
              ...unblockedUser,
              status: s.onlineUserIds.has(userId.toLowerCase()) ? UserStatus.Online : unblockedUser.status
            };
            return {
              ...s,
              blocked: newBlocked,
              friends: [restoredFriend, ...s.friends.filter((f) => f.id.toLowerCase() !== userId.toLowerCase())]
            };
          }

          return {
            ...s,
            blocked: newBlocked
          };
        });

        const freshFriends = await friendsService.getFriends().catch(() => null);
        if (freshFriends) {
          update((s) => ({
            ...s,
            friends: freshFriends.map((f) => ({
              ...f,
              status: s.onlineUserIds.has(f.id.toLowerCase()) ? UserStatus.Online : f.status
            }))
          }));
        }

        uiStore.addToast({
          title: 'Розблоковано',
          message: unblockedUser?.username 
            ? `Користувача '${unblockedUser.username}' розблоковано та повернено у список друзів!`
            : 'Користувача успішно розблоковано та повернено у список друзів!',
          type: 'success'
        });
      } catch (err: any) {
        uiStore.addToast({
          title: 'Помилка',
          message: err.message || 'Не вдалося розблокувати користувача',
          type: 'error'
        });
      }
    },

    setInitialOnlineFriends: (userIds: string[]) => {
      update((s) => {
        const lowerIds = new Set(userIds.map((id) => id.toLowerCase()));
        return {
          ...s,
          onlineUserIds: lowerIds,
          friends: s.friends.map((f) => ({
            ...f,
            status: lowerIds.has(f.id.toLowerCase()) ? UserStatus.Online : f.status
          }))
        };
      });
    },

    setUserOnline: (userId: string) => {
      const lower = userId.toLowerCase();
      update((s) => {
        const updatedSet = new Set(s.onlineUserIds);
        updatedSet.add(lower);
        return {
          ...s,
          onlineUserIds: updatedSet,
          friends: s.friends.map((f) => 
            f.id.toLowerCase() === lower 
              ? { ...f, status: UserStatus.Online }
              : f
          )
        };
      });
    },

    setUserOffline: (userId: string) => {
      const lower = userId.toLowerCase();
      update((s) => {
        const updatedSet = new Set(s.onlineUserIds);
        updatedSet.delete(lower);
        return {
          ...s,
          onlineUserIds: updatedSet,
          friends: s.friends.map((f) => 
            f.id.toLowerCase() === lower 
              ? { ...f, status: UserStatus.Offline }
              : f
          )
        };
      });
    }
  };
}

export const friendsStore = createFriendsStore();
