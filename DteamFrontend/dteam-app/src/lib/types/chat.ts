export type ChatMessageType = 0 | 1 | 2 | 3; // 0=Text, 1=Image, 2=File, 3=Voice
export type MessageDeliveryStatus = 0 | 1 | 2; // 0=Sent, 1=Delivered, 2=Read

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
  // Local UI-only state
  isOptimistic?: boolean;
  isFailed?: boolean;
}

export interface ChatConversation {
  friendId: string;
  friendUsername: string;
  friendAvatarUrl?: string | null;
  friendStatus: number; // 0=Offline, 1=Online, 2=InGame, 3=Away, 4=Busy
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
