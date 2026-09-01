import { api } from './api';
import type {
  ChatConversation,
  ChatMessage,
  ChatMediaSummary,
  ChatUploadResponse,
  CursorHistoryResponse
} from '../types/chat';

export const chatService = {
  async getConversations(): Promise<ChatConversation[]> {
    return api.get<ChatConversation[]>('/chat/conversations');
  },

  async getHistory(friendId: string, beforeCursor?: string, limit: number = 50): Promise<CursorHistoryResponse> {
    const params = new URLSearchParams();
    if (beforeCursor) params.append('before', beforeCursor);
    if (limit) params.append('limit', limit.toString());

    const query = params.toString() ? `?${params.toString()}` : '';
    return api.get<CursorHistoryResponse>(`/chat/history/${friendId}${query}`);
  },

  async getMissedMessages(friendId: string, afterTimestamp: string): Promise<ChatMessage[]> {
    return api.get<ChatMessage[]>(`/chat/sync/${friendId}?after=${encodeURIComponent(afterTimestamp)}`);
  },

  async sendMessage(payload: {
    receiverId: string;
    clientMessageId: string;
    content?: string;
    type?: number;
    uploadId?: string;
  }): Promise<ChatMessage> {
    return api.post<ChatMessage>('/chat/send', payload);
  },

  async markAsRead(messageId: string): Promise<{ success: boolean }> {
    return api.post<{ success: boolean }>(`/chat/messages/${messageId}/read`);
  },

  async clearHistory(friendId: string): Promise<{ success: boolean }> {
    return api.delete<{ success: boolean }>(`/chat/history/${friendId}`);
  },

  async getMediaSummary(friendId: string): Promise<ChatMediaSummary> {
    return api.get<ChatMediaSummary>(`/chat/media/${friendId}`);
  },

  async uploadAttachment(file: File | Blob, duration?: number, fileName?: string): Promise<ChatUploadResponse> {
    const formData = new FormData();
    if (file instanceof File) {
      formData.append('file', file);
    } else {
      formData.append('file', file, fileName || 'voice_message.webm');
    }
    if (duration !== undefined && duration !== null) {
      formData.append('duration', Math.round(duration).toString());
    }

    return api.post<ChatUploadResponse>('/chat/upload', formData);
  },

  async sendTyping(receiverId: string, isTyping: boolean): Promise<void> {
    await api.post('/chat/typing', { receiverId, isTyping });
  },

  async seedDemoConversations(): Promise<ChatConversation[]> {
    return api.post<ChatConversation[]>('/chat/seed-demo', {});
  }
};
