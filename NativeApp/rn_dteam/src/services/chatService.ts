import { Platform } from 'react-native';
import { api } from './api';
import { resolveMediaUrl } from '../utils/constants';
import type { Dialog, ChatMessage, UserStatus } from '../types';

export interface ChatUploadResponse {
  uploadId: string;
  originalFileName: string;
  contentType: string;
  fileSize: number;
  duration?: number | null;
  previewUrl: string;
}

export interface BackendChatMessage {
  id: string;
  clientMessageId: string;
  senderId: string;
  receiverId: string;
  content?: string | null;
  type: number; // 0=text, 1=file, 2=image, 3=voice
  status: number; // 0=sent, 1=delivered, 2=read
  mediaUrl?: string | null;
  originalFileName?: string | null;
  contentType?: string | null;
  fileSize?: number | null;
  duration?: number | null;
  createdAt: string;
  readAt?: string | null;
}

export interface BackendConversation {
  friendId: string;
  friendUsername: string;
  friendAvatarUrl?: string | null;
  friendStatus: number; // 0=offline, 1=online, 2=inGame
  lastMessage?: BackendChatMessage | null;
  unreadCount: number;
  lastActivityAt?: string | null;
}

export interface BackendFriend {
  id: string;
  username: string;
  email: string;
  avatarUrl?: string | null;
  bio?: string | null;
  status: number;
  lastLoginAt?: string | null;
  friendsSince?: string | null;
}

function formatMessageTime(isoString?: string | null): string {
  if (!isoString) return '';
  try {
    const date = new Date(isoString);
    const now = new Date();
    const isToday =
      date.getDate() === now.getDate() &&
      date.getMonth() === now.getMonth() &&
      date.getFullYear() === now.getFullYear();

    const hours = date.getHours().toString().padStart(2, '0');
    const minutes = date.getMinutes().toString().padStart(2, '0');

    if (isToday) {
      return `${hours}:${minutes}`;
    }
    const day = date.getDate().toString().padStart(2, '0');
    const month = (date.getMonth() + 1).toString().padStart(2, '0');
    return `${day}.${month}`;
  } catch {
    return '';
  }
}

export function mapBackendMessage(msg: BackendChatMessage, currentUserId?: string): ChatMessage {
  const isMine = currentUserId ? msg.senderId.toLowerCase() === currentUserId.toLowerCase() : false;
  let text = msg.content || '';
  if (!text) {
    if (msg.type === 3) text = '🎤 Голосове повідомлення';
    else if (msg.type === 2) text = '📷 Зображення';
    else if (msg.type === 1) text = `📎 ${msg.originalFileName || 'Файл'}`;
  }

  let mappedType: ChatMessage['type'] = 'text';
  if (msg.type === 3) mappedType = 'voice';
  else if (msg.type === 2) mappedType = 'image';
  else if (msg.type === 1) mappedType = 'file';

  return {
    id: msg.id,
    dialogId: isMine ? msg.receiverId : msg.senderId,
    senderId: msg.senderId,
    text,
    type: mappedType,
    timestamp: formatMessageTime(msg.createdAt),
    isMine,
    voiceDuration: msg.duration || undefined,
    voiceUri: resolveMediaUrl(msg.mediaUrl) || undefined,
    imageUrl: resolveMediaUrl(msg.mediaUrl) || undefined,
    fileName: msg.originalFileName || undefined,
    fileSize: msg.fileSize ? `${Math.round(msg.fileSize / 1024)} KB` : undefined,
    status: msg.status === 2 ? 'read' : msg.status === 1 ? 'delivered' : 'sent',
  };
}

export function mapBackendConversation(conv: BackendConversation, currentUserId?: string): Dialog {
  let lastMessageText = 'Немає повідомлень';
  if (conv.lastMessage) {
    if (conv.lastMessage.content) {
      lastMessageText = conv.lastMessage.content;
    } else if (conv.lastMessage.type === 3) {
      lastMessageText = '🎤 Голосове повідомлення';
    } else if (conv.lastMessage.type === 2) {
      lastMessageText = '📷 Зображення';
    } else if (conv.lastMessage.type === 1) {
      lastMessageText = `📎 ${conv.lastMessage.originalFileName || 'Файл'}`;
    }
  }

  return {
    id: conv.friendId,
    friendId: conv.friendId,
    friendUsername: conv.friendUsername,
    friendAvatarUrl: resolveMediaUrl(conv.friendAvatarUrl) || '',
    friendStatus: (conv.friendStatus as UserStatus) || 0,
    lastMessage: lastMessageText,
    lastMessageTime: formatMessageTime(conv.lastActivityAt || conv.lastMessage?.createdAt),
    unreadCount: conv.unreadCount || 0,
    messages: conv.lastMessage ? [mapBackendMessage(conv.lastMessage, currentUserId)] : [],
  };
}

export const chatService = {
  /**
   * Отримати список реальних діалогів користувача з бекенду
   */
  async getConversations(currentUserId?: string): Promise<Dialog[]> {
    try {
      const convs = await api.get<BackendConversation[]>('/chat/conversations');
      if (Array.isArray(convs)) {
        return convs.map((c) => mapBackendConversation(c, currentUserId));
      }
      return [];
    } catch (err) {
      console.warn('[chatService] Failed to load conversations:', err);
      return [];
    }
  },

  /**
   * Отримати історію реальних повідомлень для конкретного друга
   */
  async getHistory(friendId: string, currentUserId?: string, limit = 50): Promise<ChatMessage[]> {
    try {
      const res = await api.get<{ items: BackendChatMessage[] }>(`/chat/history/${friendId}?limit=${limit}`);
      if (res && Array.isArray(res.items)) {
        // Сервер повертає від нових до старих або хронологічно, сортуємо хронологічно
        const sorted = [...res.items].sort(
          (a, b) => new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime()
        );
        return sorted.map((m) => mapBackendMessage(m, currentUserId));
      }
      return [];
    } catch (err) {
      console.warn(`[chatService] Failed to load history for ${friendId}:`, err);
      return [];
    }
  },

  /**
   * Відправити текстове повідомлення на сервер
   */
  async sendMessage(receiverId: string, content: string, currentUserId?: string): Promise<ChatMessage> {
    const clientMessageId = `cm_${Date.now()}_${Math.random().toString(36).substring(2, 7)}`;
    const payload = {
      receiverId,
      clientMessageId,
      content: content.trim(),
      type: 0, // 0 = Text
    };

    const res = await api.post<BackendChatMessage>('/chat/send', payload);
    return mapBackendMessage(res, currentUserId);
  },

  /**
   * Завантажити медіа/файл/голосове повідомлення на сервер
   */
  async uploadAttachment(
    fileUri: string,
    duration?: number,
    fileName?: string,
    mimeType?: string
  ): Promise<ChatUploadResponse> {
    const formData = new FormData();

    if (Platform.OS === 'web') {
      let fileToSend: any;
      if (fileUri.startsWith('blob:') || fileUri.startsWith('data:')) {
        const response = await fetch(fileUri);
        const blob = await response.blob();
        const finalName = fileName || (duration ? 'voice.webm' : 'file.bin');
        fileToSend = new File([blob], finalName, {
          type: mimeType || blob.type || (duration ? 'audio/webm' : 'application/octet-stream'),
        });
      } else {
        const response = await fetch(fileUri);
        const blob = await response.blob();
        fileToSend = new File([blob], fileName || 'upload.bin', {
          type: mimeType || blob.type || 'application/octet-stream',
        });
      }
      formData.append('file', fileToSend);
    } else {
      const finalName = fileName || (duration ? 'voice.m4a' : 'attachment.bin');
      const finalType = mimeType || (duration ? 'audio/m4a' : 'application/octet-stream');
      formData.append('file', {
        uri: fileUri,
        name: finalName,
        type: finalType,
      } as any);
    }

    if (duration != null && duration > 0) {
      formData.append('duration', Math.round(duration).toString());
    }

    return await api.post<ChatUploadResponse>('/chat/upload', formData);
  },

  /**
   * Завантажити та відправити голосове повідомлення
   */
  async sendVoiceMessage(
    receiverId: string,
    voiceUri: string,
    duration?: number,
    currentUserId?: string
  ): Promise<ChatMessage> {
    const upload = await this.uploadAttachment(voiceUri, duration, 'voice.webm', 'audio/webm');
    const clientMessageId = `cm_voice_${Date.now()}_${Math.random().toString(36).substring(2, 7)}`;
    const payload = {
      receiverId,
      clientMessageId,
      content: 'Голосове повідомлення',
      type: 3, // 3 = Voice
      uploadId: upload.uploadId,
    };

    const res = await api.post<BackendChatMessage>('/chat/send', payload);
    return mapBackendMessage(res, currentUserId);
  },

  /**
   * Завантажити та відправити зображення
   */
  async sendImageMessage(
    receiverId: string,
    imageUri: string,
    caption?: string,
    currentUserId?: string
  ): Promise<ChatMessage> {
    const upload = await this.uploadAttachment(imageUri, undefined, 'image.jpg', 'image/jpeg');
    const clientMessageId = `cm_img_${Date.now()}_${Math.random().toString(36).substring(2, 7)}`;
    const payload = {
      receiverId,
      clientMessageId,
      content: caption || '',
      type: 2, // 2 = Image
      uploadId: upload.uploadId,
    };

    const res = await api.post<BackendChatMessage>('/chat/send', payload);
    return mapBackendMessage(res, currentUserId);
  },

  /**
   * Завантажити та відправити файл
   */
  async sendFileMessage(
    receiverId: string,
    fileUri: string,
    fileName: string,
    currentUserId?: string
  ): Promise<ChatMessage> {
    const upload = await this.uploadAttachment(fileUri, undefined, fileName);
    const clientMessageId = `cm_file_${Date.now()}_${Math.random().toString(36).substring(2, 7)}`;
    const payload = {
      receiverId,
      clientMessageId,
      content: fileName,
      type: 1, // 1 = File
      uploadId: upload.uploadId,
    };

    const res = await api.post<BackendChatMessage>('/chat/send', payload);
    return mapBackendMessage(res, currentUserId);
  },

  /**
   * Позначити повідомлення як прочитане
   */
  async markAsRead(messageId: string): Promise<void> {
    try {
      await api.post(`/chat/messages/${messageId}/read`);
    } catch {
      // ігноруємо некритичні помилки синхронізації
    }
  },

  /**
   * Отримати список друзів з сервера
   */
  async getFriends(): Promise<BackendFriend[]> {
    try {
      const list = await api.get<BackendFriend[]>('/friends/list');
      return Array.isArray(list) ? list : [];
    } catch (err) {
      console.warn('[chatService] Failed to load friends list:', err);
      return [];
    }
  },
};
