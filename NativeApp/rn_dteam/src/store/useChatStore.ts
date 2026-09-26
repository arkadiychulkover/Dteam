import { create } from 'zustand';
import type { Dialog, ChatMessage } from '../types';
import {
  chatService,
  mapBackendMessage,
  mapBackendConversation,
  BackendChatMessage,
} from '../services/chatService';
import { chatHubService } from '../services/chatHubService';
import { friendsHubService } from '../services/friendsHubService';
import { useAuthStore } from './useAuthStore';

interface ChatStoreState {
  conversations: Dialog[];
  activePeerId: string | null;
  messages: Record<string, ChatMessage[]>;
  isLoadingConversations: boolean;
  isLoadingHistory: Record<string, boolean>;
  unreadTotal: number;
  typingUsers: Record<string, boolean>;

  setActivePeerId: (peerId: string | null) => void;
  loadConversations: () => Promise<void>;
  loadHistory: (friendId: string) => Promise<ChatMessage[]>;
  sendMessage: (friendId: string, content: string) => Promise<ChatMessage>;
  sendVoiceMessage: (friendId: string, voiceUri: string, duration?: number) => Promise<ChatMessage>;
  sendImageMessage: (friendId: string, imageUri: string, caption?: string) => Promise<ChatMessage>;
  sendFileMessage: (friendId: string, fileUri: string, fileName: string) => Promise<ChatMessage>;
  sendOptimisticMessage: (msg: ChatMessage) => void;
  markAsRead: (friendId: string) => Promise<void>;
  handleIncomingMessage: (msg: BackendChatMessage) => void;
  handleMessageRead: (data: { messageId: string; readerUserId: string; readAt: string }) => void;
  handleUserTyping: (data: { userId: string; isTyping: boolean }) => void;
  startRealtime: () => Promise<void>;
  stopRealtime: () => Promise<void>;
}

let isRealtimeStarted = false;

export const useChatStore = create<ChatStoreState>((set, get) => ({
  conversations: [],
  activePeerId: null,
  messages: {},
  isLoadingConversations: false,
  isLoadingHistory: {},
  unreadTotal: 0,
  typingUsers: {},

  setActivePeerId: (peerId: string | null) => {
    const normalized = peerId ? peerId.toLowerCase() : null;
    if (get().activePeerId === normalized) return;
    set({ activePeerId: normalized });
    if (normalized) {
      get().markAsRead(normalized);
    }
  },

  loadConversations: async () => {
    const user = useAuthStore.getState().user;
    if (!user) return;

    set({ isLoadingConversations: true });
    try {
      const convs = await chatService.getConversations(user.id);
      const totalUnread = convs.reduce((sum, c) => sum + (c.unreadCount || 0), 0);
      set({
        conversations: convs,
        unreadTotal: totalUnread,
        isLoadingConversations: false,
      });
    } catch (err) {
      console.warn('[useChatStore] Error loading conversations:', err);
      set({ isLoadingConversations: false });
    }
  },

  loadHistory: async (friendId: string) => {
    const user = useAuthStore.getState().user;
    const peerKey = friendId.toLowerCase();

    set((state) => ({
      isLoadingHistory: { ...state.isLoadingHistory, [peerKey]: true },
    }));

    try {
      const msgs = await chatService.getHistory(friendId, user?.id);
      set((state) => ({
        messages: { ...state.messages, [peerKey]: msgs },
        isLoadingHistory: { ...state.isLoadingHistory, [peerKey]: false },
      }));

      // Mark unread messages as read
      get().markAsRead(friendId);
      return msgs;
    } catch (err) {
      console.warn(`[useChatStore] Failed to load history for ${friendId}:`, err);
      set((state) => ({
        isLoadingHistory: { ...state.isLoadingHistory, [peerKey]: false },
      }));
      return [];
    }
  },

  sendOptimisticMessage: (msg: ChatMessage) => {
    const peerKey = msg.dialogId.toLowerCase();
    set((state) => {
      const currentList = state.messages[peerKey] || [];
      return {
        messages: {
          ...state.messages,
          [peerKey]: [...currentList, msg],
        },
      };
    });
  },

  sendMessage: async (friendId: string, content: string) => {
    const user = useAuthStore.getState().user;
    const peerKey = friendId.toLowerCase();

    try {
      const savedMsg = await chatService.sendMessage(friendId, content, user?.id);

      // Оновлюємо список повідомлень у сторі
      set((state) => {
        const currentList = state.messages[peerKey] || [];
        // Якщо було оптимістичне повідомлення, оновлюємо його або додаємо
        const filtered = currentList.filter(
          (m) => !m.id.startsWith('temp-') || m.text !== savedMsg.text
        );
        return {
          messages: {
            ...state.messages,
            [peerKey]: [...filtered, savedMsg],
          },
        };
      });

      // Оновлюємо останнє повідомлення в діалогах
      set((state) => {
        const updatedConvs = state.conversations.map((c) => {
          if (c.friendId.toLowerCase() === peerKey) {
            return {
              ...c,
              lastMessage: savedMsg.text,
              lastMessageTime: savedMsg.timestamp,
            };
          }
          return c;
        });
        return { conversations: updatedConvs };
      });

      return savedMsg;
    } catch (err) {
      console.warn('[useChatStore] Error sending message:', err);
      throw err;
    }
  },

  sendVoiceMessage: async (friendId: string, voiceUri: string, duration?: number) => {
    const user = useAuthStore.getState().user;
    const peerKey = friendId.toLowerCase();

    try {
      const savedMsg = await chatService.sendVoiceMessage(friendId, voiceUri, duration, user?.id);

      set((state) => {
        const currentList = state.messages[peerKey] || [];
        const filtered = currentList.filter(
          (m) => !m.id.startsWith('temp-voice-') && m.id !== savedMsg.id
        );
        return {
          messages: {
            ...state.messages,
            [peerKey]: [...filtered, savedMsg],
          },
        };
      });

      set((state) => {
        const updatedConvs = state.conversations.map((c) => {
          if (c.friendId.toLowerCase() === peerKey) {
            return {
              ...c,
              lastMessage: '🎤 Голосове повідомлення',
              lastMessageTime: savedMsg.timestamp,
            };
          }
          return c;
        });
        return { conversations: updatedConvs };
      });

      return savedMsg;
    } catch (err) {
      console.warn('[useChatStore] Error sending voice message:', err);
      throw err;
    }
  },

  sendImageMessage: async (friendId: string, imageUri: string, caption?: string) => {
    const user = useAuthStore.getState().user;
    const peerKey = friendId.toLowerCase();

    try {
      const savedMsg = await chatService.sendImageMessage(friendId, imageUri, caption, user?.id);

      set((state) => {
        const currentList = state.messages[peerKey] || [];
        const filtered = currentList.filter(
          (m) => !m.id.startsWith('temp-img-') && m.id !== savedMsg.id
        );
        return {
          messages: {
            ...state.messages,
            [peerKey]: [...filtered, savedMsg],
          },
        };
      });

      set((state) => {
        const updatedConvs = state.conversations.map((c) => {
          if (c.friendId.toLowerCase() === peerKey) {
            return {
              ...c,
              lastMessage: '📷 Зображення',
              lastMessageTime: savedMsg.timestamp,
            };
          }
          return c;
        });
        return { conversations: updatedConvs };
      });

      return savedMsg;
    } catch (err) {
      console.warn('[useChatStore] Error sending image message:', err);
      throw err;
    }
  },

  sendFileMessage: async (friendId: string, fileUri: string, fileName: string) => {
    const user = useAuthStore.getState().user;
    const peerKey = friendId.toLowerCase();

    try {
      const savedMsg = await chatService.sendFileMessage(friendId, fileUri, fileName, user?.id);

      set((state) => {
        const currentList = state.messages[peerKey] || [];
        const filtered = currentList.filter(
          (m) => !m.id.startsWith('temp-file-') && m.id !== savedMsg.id
        );
        return {
          messages: {
            ...state.messages,
            [peerKey]: [...filtered, savedMsg],
          },
        };
      });

      set((state) => {
        const updatedConvs = state.conversations.map((c) => {
          if (c.friendId.toLowerCase() === peerKey) {
            return {
              ...c,
              lastMessage: `📎 ${fileName}`,
              lastMessageTime: savedMsg.timestamp,
            };
          }
          return c;
        });
        return { conversations: updatedConvs };
      });

      return savedMsg;
    } catch (err) {
      console.warn('[useChatStore] Error sending file message:', err);
      throw err;
    }
  },

  markAsRead: async (friendId: string) => {
    const peerKey = friendId.toLowerCase();
    const currentMessages = get().messages[peerKey] || [];

    const unread = currentMessages.filter((m) => !m.isMine && m.status !== 'read');
    const conv = get().conversations.find((c) => c.friendId.toLowerCase() === peerKey);
    const hasUnreadCounter = conv && (conv.unreadCount ?? 0) > 0;

    if (unread.length === 0 && !hasUnreadCounter) {
      return;
    }

    for (const m of unread) {
      chatService.markAsRead(m.id).catch(() => {});
    }

    // Оновлюємо статус повідомлень на 'read'
    set((state) => {
      const msgs = state.messages[peerKey];
      const updated = msgs
        ? msgs.map((m) => (!m.isMine && m.status !== 'read' ? { ...m, status: 'read' as const } : m))
        : [];
      const convs = state.conversations.map((c) => {
        if (c.friendId.toLowerCase() === peerKey) {
          return { ...c, unreadCount: 0 };
        }
        return c;
      });

      const totalUnread = convs.reduce((sum, c) => sum + (c.unreadCount || 0), 0);

      return {
        messages: msgs ? { ...state.messages, [peerKey]: updated } : state.messages,
        conversations: convs,
        unreadTotal: totalUnread,
      };
    });
  },

  handleIncomingMessage: (msg: BackendChatMessage) => {
    const currentUserId = useAuthStore.getState().user?.id;
    if (!currentUserId) return;

    const myId = currentUserId.toLowerCase();
    const senderId = (msg.senderId || '').toLowerCase();
    const receiverId = (msg.receiverId || '').toLowerCase();
    const isMine = senderId === myId;
    const peerKey = isMine ? receiverId : senderId;

    if (!peerKey) return;

    const mapped = mapBackendMessage(msg, currentUserId);
    const activePeer = get().activePeerId;
    const isChatActive = activePeer === peerKey;

    console.log('[useChatStore] ⚡ handleIncomingMessage for peer:', peerKey, 'isChatActive:', isChatActive);

    // 1. Оновлюємо повідомлення активного діалогу
    set((state) => {
      const currentList = state.messages[peerKey] || [];

      // Перевіряємо чи повідомлення вже є (за id або за clientMessageId)
      const existingIdx = currentList.findIndex(
        (m) =>
          m.id === mapped.id ||
          (msg.clientMessageId && m.id === msg.clientMessageId) ||
          (m.id.startsWith('temp-') && m.text === mapped.text)
      );

      let nextList: ChatMessage[];
      if (existingIdx !== -1) {
        nextList = [...currentList];
        nextList[existingIdx] = mapped;
      } else {
        nextList = [...currentList, mapped];
      }

      return {
        messages: {
          ...state.messages,
          [peerKey]: nextList,
        },
      };
    });

    // 2. Якщо користувач прямо зараз дивиться цей чат, автоматично позначаємо прочитаним
    if (isChatActive && !isMine) {
      chatService.markAsRead(msg.id).catch(() => {});
    }

    // 3. Оновлюємо список діалогів
    set((state) => {
      const existingConvIdx = state.conversations.findIndex(
        (c) => c.friendId.toLowerCase() === peerKey
      );

      if (existingConvIdx !== -1) {
        const existingConv = state.conversations[existingConvIdx];
        const unreadCount =
          isChatActive || isMine ? 0 : (existingConv.unreadCount || 0) + 1;

        const updatedConv: Dialog = {
          ...existingConv,
          lastMessage: mapped.text,
          lastMessageTime: mapped.timestamp,
          unreadCount,
          messages: [mapped],
        };

        const rest = state.conversations.filter(
          (c) => c.friendId.toLowerCase() !== peerKey
        );
        const nextConvs = [updatedConv, ...rest];
        const totalUnread = nextConvs.reduce((sum, c) => sum + (c.unreadCount || 0), 0);

        return {
          conversations: nextConvs,
          unreadTotal: totalUnread,
        };
      } else {
        // Якщо діалог ще не існував у списку, оновлюємо весь список
        get().loadConversations();
        return state;
      }
    });
  },

  handleMessageRead: (data) => {
    const currentUserId = useAuthStore.getState().user?.id;
    if (!currentUserId) return;

    set((state) => {
      let updated = false;
      const newMessages: Record<string, ChatMessage[]> = {};

      Object.keys(state.messages).forEach((peerKey) => {
        const list = state.messages[peerKey];
        const hasMsg = list.some((m) => m.id === data.messageId);
        if (hasMsg) {
          updated = true;
          newMessages[peerKey] = list.map((m) =>
            m.id === data.messageId ? { ...m, status: 'read' as const } : m
          );
        } else {
          newMessages[peerKey] = list;
        }
      });

      if (!updated) return state;
      return { messages: newMessages };
    });
  },

  handleUserTyping: (data) => {
    const peerKey = data.userId.toLowerCase();
    set((state) => ({
      typingUsers: {
        ...state.typingUsers,
        [peerKey]: data.isTyping,
      },
    }));
  },

  startRealtime: async () => {
    if (isRealtimeStarted) {
      return;
    }
    isRealtimeStarted = true;
    console.log('[useChatStore] 🚀 Starting real-time ChatHub and FriendsHub listeners...');

    chatHubService.clearHandlers();

    // Підписуємося на події ChatHub
    chatHubService.onReceiveMessage((msg) => {
      get().handleIncomingMessage(msg);
    });

    chatHubService.onMessageRead((data) => {
      get().handleMessageRead(data);
    });

    chatHubService.onUserTyping((data) => {
      get().handleUserTyping(data);
    });

    chatHubService.onReconnected(() => {
      console.log('[useChatStore] 🔄 Reconnected, refreshing conversations...');
      get().loadConversations();
    });

    // Запускаємо підключення
    await Promise.all([
      chatHubService.start(),
      friendsHubService.start(),
    ]);
  },

  stopRealtime: async () => {
    isRealtimeStarted = false;
    console.log('[useChatStore] ⏹️ Stopping real-time hubs...');
    await Promise.all([
      chatHubService.stop(),
      friendsHubService.stop(),
    ]);
  },
}));
