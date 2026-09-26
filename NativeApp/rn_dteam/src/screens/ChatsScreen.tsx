import React, { useState, useEffect, useCallback } from 'react';
import {
  View,
  Text,
  StyleSheet,
  TextInput,
  FlatList,
  TouchableOpacity,
  StatusBar,
  ActivityIndicator,
  RefreshControl,
  Modal,
} from 'react-native';
import { colors } from '../theme/colors';
import { Dialog, UserStatus } from '../types';
import { DialogItem } from '../components/chat/DialogItem';
import { AuthRequiredView } from '../components/common/AuthRequiredView';
import { BackendImage } from '../components/BackendImage';
import { useAuthStore } from '../store/useAuthStore';
import { useChatStore } from '../store/useChatStore';
import { usePresenceStore } from '../store/usePresenceStore';
import { chatService, BackendFriend } from '../services/chatService';
import { Ionicons } from '@expo/vector-icons';

interface ChatsScreenProps {
  onSelectDialog: (dialog: Dialog) => void;
  navigation?: any;
  onNavigateLogin?: () => void;
}

export const ChatsScreen: React.FC<ChatsScreenProps> = ({
  onSelectDialog,
  navigation,
  onNavigateLogin,
}) => {
  const { user, isAuthenticated } = useAuthStore();
  const conversations = useChatStore((s) => s.conversations);
  const isLoadingConversations = useChatStore((s) => s.isLoadingConversations);
  const loadConversations = useChatStore((s) => s.loadConversations);
  const isOnline = usePresenceStore((s) => s.isOnline);

  const [friends, setFriends] = useState<BackendFriend[]>([]);
  const [isRefreshing, setIsRefreshing] = useState(false);
  const [searchQuery, setSearchQuery] = useState('');
  const [filter, setFilter] = useState<'all' | 'online' | 'unread'>('all');
  const [isNewChatModalOpen, setIsNewChatModalOpen] = useState(false);

  const fetchFriends = useCallback(async () => {
    if (!isAuthenticated) return;
    try {
      const friendsList = await chatService.getFriends();
      setFriends(friendsList);
    } catch (err) {
      console.warn('[ChatsScreen] Error fetching friends list:', err);
    }
  }, [isAuthenticated]);

  useEffect(() => {
    if (isAuthenticated) {
      loadConversations();
      fetchFriends();
    } else {
      setFriends([]);
    }
  }, [isAuthenticated, loadConversations, fetchFriends]);

  const handleRefresh = async () => {
    setIsRefreshing(true);
    await Promise.all([
      loadConversations(),
      fetchFriends(),
    ]);
    setIsRefreshing(false);
  };

  const handleStartChatWithFriend = (friend: BackendFriend) => {
    setIsNewChatModalOpen(false);
    // Шукаємо, чи вже є діалог з цим другом
    const existing = conversations.find((c) => c.friendId === friend.id);
    if (existing) {
      onSelectDialog(existing);
      return;
    }

    // Створюємо новий діалог для переходу
    const newDialog: Dialog = {
      id: friend.id,
      friendId: friend.id,
      friendUsername: friend.username,
      friendAvatarUrl: friend.avatarUrl || '',
      friendStatus: (friend.status as UserStatus) || 0,
      lastMessage: 'Розпочніть спілкування',
      lastMessageTime: 'Зараз',
      unreadCount: 0,
      messages: [],
    };
    onSelectDialog(newDialog);
  };

  if (!isAuthenticated) {
    return (
      <AuthRequiredView
        screenTitle="Повідомлення"
        screenSubtitle="Спілкуйтеся з друзями та тімейтами"
        icon="chatbubbles-outline"
        title="Потрібна авторизація"
        description="Увійдіть у ваш акаунт DTEAM, щоб переглядати листування та спілкуватися з друзями."
        onLogin={() => {
          if (onNavigateLogin) {
            onNavigateLogin();
          } else if (navigation?.navigate) {
            navigation.navigate('Login');
          }
        }}
      />
    );
  }

  const filteredDialogs = conversations.filter((d) => {
    const matchesSearch =
      d.friendUsername.toLowerCase().includes(searchQuery.toLowerCase()) ||
      d.lastMessage.toLowerCase().includes(searchQuery.toLowerCase());

    if (!matchesSearch) return false;

    if (filter === 'online') {
      return isOnline(d.friendId) || d.friendStatus === 1 || d.friendStatus === 2;
    }
    if (filter === 'unread') {
      return d.unreadCount > 0;
    }
    return true;
  });

  return (
    <View style={styles.container}>
      <StatusBar barStyle="light-content" backgroundColor={colors.background} />

      <View style={styles.header}>
        <View>
          <Text style={styles.screenTitle}>Повідомлення</Text>
          <Text style={styles.screenSubtitle}>
            {conversations.length} {conversations.length === 1 ? 'діалог' : 'діалогів'} з друзями
          </Text>
        </View>

        <TouchableOpacity
          style={styles.newChatBtn}
          onPress={() => setIsNewChatModalOpen(true)}
          activeOpacity={0.8}
        >
          <Ionicons name="create-outline" size={20} color="#021118" />
        </TouchableOpacity>
      </View>

      <View style={styles.searchContainer}>
        <Ionicons name="search" size={18} color={colors.textDim} style={styles.searchIcon} />
        <TextInput
          style={styles.searchInput}
          placeholder="Пошук діалогів чи повідомлень..."
          placeholderTextColor={colors.textDim}
          value={searchQuery}
          onChangeText={setSearchQuery}
        />
        {searchQuery.length > 0 && (
          <TouchableOpacity onPress={() => setSearchQuery('')}>
            <Ionicons name="close-circle" size={18} color={colors.textDim} />
          </TouchableOpacity>
        )}
      </View>

      <View style={styles.filtersRow}>
        <TouchableOpacity
          style={[styles.filterChip, filter === 'all' && styles.filterChipActive]}
          onPress={() => setFilter('all')}
        >
          <Text style={[styles.filterText, filter === 'all' && styles.filterTextActive]}>
            Всі ({conversations.length})
          </Text>
        </TouchableOpacity>

        <TouchableOpacity
          style={[styles.filterChip, filter === 'online' && styles.filterChipActive]}
          onPress={() => setFilter('online')}
        >
          <View style={[styles.filterDot, { backgroundColor: colors.accentEmerald }]} />
          <Text style={[styles.filterText, filter === 'online' && styles.filterTextActive]}>
            У мережі
          </Text>
        </TouchableOpacity>

        <TouchableOpacity
          style={[styles.filterChip, filter === 'unread' && styles.filterChipActive]}
          onPress={() => setFilter('unread')}
        >
          <Text style={[styles.filterText, filter === 'unread' && styles.filterTextActive]}>
            Непрочитані
          </Text>
        </TouchableOpacity>
      </View>

      {isLoadingConversations && conversations.length === 0 ? (
        <View style={styles.centerBox}>
          <ActivityIndicator size="large" color={colors.primary} />
          <Text style={styles.loadingText}>Завантаження діалогів...</Text>
        </View>
      ) : (
        <FlatList
          data={filteredDialogs}
          keyExtractor={(item) => item.id}
          renderItem={({ item }) => (
            <DialogItem dialog={item} onPress={() => onSelectDialog(item)} />
          )}
          contentContainerStyle={styles.listContent}
          showsVerticalScrollIndicator={false}
          refreshControl={
            <RefreshControl
              refreshing={isRefreshing}
              onRefresh={handleRefresh}
              tintColor={colors.primary}
              colors={[colors.primary]}
            />
          }
          ListEmptyComponent={
            <View style={styles.emptyContainer}>
              <View style={styles.emptyIconCircle}>
                <Ionicons name="chatbubbles-outline" size={36} color={colors.primary} />
              </View>
              <Text style={styles.emptyTitle}>
                {searchQuery ? 'Діалогів не знайдено' : 'Немає активних діалогів'}
              </Text>
              <Text style={styles.emptySubtitle}>
                {searchQuery
                  ? 'Спробуйте змінити пошуковий запит'
                  : 'Виберіть друга зі списку, щоб надіслати повідомлення'}
              </Text>

              {friends.length > 0 && !searchQuery && (
                <View style={styles.quickFriendsSection}>
                  <Text style={styles.quickFriendsHeader}>Ваші друзі:</Text>
                  {friends.map((f) => (
                    <TouchableOpacity
                      key={f.id}
                      style={styles.quickFriendRow}
                      onPress={() => handleStartChatWithFriend(f)}
                      activeOpacity={0.8}
                    >
                      <BackendImage
                        src={f.avatarUrl}
                        style={styles.quickFriendAvatar}
                        fallbackText={f.username.slice(0, 2).toUpperCase()}
                      />
                      <View style={styles.quickFriendInfo}>
                        <Text style={styles.quickFriendName}>{f.username}</Text>
                        <Text
                          style={[
                            styles.quickFriendStatus,
                            {
                              color:
                                isOnline(f.id) || f.status === 1
                                  ? colors.accentEmerald
                                  : colors.textDim,
                            },
                          ]}
                        >
                          {isOnline(f.id) || f.status === 1
                            ? 'У мережі'
                            : f.status === 2
                            ? 'У грі'
                            : 'Офлайн'}
                        </Text>
                      </View>
                      <View style={styles.quickWriteBtn}>
                        <Ionicons name="paper-plane-outline" size={14} color="#021118" />
                        <Text style={styles.quickWriteText}>Написати</Text>
                      </View>
                    </TouchableOpacity>
                  ))}
                </View>
              )}
            </View>
          }
        />
      )}

      {/* Модальне вікно вибору друга для нового чату */}
      <Modal
        visible={isNewChatModalOpen}
        animationType="slide"
        transparent
        onRequestClose={() => setIsNewChatModalOpen(false)}
      >
        <View style={styles.modalOverlay}>
          <View style={styles.modalContent}>
            <View style={styles.modalHeader}>
              <Text style={styles.modalTitle}>Нове повідомлення</Text>
              <TouchableOpacity
                onPress={() => setIsNewChatModalOpen(false)}
                activeOpacity={0.7}
              >
                <Ionicons name="close" size={24} color={colors.textMuted} />
              </TouchableOpacity>
            </View>

            <Text style={styles.modalSubtitle}>Оберіть друга для початку спілкування:</Text>

            {friends.length === 0 ? (
              <View style={styles.noFriendsBox}>
                <Ionicons name="people-outline" size={36} color={colors.textDim} />
                <Text style={styles.noFriendsText}>У вас поки що немає друзів у списку</Text>
              </View>
            ) : (
              <FlatList
                data={friends}
                keyExtractor={(item) => item.id}
                renderItem={({ item }) => {
                  const friendOnline = isOnline(item.id) || item.status === 1;
                  return (
                    <TouchableOpacity
                      style={styles.friendModalItem}
                      onPress={() => handleStartChatWithFriend(item)}
                      activeOpacity={0.7}
                    >
                      <View style={styles.friendModalAvatarWrap}>
                        <BackendImage
                          src={item.avatarUrl}
                          style={styles.friendModalAvatar}
                          fallbackText={item.username.slice(0, 2).toUpperCase()}
                        />
                        <View
                          style={[
                            styles.friendModalStatusDot,
                            {
                              backgroundColor: friendOnline
                                ? colors.accentEmerald
                                : colors.offline,
                            },
                          ]}
                        />
                      </View>
                      <View style={styles.friendModalInfo}>
                        <View style={styles.friendModalNameRow}>
                          <Text style={styles.friendModalUsername}>{item.username}</Text>
                          <Text
                            style={[
                              styles.friendModalStatusText,
                              {
                                color: friendOnline
                                  ? colors.accentEmerald
                                  : colors.textDim,
                              },
                            ]}
                          >
                            {friendOnline ? 'У мережі' : 'Не в мережі'}
                          </Text>
                        </View>
                        <Text style={styles.friendModalBio} numberOfLines={1}>
                          {item.bio || 'Користувач DTEAM'}
                        </Text>
                      </View>
                      <Ionicons name="chevron-forward" size={18} color={colors.textDim} />
                    </TouchableOpacity>
                  );
                }}
              />
            )}
          </View>
        </View>
      </Modal>
    </View>
  );
};

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: colors.background,
    paddingHorizontal: 16,
    paddingTop: 12,
  },
  header: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    marginBottom: 16,
  },
  screenTitle: {
    fontSize: 22,
    fontWeight: '900',
    color: colors.text,
    letterSpacing: 0.3,
  },
  screenSubtitle: {
    fontSize: 12,
    color: colors.textMuted,
    marginTop: 2,
  },
  newChatBtn: {
    width: 40,
    height: 40,
    borderRadius: 12,
    backgroundColor: colors.primary,
    alignItems: 'center',
    justifyContent: 'center',
  },
  searchContainer: {
    flexDirection: 'row',
    alignItems: 'center',
    backgroundColor: colors.surface,
    borderRadius: 14,
    paddingHorizontal: 12,
    height: 44,
    borderWidth: 1,
    borderColor: colors.border,
    marginBottom: 14,
  },
  searchIcon: {
    marginRight: 8,
  },
  searchInput: {
    flex: 1,
    color: colors.text,
    fontSize: 13,
  },
  filtersRow: {
    flexDirection: 'row',
    gap: 8,
    marginBottom: 14,
  },
  filterChip: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 6,
    backgroundColor: colors.surfaceCard,
    paddingHorizontal: 12,
    paddingVertical: 7,
    borderRadius: 10,
    borderWidth: 1,
    borderColor: colors.border,
  },
  filterChipActive: {
    backgroundColor: 'rgba(34, 211, 238, 0.15)',
    borderColor: colors.primary,
  },
  filterDot: {
    width: 6,
    height: 6,
    borderRadius: 3,
  },
  filterText: {
    fontSize: 12,
    fontWeight: '600',
    color: colors.textMuted,
  },
  filterTextActive: {
    color: colors.primary,
    fontWeight: '800',
  },
  listContent: {
    paddingBottom: 20,
  },
  centerBox: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
    paddingVertical: 40,
  },
  loadingText: {
    color: colors.textMuted,
    fontSize: 13,
    marginTop: 10,
  },
  emptyContainer: {
    alignItems: 'center',
    paddingVertical: 32,
    paddingHorizontal: 16,
  },
  emptyIconCircle: {
    width: 64,
    height: 64,
    borderRadius: 32,
    backgroundColor: 'rgba(34, 211, 238, 0.1)',
    justifyContent: 'center',
    alignItems: 'center',
    marginBottom: 14,
  },
  emptyTitle: {
    fontSize: 16,
    fontWeight: '800',
    color: colors.text,
    marginBottom: 6,
  },
  emptySubtitle: {
    fontSize: 12,
    color: colors.textMuted,
    textAlign: 'center',
    lineHeight: 18,
    marginBottom: 20,
  },
  quickFriendsSection: {
    width: '100%',
    backgroundColor: colors.surfaceCard,
    borderRadius: 16,
    padding: 16,
    borderWidth: 1,
    borderColor: colors.border,
    marginTop: 8,
  },
  quickFriendsHeader: {
    fontSize: 13,
    fontWeight: '800',
    color: colors.textSecondary,
    marginBottom: 12,
  },
  quickFriendRow: {
    flexDirection: 'row',
    alignItems: 'center',
    paddingVertical: 10,
    borderBottomWidth: 1,
    borderBottomColor: colors.border,
    gap: 12,
  },
  quickFriendAvatar: {
    width: 40,
    height: 40,
    borderRadius: 20,
  },
  quickFriendInfo: {
    flex: 1,
  },
  quickFriendName: {
    fontSize: 14,
    fontWeight: '700',
    color: colors.text,
  },
  quickFriendStatus: {
    fontSize: 11,
    color: colors.textDim,
    marginTop: 2,
  },
  quickWriteBtn: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 6,
    backgroundColor: colors.primary,
    paddingHorizontal: 12,
    paddingVertical: 6,
    borderRadius: 8,
  },
  quickWriteText: {
    fontSize: 12,
    fontWeight: '800',
    color: '#021118',
  },
  modalOverlay: {
    flex: 1,
    backgroundColor: 'rgba(2, 17, 24, 0.8)',
    justifyContent: 'flex-end',
  },
  modalContent: {
    backgroundColor: colors.surfaceCard,
    borderTopLeftRadius: 24,
    borderTopRightRadius: 24,
    padding: 20,
    maxHeight: '70%',
    borderWidth: 1,
    borderColor: colors.border,
  },
  modalHeader: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    marginBottom: 8,
  },
  modalTitle: {
    fontSize: 18,
    fontWeight: '900',
    color: colors.text,
  },
  modalSubtitle: {
    fontSize: 12,
    color: colors.textMuted,
    marginBottom: 16,
  },
  noFriendsBox: {
    alignItems: 'center',
    paddingVertical: 32,
    gap: 8,
  },
  noFriendsText: {
    fontSize: 13,
    color: colors.textDim,
  },
  friendModalItem: {
    flexDirection: 'row',
    alignItems: 'center',
    paddingVertical: 12,
    borderBottomWidth: 1,
    borderBottomColor: colors.border,
    gap: 12,
  },
  friendModalAvatarWrap: {
    position: 'relative',
  },
  friendModalAvatar: {
    width: 44,
    height: 44,
    borderRadius: 22,
  },
  friendModalStatusDot: {
    position: 'absolute',
    bottom: 0,
    right: 0,
    width: 12,
    height: 12,
    borderRadius: 6,
    borderWidth: 2,
    borderColor: '#08121f',
  },
  friendModalInfo: {
    flex: 1,
  },
  friendModalNameRow: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
  },
  friendModalStatusText: {
    fontSize: 11,
    fontWeight: '700',
  },
  friendModalUsername: {
    fontSize: 14,
    fontWeight: '800',
    color: colors.text,
  },
  friendModalBio: {
    fontSize: 12,
    color: colors.textDim,
    marginTop: 2,
  },
});
