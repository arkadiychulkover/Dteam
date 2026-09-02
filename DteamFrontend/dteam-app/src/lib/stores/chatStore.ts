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
      chatHubService.onMessageDeleted((data) => {
        handleMessageDeleted(data.messageId, data.peerId);
      }),
      chatHubService.onReconnected(() => {
        handleReconnected();
      })
    );
  }

  function handleIncomingMessage(msg: ChatMessage) {
    const user = get(currentUser);
    if (!user) return;

    const myId = user.id.toLowerCase();
    const senderId = msg.senderId.toLowerCase();
    const receiverId = msg.receiverId.toLowerCase();
    const isMine = senderId === myId;
    const peerId = isMine ? receiverId : senderId;

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
      const convIdx = convs.findIndex(c => c.friendId.toLowerCase() === peerId);

      const isCurrentActive = state.activeFriendId?.toLowerCase() === peerId;
      const isIncoming = !isMine;

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
    if (state.activeFriendId?.toLowerCase() === peerId && !isMine) {
      chatService.markAsRead(msg.id).catch(() => {});
    }
  }

  function handleMessageRead(messageId: string, readerUserId: string, readAt: string) {
    const readerId = readerUserId.toLowerCase();
    update(state => {
      const newMessages: Record<string, ChatMessage[]> = {};
      for (const [friendId, msgs] of Object.entries(state.messages)) {
        newMessages[friendId] = msgs.map(m => {
          if (m.id === messageId || (m.receiverId.toLowerCase() === readerId && new Date(m.createdAt) <= new Date(readAt))) {
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
    const id = userId.toLowerCase();
    update(state => ({
      ...state,
      typingUsers: {
        ...state.typingUsers,
        [id]: isTyping
      }
    }));
  }

  function handleHistoryCleared(clearedWithUserId: string) {
    const peerId = clearedWithUserId.toLowerCase();
    update(state => ({
      ...state,
      messages: {
        ...state.messages,
        [peerId]: []
      },
      conversations: state.conversations.map(c =>
        c.friendId.toLowerCase() === peerId ? { ...c, lastMessage: null, unreadCount: 0 } : c
      ),
      cursors: {
        ...state.cursors,
        [peerId]: null
      },
      hasMore: {
        ...state.hasMore,
        [peerId]: false
      },
      mediaSummary: state.activeFriendId?.toLowerCase() === peerId ? null : state.mediaSummary
    }));
  }

  function filterMediaGroups(groups: ChatMonthGroup[] | undefined, messageId: string): ChatMonthGroup[] {
    if (!groups) return [];
    return groups
      .map(g => ({
        ...g,
        items: g.items.filter(item => item.messageId !== messageId)
      }))
      .filter(g => g.items.length > 0);
  }

  function handleMessageDeleted(messageId: string, peerId: string) {
    const id = peerId.toLowerCase();
    update(state => {
      const msgs = state.messages[id] || [];
      const filtered = msgs.filter(m => m.id !== messageId);
      const lastMsg = filtered.length > 0 ? filtered[filtered.length - 1] : null;

      let updatedSummary = state.mediaSummary;
      if (updatedSummary) {
        const photos = filterMediaGroups(updatedSummary.photosByMonth, messageId);
        const files = filterMediaGroups(updatedSummary.filesByMonth, messageId);
        const voice = filterMediaGroups(updatedSummary.voiceByMonth, messageId);
        updatedSummary = {
          ...updatedSummary,
          photosByMonth: photos,
          filesByMonth: files,
          voiceByMonth: voice,
          totalPhotos: photos.reduce((acc, g) => acc + g.items.length, 0),
          totalFiles: files.reduce((acc, g) => acc + g.items.length, 0),
          totalVoiceMessages: voice.reduce((acc, g) => acc + g.items.length, 0)
        };
      }

      return {
        ...state,
        messages: {
          ...state.messages,
          [id]: filtered
        },
        conversations: state.conversations.map(c =>
          c.friendId.toLowerCase() === id ? { ...c, lastMessage: lastMsg } : c
        ),
        mediaSummary: updatedSummary,
        currentPlayingVoiceId: state.currentPlayingVoiceId === messageId ? null : state.currentPlayingVoiceId
      };
    });

    loadMediaSummary(id);
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
    const id = friendId.toLowerCase();
    update(s => ({
      ...s,
      activeFriendId: id,
      rightSidebarTab: 'profile'
    }));

    loadHistory(id);
    loadMediaSummary(id);

    // Reset unread count locally for this conversation
    update(s => ({
      ...s,
      conversations: s.conversations.map(c =>
        c.friendId.toLowerCase() === id ? { ...c, unreadCount: 0 } : c
      )
    }));
  }

  async function loadHistory(friendId: string) {
    const id = friendId.toLowerCase();
    update(s => ({ ...s, isLoadingHistory: true }));
    try {
      const res = await chatService.getHistory(friendId, undefined, 50);
      update(s => ({
        ...s,
        messages: {
          ...s.messages,
          [id]: res.items
        },
        cursors: {
          ...s.cursors,
          [id]: res.nextCursor || null
        },
        hasMore: {
          ...s.hasMore,
          [id]: res.hasMore
        },
        isLoadingHistory: false
      }));

      // Mark unread messages as read
      const user = get(currentUser);
      if (user) {
        const unreadIncoming = res.items.filter(m => m.senderId.toLowerCase() === id && m.status !== 2);
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
    const id = friendId.toLowerCase();
    const state = get({ subscribe });
    const cursor = state.cursors[id];
    if (!cursor || !state.hasMore[id] || state.isLoadingMore) {
      return;
    }

    update(s => ({ ...s, isLoadingMore: true }));
    try {
      const res = await chatService.getHistory(friendId, cursor, 50);
      update(s => {
        const existing = s.messages[id] || [];
        // Prepend older messages
        return {
          ...s,
          messages: {
            ...s.messages,
            [id]: [...res.items, ...existing]
          },
          cursors: {
            ...s.cursors,
            [id]: res.nextCursor || null
          },
          hasMore: {
            ...s.hasMore,
            [id]: res.hasMore
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

    const rawReceiverId = state.activeFriendId;
    const receiverId = rawReceiverId.toLowerCase();
    const clientMessageId = `cm_${Date.now()}_${Math.random().toString(36).substring(2, 9)}`;

    // Optimistic message
    const optimisticMsg: ChatMessage = {
      id: `opt_${clientMessageId}`,
      clientMessageId,
      senderId: user.id,
      receiverId: rawReceiverId,
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
        receiverId: rawReceiverId,
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
    const id = friendId.toLowerCase();
    try {
      await chatService.clearHistory(friendId);
      update(s => ({
        ...s,
        messages: {
          ...s.messages,
          [id]: []
        },
        conversations: s.conversations.map(c =>
          c.friendId.toLowerCase() === id ? { ...c, lastMessage: null, unreadCount: 0 } : c
        ),
        cursors: {
          ...s.cursors,
          [id]: null
        },
        hasMore: {
          ...s.hasMore,
          [id]: false
        }
      }));
      loadMediaSummary(id);
    } catch (e) {
      console.warn('[ChatStore] Failed to clear history:', e);
    }
  }

  async function deleteMessage(messageId: string, peerId: string) {
    try {
      await chatService.deleteMessage(messageId);
      handleMessageDeleted(messageId, peerId);
      loadMediaSummary(peerId);
    } catch (e) {
      console.warn('[ChatStore] Failed to delete message:', e);
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
    deleteMessage,
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
