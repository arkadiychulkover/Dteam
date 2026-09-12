export type ChatMessageType = 0 | 1 | 2 | 3;
export type MessageDeliveryStatus = 0 | 1 | 2;

export interface ChatMessage {
  id: string;
  clientMessageId: string;
  senderId: string;
  receiverId: string;
  content?: string | null;
  type: ChatMessageType;
  status: MessageDeliveryStatus;
  mediaUrl?: string | null;
  originalFileName?: string | null;
  contentType?: string | null;
  fileSize?: number | null;
  duration?: number | null;
  createdAt: string;
  readAt?: string | null;

  isOptimistic?: boolean;
  isFailed?: boolean;
}

export interface ChatConversation {
  friendId: string;
  friendUsername: string;
  friendAvatarUrl?: string | null;
  friendStatus: number;
  lastMessage?: ChatMessage | null;
  unreadCount: number;
  lastActivityAt?: string | null;
}

export interface ChatUploadResponse {
  uploadId: string;
  originalFileName: string;
  contentType: string;
  fileSize: number;
  duration?: number | null;
  previewUrl: string;
}

export interface ChatMediaItem {
  messageId: string;
  senderId: string;
  senderUsername: string;
  mediaUrl: string;
  originalFileName: string;
  contentType: string;
  fileSize: number;
  duration?: number | null;
  createdAt: string;
}

export interface ChatMonthGroup {
  monthLabel: string;
  year: number;
  month: number;
  items: ChatMediaItem[];
}

export interface ChatMediaSummary {
  totalPhotos: number;
  totalFiles: number;
  totalVoiceMessages: number;
  photosByMonth: ChatMonthGroup[];
  filesByMonth: ChatMonthGroup[];
  voiceByMonth: ChatMonthGroup[];
}

export interface CursorHistoryResponse {
  items: ChatMessage[];
  nextCursor?: string | null;
  hasMore: boolean;
  totalCount: number;
}
