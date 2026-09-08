export type NotificationType =
  | 'friend_request'
  | 'friend_accepted'
  | 'wallet_deposit'
  | 'chat_message'
  | 'system'
  | 'security_alert';

export interface NotificationActor {
  id: string;
  username: string;
  avatarUrl: string | null;
}

export interface AppNotification {
  id: string;
  type: NotificationType;
  title: string;
  message: string;
  entityType: string | null;
  entityId: string | null;
  eventId: string | null;
  actor: NotificationActor | null;
  dataJson: string | null;
  isRead: boolean;
  createdAt: string;
  readAt: string | null;
}

export interface NotificationFeedResponse {
  items: AppNotification[];
  unreadCount: number;
  nextCursor: string | null;
  hasMore: boolean;
}

/**
 * Helper to determine client-side navigation route for a notification
 */
export function getNotificationUrl(notification: AppNotification): { tab: string; subTab?: string; param?: string } {
  switch (notification.type) {
    case 'friend_request':
    case 'friend_accepted':
      return { tab: 'friends' };
    case 'wallet_deposit':
      return { tab: 'settings', subTab: 'wallet' };
    case 'chat_message':
      return { tab: 'chat', param: notification.actor?.id };
    default:
      return { tab: 'store' };
  }
}
