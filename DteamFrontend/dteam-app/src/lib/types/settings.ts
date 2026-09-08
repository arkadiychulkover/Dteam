export interface UserProfileSettings {
  id: string;
  username: string;
  email: string;
  bio: string | null;
  avatarUrl: string | null;
  bannerUrl: string | null;
  preferredLanguage: string;
  createdAt: string;
}

export interface NotificationPreferences {
  notifyBigSales: boolean;
  notifyWishlistDiscounts: boolean;
  notifyProfileComments: boolean;
  notifyFriendRequests: boolean;
  notifyFriendRequestAccepted: boolean;
  notifyFriendRequestDeclined: boolean;
  chatNotificationsEnabled: boolean;
  chatSoundEnabled: boolean;
}

export interface WalletSummary {
  balanceInNanoTons: number;
  formattedBalance: number;
  currency: string;
}

export interface SettingsResponse {
  profile: UserProfileSettings;
  preferences: NotificationPreferences;
  walletSummary: WalletSummary;
}

export interface UpdateGeneralSettings {
  username: string;
  email: string;
  bio?: string | null;
  avatarUrl?: string | null;
  bannerUrl?: string | null;
  preferredLanguage?: string | null;
}

export interface ChangePasswordRequest {
  oldPassword: string;
  newPassword: string;
  confirmNewPassword: string;
}

export interface DeleteAccountRequest {
  usernameConfirmation: string;
  password: string;
  confirmPassword: string;
}

export interface WalletTransactionItem {
  id: string;
  amount: number;
  type: string;
  title: string;
  currency: string;
  date: string;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export type SettingsTabId = 'general' | 'password' | 'notifications' | 'wallet' | 'delete-account';
