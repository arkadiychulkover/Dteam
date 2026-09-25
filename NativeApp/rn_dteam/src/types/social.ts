export type TabId = 'chats' | 'profile' | 'settings' | 'library' | 'store';

export type UserStatus = 0 | 1 | 2; // 0 = offline, 1 = online, 2 = inGame

export interface Badge {
  id: string;
  title: string;
  description: string;
  imageUrl?: string;
  iconName: string;
  rarity: 'common' | 'rare' | 'epic' | 'legendary' | 'mythic';
  level: number;
  earnedDate: string;
  contractAddress?: string;
  tokenId?: string;
}

export interface ActivityItem {
  id: string;
  type: 'achievement' | 'game_purchased' | 'review' | 'nft_minted';
  title: string;
  subtitle: string;
  timestamp: string;
  gameTitle?: string;
  gameCover?: string;
}

export interface UserProfile {
  id: string;
  username: string;
  displayName: string;
  email: string;
  avatarUrl: string;
  bio: string;
  status: UserStatus;
  gameStatus?: string;
  level: number;
  currentXp: number;
  nextLevelXp: number;
  tokensBalance: number; // DTP
  balanceUah: number;
  walletAddress: string;
  stats: {
    gamesCount: number;
    badgesCount: number;
    reviewsCount: number;
    friendsCount: number;
  };
  badges: Badge[];
  recentActivity: ActivityItem[];
}

export type MessageType = 'text' | 'voice' | 'image' | 'file';

export interface ChatMessage {
  id: string;
  dialogId: string;
  senderId: string;
  text: string;
  type: MessageType;
  timestamp: string;
  isMine: boolean;
  voiceDuration?: number; // seconds
  voiceUri?: string;
  imageUrl?: string;
  fileName?: string;
  fileSize?: string;
  status?: 'sent' | 'delivered' | 'read';
}

export interface Dialog {
  id: string;
  friendId: string;
  friendUsername: string;
  friendAvatarUrl: string;
  friendStatus: UserStatus;
  gameStatus?: string;
  lastMessage: string;
  lastMessageTime: string;
  unreadCount: number;
  messages: ChatMessage[];
}

export interface Transaction {
  id: string;
  type: 'deposit' | 'purchase' | 'cashback' | 'reward';
  title: string;
  date: string;
  amount: string;
  isPositive: boolean;
  status: 'completed' | 'pending';
}

export interface SettingsState {
  displayName: string;
  bio: string;
  language: string;
  timezone: string;
  country: string;
  notifications: {
    chatMessages: boolean;
    friendRequests: boolean;
    wishlistDiscounts: boolean;
    gameUpdates: boolean;
    emailDigest: boolean;
  };
}
