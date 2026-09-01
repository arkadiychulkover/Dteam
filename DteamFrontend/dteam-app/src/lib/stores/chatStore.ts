import { writable, get, derived } from 'svelte/store';
import { chatService } from '../services/chatService';
import { chatHubService } from '../services/chatHubService';
import { authStore, currentUser } from './authStore';
import type {
  ChatConversation,
  ChatMessage,
  ChatMediaSummary
} from '../types/chat';

export type RightSidebarTab = 'profile' | 'photos' | 'files' | 'voice';

interface ChatState {
  conversations: ChatConversation[];
  activeFriendId: string | null;
  messages: Record<string, ChatMessage[]>;
  cursors: Record<string, string | null>;
  hasMore: Record<string, boolean>;
  isLoadingConversations: boolean;
  isLoadingHistory: boolean;
  isLoadingMore: boolean;
  typingUsers: Record<string, boolean>;
  mediaSummary: ChatMediaSummary | null;
  rightSidebarTab: RightSidebarTab;
  searchQuery: string;
  currentPlayingVoiceId: string | null;
  notificationsEnabled: Record<string, boolean>;
}

const initialState: ChatState = {
  conversations: [],
  activeFriendId: null,
  messages: {},
  cursors: {},
  hasMore: {},
  isLoadingConversations: false,
  isLoadingHistory: false,
  isLoadingMore: false,
  typingUsers: {},
  mediaSummary: null,
  rightSidebarTab: 'profile',
  searchQuery: '',
  currentPlayingVoiceId: null,
  notificationsEnabled: {}
};

function createChatStore() {
  const { subscribe, update, set } = writable<ChatState>(initialState);

  // Helper: auto wire SignalR events
  let unsubs: Array<() => void> = [];

  function initRealtimeListeners() {
    unsubs.forEach(u => u());
    unsubs = [];

    unsubs.push(
      chatHubService.onReceiveMessage((msg) => {
        handleIncomingMessage(msg);
      }),
      chatHubService.onMessageRead((data) => {
        handleMessageRead(data.messageId, data.readerUserId, data.readAt);
      }),
      chatHubService.onUserTyping((data) => {
        handleUserTyping(data.userId, data.isTyping);
      }),
      chatHubService.onHistoryCleared((data) => {
        handleHistoryCleared(data.clearedWithUserId);
      }),
      chatHubService.onReconnected(() => {
        handleReconnected();
      })
    );
  }

  function handleIncomingMessage(msg: ChatMessage) {
    const user = get(currentUser);
    if (!user) return;

    const peerId = msg.senderId === user.id ? msg.receiverId : msg.senderId;

    update(state => {
      const currentList = state.messages[peerId] || [];

      // Deduplicate / replace optimistic message if clientMessageId matches
      const existingIdx = currentList.findIndex(
        m => m.id === msg.id || (m.clientMessageId && m.clientMessageId === msg.clientMessageId)
      );

      let updatedList: ChatMessage[];
      if (existingIdx !== -1) {
        updatedList = [...currentList];
        updatedList[existingIdx] = { ...msg, isOptimistic: false };
      } else {
        updatedList = [...currentList, msg];
      }

      // Update conversations list last message and unread count
      const convs = [...state.conversations];
      const convIdx = convs.findIndex(c => c.friendId === peerId);

      const isCurrentActive = state.activeFriendId === peerId;
      const isIncoming = msg.senderId !== user.id;

      if (convIdx !== -1) {
        convs[convIdx] = {
          ...convs[convIdx],
          lastMessage: msg,
          lastActivityAt: msg.createdAt,
          unreadCount: isCurrentActive ? 0 : (convs[convIdx].unreadCount + (isIncoming ? 1 : 0))
        };
      } else {
        convs.unshift({
          friendId: peerId,
          friendUsername: 'Співрозмовник',
          friendStatus: 1,
          lastMessage: msg,
          unreadCount: isCurrentActive ? 0 : (isIncoming ? 1 : 0),
          lastActivityAt: msg.createdAt
        });
      }

      // Sort conversations by unread and recent
      convs.sort((a, b) => {
        const timeA = a.lastActivityAt ? new Date(a.lastActivityAt).getTime() : 0;
        const timeB = b.lastActivityAt ? new Date(b.lastActivityAt).getTime() : 0;
        return timeB - timeA;
      });

      return {
        ...state,
        messages: {
          ...state.messages,
          [peerId]: updatedList
        },
        conversations: convs
      };
    });

    // If currently looking at this active conversation and message is from peer, mark read
    const state = get({ subscribe });
    if (state.activeFriendId === peerId && msg.senderId !== user.id) {
      chatService.markAsRead(msg.id).catch(() => {});
    }
  }

  function handleMessageRead(messageId: string, readerUserId: string, readAt: string) {
    update(state => {
      const newMessages: Record<string, ChatMessage[]> = {};
      for (const [friendId, msgs] of Object.entries(state.messages)) {
        newMessages[friendId] = msgs.map(m => {
          if (m.id === messageId || (m.receiverId === readerUserId && new Date(m.createdAt) <= new Date(readAt))) {
            return { ...m, status: 2, readAt };
          }
          return m;
        });
      }

      return {
        ...state,
        messages: newMessages
      };
    });
  }

  function handleUserTyping(userId: string, isTyping: boolean) {
    update(state => ({
      ...state,
      typingUsers: {
        ...state.typingUsers,
        [userId]: isTyping
      }
    }));
  }

  function handleHistoryCleared(clearedWithUserId: string) {
    update(state => ({
      ...state,
      messages: {
        ...state.messages,
        [clearedWithUserId]: []
      },
      cursors: {
        ...state.cursors,
        [clearedWithUserId]: null
      },
      hasMore: {
        ...state.hasMore,
        [clearedWithUserId]: false
      }
    }));
  }

  async function handleReconnected() {
    const state = get({ subscribe });
    if (state.activeFriendId) {
      const msgs = state.messages[state.activeFriendId] || [];
      if (msgs.length > 0) {
        const lastMsg = msgs[msgs.length - 1];
        try {
          const missed = await chatService.getMissedMessages(state.activeFriendId, lastMsg.createdAt);
          if (missed && missed.length > 0) {
            missed.forEach(m => handleIncomingMessage(m));
          }
        } catch (e) {
          console.warn('[ChatStore] Could not sync missed messages:', e);
        }
      }
    }
    loadConversations();
  }

  async function loadConversations() {
    update(s => ({ ...s, isLoadingConversations: true }));
    try {
      const convs = await chatService.getConversations();
      update(s => ({ ...s, conversations: convs, isLoadingConversations: false }));
    } catch (e) {
      console.warn('[ChatStore] Failed to load conversations:', e);
      update(s => ({ ...s, isLoadingConversations: false }));
    }
  }

  async function seedDemoConversations() {
    update(s => ({ ...s, isLoadingConversations: true }));
    try {
      const convs = await chatService.seedDemoConversations();
      update(s => ({ ...s, conversations: convs, isLoadingConversations: false }));
      if (convs.length > 0) {
        selectConversation(convs[0].friendId);
      }
    } catch (e) {
      console.warn('[ChatStore] Failed to seed demo conversations:', e);
      update(s => ({ ...s, isLoadingConversations: false }));
    }
  }

  async function selectConversation(friendId: string) {
    update(s => ({
      ...s,
      activeFriendId: friendId,
      rightSidebarTab: 'profile'
    }));

    loadHistory(friendId);
    loadMediaSummary(friendId);

    // Reset unread count locally for this conversation
    update(s => ({
      ...s,
      conversations: s.conversations.map(c =>
        c.friendId === friendId ? { ...c, unreadCount: 0 } : c
      )
    }));
  }

  async function loadHistory(friendId: string) {
    update(s => ({ ...s, isLoadingHistory: true }));
    try {
      const res = await chatService.getHistory(friendId, undefined, 50);
      update(s => ({
        ...s,
        messages: {
          ...s.messages,
          [friendId]: res.items
        },
        cursors: {
          ...s.cursors,
          [friendId]: res.nextCursor || null
        },
        hasMore: {
          ...s.hasMore,
          [friendId]: res.hasMore
        },
        isLoadingHistory: false
      }));

      // Mark unread messages as read
      const user = get(currentUser);
      if (user) {
        const unreadIncoming = res.items.filter(m => m.senderId === friendId && m.status !== 2);
        for (const m of unreadIncoming) {
          chatService.markAsRead(m.id).catch(() => {});
        }
      }
    } catch (e) {
      console.warn('[ChatStore] Failed to load history:', e);
      update(s => ({ ...s, isLoadingHistory: false }));
    }
  }

  async function loadMoreHistory(friendId: string) {
    const state = get({ subscribe });
    const cursor = state.cursors[friendId];
    if (!cursor || !state.hasMore[friendId] || state.isLoadingMore) {
      return;
    }

    update(s => ({ ...s, isLoadingMore: true }));
    try {
      const res = await chatService.getHistory(friendId, cursor, 50);
      update(s => {
        const existing = s.messages[friendId] || [];
        // Prepend older messages
        return {
          ...s,
          messages: {
            ...s.messages,
            [friendId]: [...res.items, ...existing]
          },
          cursors: {
            ...s.cursors,
            [friendId]: res.nextCursor || null
          },
          hasMore: {
            ...s.hasMore,
            [friendId]: res.hasMore
          },
          isLoadingMore: false
        };
      });
    } catch (e) {
      console.warn('[ChatStore] Failed to load more history:', e);
      update(s => ({ ...s, isLoadingMore: false }));
    }
  }

  async function sendMessage(
    content: string,
    type: 0 | 1 | 2 | 3 = 0,
    uploadId?: string,
    extraMeta?: Partial<ChatMessage>
  ) {
    const user = get(currentUser);
    const state = get({ subscribe });
    if (!user || !state.activeFriendId) return;

    const receiverId = state.activeFriendId;
    const clientMessageId = `cm_${Date.now()}_${Math.random().toString(36).substring(2, 9)}`;

    // Optimistic message
    const optimisticMsg: ChatMessage = {
      id: `opt_${clientMessageId}`,
      clientMessageId,
      senderId: user.id,
      receiverId,
      content: content?.trim() || null,
      type,
      status: 0,
      createdAt: new Date().toISOString(),
      isOptimistic: true,
      ...extraMeta
    };

    // Add optimistically to UI
    update(s => ({
      ...s,
      messages: {
        ...s.messages,
        [receiverId]: [...(s.messages[receiverId] || []), optimisticMsg]
      }
    }));

    try {
      const sent = await chatService.sendMessage({
        receiverId,
        clientMessageId,
        content: content?.trim(),
        type,
        uploadId
      });

      // Update state with confirmed server message
      update(s => {
        const msgs = (s.messages[receiverId] || []).map(m =>
          m.clientMessageId === clientMessageId ? { ...sent, isOptimistic: false } : m
        );
        return {
          ...s,
          messages: {
            ...s.messages,
            [receiverId]: msgs
          }
        };
      });

      loadMediaSummary(receiverId);
    } catch (e) {
      console.warn('[ChatStore] Failed to send message:', e);
      // Mark optimistic message as failed
      update(s => {
        const msgs = (s.messages[receiverId] || []).map(m =>
          m.clientMessageId === clientMessageId ? { ...m, isOptimistic: false, isFailed: true } : m
        );
        return {
          ...s,
          messages: {
            ...s.messages,
            [receiverId]: msgs
          }
        };
      });
    }
  }

  async function clearHistory(friendId: string) {
    try {
      await chatService.clearHistory(friendId);
      update(s => ({
        ...s,
        messages: {
          ...s.messages,
          [friendId]: []
        },
        conversations: s.conversations.map(c =>
          c.friendId === friendId ? { ...c, lastMessage: null, unreadCount: 0 } : c
        )
      }));
      loadMediaSummary(friendId);
    } catch (e) {
      console.warn('[ChatStore] Failed to clear history:', e);
    }
  }

  async function loadMediaSummary(friendId: string) {
    try {
      const summary = await chatService.getMediaSummary(friendId);
      update(s => ({ ...s, mediaSummary: summary }));
    } catch (e) {
      console.warn('[ChatStore] Failed to load media summary:', e);
    }
  }

  function setRightSidebarTab(tab: RightSidebarTab) {
    update(s => ({ ...s, rightSidebarTab: tab }));
  }

  function setSearchQuery(query: string) {
    update(s => ({ ...s, searchQuery: query }));
  }

  function setPlayingVoiceId(messageId: string | null) {
    update(s => ({ ...s, currentPlayingVoiceId: messageId }));
  }

  function toggleNotifications(friendId: string) {
    update(s => {
      const current = s.notificationsEnabled[friendId] ?? true;
      return {
        ...s,
        notificationsEnabled: {
          ...s.notificationsEnabled,
          [friendId]: !current
        }
      };
    });
  }

  initRealtimeListeners();

  return {
    subscribe,
    loadConversations,
    seedDemoConversations,
    selectConversation,
    loadHistory,
    loadMoreHistory,
    sendMessage,
    clearHistory,
    loadMediaSummary,
    setRightSidebarTab,
    setSearchQuery,
    setPlayingVoiceId,
    toggleNotifications
  };
}

export const chatStore = createChatStore();

// Derived total unread count for header badge
export const totalChatUnreadCount = derived(chatStore, ($c) => {
  return $c.conversations.reduce((sum, item) => sum + item.unreadCount, 0);
});
