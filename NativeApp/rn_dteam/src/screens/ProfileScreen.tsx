import React, { useState, useEffect, useCallback } from 'react';
import {
  View,
  Text,
  StyleSheet,
  ScrollView,
  TouchableOpacity,
  StatusBar,
  Alert,
  ActivityIndicator,
  RefreshControl,
} from 'react-native';
import { colors } from '../theme/colors';
import { Badge } from '../types';
import { ProfileLevelCard } from '../components/common/ProfileLevelCard';
import { BadgeCard } from '../components/common/BadgeCard';
import { BadgeDetailModal } from '../components/common/BadgeDetailModal';
import { AuthRequiredView } from '../components/common/AuthRequiredView';
import { BackendImage } from '../components/BackendImage';
import { useAuthStore } from '../store/useAuthStore';
import { nftService } from '../services/nftService';
import { calculateProfileLevel } from '../utils/levelUtils';
import { api } from '../services/api';
import { Ionicons } from '@expo/vector-icons';

interface RealActivityItem {
  id: string;
  title: string;
  description?: string;
  details?: string;
  createdAt: string;
  typeName?: string;
}

interface ProfileScreenProps {
  onNavigateToSettings: () => void;
  onNavigateToLibrary?: () => void;
  onNavigateToLogin?: () => void;
  navigation?: any;
}

export const ProfileScreen: React.FC<ProfileScreenProps> = ({
  onNavigateToSettings,
  onNavigateToLibrary,
  onNavigateToLogin,
  navigation,
}) => {
  const { user, isAuthenticated, checkAuth } = useAuthStore();
  const [selectedBadge, setSelectedBadge] = useState<Badge | null>(null);
  const [isBadgeModalOpen, setIsBadgeModalOpen] = useState(false);

  const [tdpBalance, setTdpBalance] = useState<number>(0);
  const [userNfts, setUserNfts] = useState<Badge[]>([]);
  const [activities, setActivities] = useState<RealActivityItem[]>([]);
  const [isLoadingServerData, setIsLoadingServerData] = useState<boolean>(true);
  const [isRefreshing, setIsRefreshing] = useState<boolean>(false);
  const [liveStats, setLiveStats] = useState<{ gamesCount: number; friendsCount: number }>({
    gamesCount: 0,
    friendsCount: 0,
  });

  const username = user?.username || 'Користувач';
  const email = user?.email || '';
  const walletAddress = user?.hardhatAddress || user?.walletAddress || '';
  const bio = user?.bio || '';

  // Завантажуємо реальні дані профілю, балансу, значків та активності з сервера
  const loadRealServerData = useCallback(async () => {
    if (!isAuthenticated || !user) {
      setIsLoadingServerData(false);
      setIsRefreshing(false);
      return;
    }

    try {
      const wallet = user.hardhatAddress || user.walletAddress;

      // 1. Реальний баланс TDP з ендпоінту бекенду
      if (wallet) {
        const balance = await nftService.getTdpBalance(wallet);
        setTdpBalance(balance);
      }

      // 2. Тільки РЕАЛЬНІ NFT-значки цього користувача з сервера
      const nfts = await nftService.getUserNfts(user.id);
      setUserNfts(nfts);

      // 3. Реальна статистика ігор та друзів з профілю
      try {
        const publicProfile = await api.get<any>(`/users/${user.id}/profile`);
        if (publicProfile) {
          setLiveStats({
            gamesCount: publicProfile.gamesCount ?? 0,
            friendsCount: publicProfile.friendsCount ?? 0,
          });
        }
      } catch (err) {
        console.warn('[ProfileScreen] Could not fetch profile stats:', err);
      }

      // 4. Реальна стрічка активності користувача
      try {
        const userActs = await api.get<RealActivityItem[]>(`/activity/user/${user.id}`);
        if (Array.isArray(userActs)) {
          setActivities(userActs);
        }
      } catch {
        // Якщо немає активності, залишається пустий масив
      }
    } catch (err) {
      console.warn('[ProfileScreen] Error loading real server data:', err);
    } finally {
      setIsLoadingServerData(false);
      setIsRefreshing(false);
    }
  }, [isAuthenticated, user?.id, user?.hardhatAddress, user?.walletAddress]);

  useEffect(() => {
    if (isAuthenticated) {
      setIsLoadingServerData(true);
      loadRealServerData();
    } else {
      setIsLoadingServerData(false);
    }
  }, [isAuthenticated, loadRealServerData]);

  const handleRefresh = async () => {
    setIsRefreshing(true);
    await checkAuth().catch(() => {});
    await loadRealServerData();
  };

  if (!isAuthenticated) {
    return (
      <AuthRequiredView
        screenTitle="МІЙ ПРОФІЛЬ"
        screenSubtitle="Особистий кабінет геймера"
        icon="person-circle-outline"
        title="Потрібна авторизація"
        description="Увійдіть у ваш акаунт DTEAM, щоб переглядати свій профіль, баланс TDP, NFT значки та рівень."
        onLogin={() => {
          if (onNavigateToLogin) {
            onNavigateToLogin();
          } else if (navigation?.navigate) {
            navigation.navigate('Login');
          }
        }}
      />
    );
  }

  // Обчислюємо динамічний рівень на основі реальних TDP токенів
  const levelInfo = calculateProfileLevel(tdpBalance);

  const handleOpenBadge = (badge: Badge) => {
    setSelectedBadge(badge);
    setIsBadgeModalOpen(true);
  };

  const handleCopyWallet = () => {
    if (walletAddress) {
      Alert.alert('Гаманець', `Адресу скопійовано:\n${walletAddress}`);
    }
  };

  // Реальний баланс TON з даних користувача
  const tonBalance = user?.balanceInNanoTons
    ? (user.balanceInNanoTons / 1e9).toFixed(2)
    : '0.00';

  return (
    <View style={styles.container}>
      <StatusBar barStyle="light-content" backgroundColor={colors.background} />

      <ScrollView
        showsVerticalScrollIndicator={false}
        contentContainerStyle={styles.scrollContent}
        refreshControl={
          <RefreshControl
            refreshing={isRefreshing}
            onRefresh={handleRefresh}
            tintColor={colors.primary}
            colors={[colors.primary]}
          />
        }
      >
        {/* Баннер профілю */}
        <View style={styles.bannerContainer}>
          {user?.bannerUrl ? (
            <BackendImage
              src={user.bannerUrl}
              style={styles.bannerImage}
              resizeMode="cover"
            />
          ) : (
            <View style={styles.bannerPlaceholder} />
          )}
          <View style={styles.bannerOverlay} />

          <View style={styles.topActionsRow}>
            <View style={styles.networkBadge}>
              <View style={styles.networkDot} />
              <Text style={styles.networkText}>DTEAM LIVE</Text>
            </View>

            <TouchableOpacity
              style={styles.settingsBtn}
              onPress={onNavigateToSettings}
              activeOpacity={0.8}
            >
              <Ionicons name="settings-outline" size={20} color={colors.text} />
            </TouchableOpacity>
          </View>
        </View>

        {/* Шапка профілю з аватаром та даними */}
        <View style={styles.profileHeader}>
          <View style={styles.avatarRow}>
            <View style={styles.avatarWrapper}>
              <BackendImage
                src={user?.avatarUrl}
                style={styles.avatar}
                fallbackText={username.slice(0, 2).toUpperCase()}
              />
              <View style={styles.onlineDot} />
            </View>

            <View style={styles.headerButtons}>
              <TouchableOpacity
                style={styles.editProfileBtn}
                onPress={onNavigateToSettings}
                activeOpacity={0.8}
              >
                <Ionicons name="create-outline" size={16} color="#021118" />
                <Text style={styles.editProfileText}>Налаштування</Text>
              </TouchableOpacity>
            </View>
          </View>

          <View style={styles.identityBlock}>
            <View style={styles.nameRow}>
              <Text style={styles.displayName}>{username}</Text>
              <Ionicons name="checkmark-circle" size={18} color={colors.primary} />
            </View>
            <Text style={styles.usernameHandle}>@{username.toLowerCase()}</Text>

            {email ? (
              <Text style={styles.emailText}>{email}</Text>
            ) : null}

            {bio ? (
              <Text style={styles.bioText}>{bio}</Text>
            ) : (
              <Text style={styles.bioTextEmpty}>Опис профілю ще не додано</Text>
            )}
          </View>

          {/* Тільки реальні показники (Ігор, Значків, Друзів) */}
          <View style={styles.statsGrid}>
            <TouchableOpacity
              style={styles.statItem}
              onPress={onNavigateToLibrary}
              activeOpacity={0.7}
            >
              <Text style={styles.statNumber}>{liveStats.gamesCount}</Text>
              <Text style={styles.statLabel}>Ігор</Text>
            </TouchableOpacity>

            <View style={styles.statDivider} />

            <View style={styles.statItem}>
              <Text style={styles.statNumber}>{userNfts.length}</Text>
              <Text style={styles.statLabel}>NFT Значків</Text>
            </View>

            <View style={styles.statDivider} />

            <View style={styles.statItem}>
              <Text style={styles.statNumber}>{liveStats.friendsCount}</Text>
              <Text style={styles.statLabel}>Друзів</Text>
            </View>
          </View>
        </View>

        {/* Динамічна картка рівня користувача на основі TDP */}
        <ProfileLevelCard
          level={levelInfo.level}
          currentXp={levelInfo.currentXp}
          nextLevelXp={levelInfo.xpForNextLevel}
        />

        {/* Картка балансу та кошелька */}
        <View style={styles.walletCard}>
          <View style={styles.walletHeader}>
            <View style={styles.walletTitleRow}>
              <Ionicons name="wallet-outline" size={18} color={colors.primary} />
              <Text style={styles.walletTitle}>DTEAM Баланс</Text>
            </View>
            {walletAddress ? (
              <TouchableOpacity onPress={handleCopyWallet} activeOpacity={0.7}>
                <Ionicons name="copy-outline" size={16} color={colors.textDim} />
              </TouchableOpacity>
            ) : null}
          </View>

          <View style={styles.balanceRow}>
            <View style={styles.balanceItem}>
              <Text style={styles.balanceLabel}>Бали TDP</Text>
              <Text style={styles.balanceValueDtp}>{tdpBalance} TDP</Text>
            </View>
            <View style={styles.balanceItem}>
              <Text style={styles.balanceLabel}>Баланс TON</Text>
              <Text style={styles.balanceValueUah}>{tonBalance} TON</Text>
            </View>
          </View>

          <Text style={styles.walletAddressText} numberOfLines={1}>
            {walletAddress || 'Гаманець не підключено'}
          </Text>
        </View>

        {/* Секція NFT значків (тільки РЕАЛЬНІ значки користувача) */}
        <View style={styles.section}>
          <View style={styles.sectionHeader}>
            <View style={styles.sectionTitleRow}>
              <Ionicons name="trophy-outline" size={18} color={colors.accentAmber} />
              <Text style={styles.sectionTitle}>Колекція NFT значків ({userNfts.length})</Text>
              {isLoadingServerData && (
                <ActivityIndicator size="small" color={colors.primary} style={{ marginLeft: 8 }} />
              )}
            </View>
          </View>

          {userNfts.length > 0 ? (
            <ScrollView horizontal showsHorizontalScrollIndicator={false} style={styles.badgesScroll}>
              {userNfts.map((badge) => (
                <BadgeCard key={badge.id} badge={badge} onPress={handleOpenBadge} />
              ))}
            </ScrollView>
          ) : (
            <View style={styles.emptySectionBox}>
              <Ionicons name="ribbon-outline" size={36} color={colors.textDim} />
              <Text style={styles.emptySectionTitle}>У вас поки що немає NFT значків</Text>
              <Text style={styles.emptySectionSubtitle}>
                Отримуйте значки за ігрову активність та участь у спільноті Dteam.
              </Text>
            </View>
          )}
        </View>

        {/* Секція активності (тільки РЕАЛЬНІ події користувача з сервера) */}
        <View style={styles.section}>
          <View style={styles.sectionHeader}>
            <View style={styles.sectionTitleRow}>
              <Ionicons name="pulse-outline" size={18} color={colors.primary} />
              <Text style={styles.sectionTitle}>Нещодавня активність</Text>
            </View>
          </View>

          {activities.length > 0 ? (
            <View style={styles.activitiesList}>
              {activities.map((act) => (
                <View key={act.id} style={styles.activityItem}>
                  <View style={styles.activityIconBox}>
                    <Ionicons name="sparkles" size={16} color={colors.primary} />
                  </View>
                  <View style={styles.activityContent}>
                    <Text style={styles.activityTitle}>{act.title}</Text>
                    {act.description ? (
                      <Text style={styles.activitySubtitle}>{act.description}</Text>
                    ) : null}
                  </View>
                  <Text style={styles.activityTime}>
                    {new Date(act.createdAt).toLocaleDateString('uk-UA')}
                  </Text>
                </View>
              ))}
            </View>
          ) : (
            <View style={styles.emptySectionBox}>
              <Ionicons name="flash-outline" size={32} color={colors.textDim} />
              <Text style={styles.emptySectionTitle}>Нещодавньої активності не зафіксовано</Text>
              <Text style={styles.emptySectionSubtitle}>
                Ваші досягнення та покупки відображатимуться тут.
              </Text>
            </View>
          )}
        </View>
      </ScrollView>

      <BadgeDetailModal
        badge={selectedBadge}
        isOpen={isBadgeModalOpen}
        onClose={() => setIsBadgeModalOpen(false)}
      />
    </View>
  );
};

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: colors.background,
  },
  scrollContent: {
    paddingBottom: 40,
  },
  bannerContainer: {
    height: 140,
    width: '100%',
    position: 'relative',
    backgroundColor: '#031f2b',
  },
  bannerImage: {
    width: '100%',
    height: '100%',
  },
  bannerPlaceholder: {
    width: '100%',
    height: '100%',
    backgroundColor: 'rgba(3, 33, 44, 0.9)',
  },
  bannerOverlay: {
    position: 'absolute',
    top: 0,
    left: 0,
    right: 0,
    bottom: 0,
    backgroundColor: 'rgba(2, 17, 24, 0.5)',
  },
  topActionsRow: {
    position: 'absolute',
    top: 12,
    left: 16,
    right: 16,
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
  },
  networkBadge: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 6,
    backgroundColor: 'rgba(3, 33, 44, 0.85)',
    paddingHorizontal: 10,
    paddingVertical: 5,
    borderRadius: 12,
    borderWidth: 1,
    borderColor: colors.borderLight,
  },
  networkDot: {
    width: 6,
    height: 6,
    borderRadius: 3,
    backgroundColor: colors.primary,
  },
  networkText: {
    fontSize: 10,
    fontWeight: '800',
    color: colors.primary,
    fontFamily: 'monospace',
  },
  settingsBtn: {
    width: 38,
    height: 38,
    borderRadius: 12,
    backgroundColor: 'rgba(3, 33, 44, 0.85)',
    alignItems: 'center',
    justifyContent: 'center',
    borderWidth: 1,
    borderColor: colors.borderLight,
  },
  profileHeader: {
    backgroundColor: colors.surfaceCard,
    borderRadius: 24,
    marginHorizontal: 16,
    marginTop: -40,
    padding: 16,
    borderWidth: 1,
    borderColor: colors.border,
    marginBottom: 16,
  },
  avatarRow: {
    flexDirection: 'row',
    alignItems: 'flex-end',
    justifyContent: 'space-between',
    marginBottom: 12,
  },
  avatarWrapper: {
    position: 'relative',
  },
  avatar: {
    width: 76,
    height: 76,
    borderRadius: 38,
    borderWidth: 3,
    borderColor: colors.primary,
    backgroundColor: colors.surface,
  },
  onlineDot: {
    position: 'absolute',
    bottom: 2,
    right: 2,
    width: 16,
    height: 16,
    borderRadius: 8,
    backgroundColor: colors.accentEmerald,
    borderWidth: 2,
    borderColor: colors.surfaceCard,
  },
  headerButtons: {
    flexDirection: 'row',
    gap: 8,
  },
  editProfileBtn: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 6,
    backgroundColor: colors.primary,
    paddingHorizontal: 14,
    paddingVertical: 8,
    borderRadius: 12,
  },
  editProfileText: {
    fontSize: 12,
    fontWeight: '800',
    color: '#021118',
  },
  identityBlock: {
    marginBottom: 16,
  },
  nameRow: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 6,
  },
  displayName: {
    fontSize: 20,
    fontWeight: '900',
    color: colors.text,
  },
  usernameHandle: {
    fontSize: 12,
    color: colors.textDim,
    fontFamily: 'monospace',
    marginTop: 2,
  },
  emailText: {
    fontSize: 11,
    color: colors.textMuted,
    marginTop: 2,
  },
  bioText: {
    fontSize: 13,
    color: colors.textSecondary,
    lineHeight: 18,
    marginTop: 8,
  },
  bioTextEmpty: {
    fontSize: 12,
    color: colors.textDim,
    fontStyle: 'italic',
    marginTop: 8,
  },
  statsGrid: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-around',
    backgroundColor: colors.background,
    borderRadius: 14,
    paddingVertical: 12,
    borderWidth: 1,
    borderColor: colors.border,
  },
  statItem: {
    alignItems: 'center',
    flex: 1,
  },
  statNumber: {
    fontSize: 16,
    fontWeight: '900',
    color: colors.text,
    fontFamily: 'monospace',
  },
  statLabel: {
    fontSize: 10,
    color: colors.textDim,
    marginTop: 2,
  },
  statDivider: {
    width: 1,
    height: 24,
    backgroundColor: colors.border,
  },
  walletCard: {
    backgroundColor: colors.surfaceCard,
    borderRadius: 20,
    padding: 16,
    marginHorizontal: 16,
    borderWidth: 1,
    borderColor: colors.border,
    marginBottom: 16,
  },
  walletHeader: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    marginBottom: 12,
  },
  walletTitleRow: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 8,
  },
  walletTitle: {
    fontSize: 14,
    fontWeight: '800',
    color: colors.text,
  },
  balanceRow: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    marginBottom: 10,
  },
  balanceItem: {},
  balanceLabel: {
    fontSize: 11,
    color: colors.textDim,
    marginBottom: 2,
  },
  balanceValueDtp: {
    fontSize: 16,
    fontWeight: '900',
    color: colors.primary,
    fontFamily: 'monospace',
  },
  balanceValueUah: {
    fontSize: 16,
    fontWeight: '900',
    color: colors.accentEmerald,
    fontFamily: 'monospace',
  },
  walletAddressText: {
    fontSize: 10,
    color: colors.textDim,
    fontFamily: 'monospace',
    backgroundColor: colors.background,
    padding: 8,
    borderRadius: 8,
  },
  section: {
    marginHorizontal: 16,
    marginBottom: 20,
  },
  sectionHeader: {
    marginBottom: 12,
  },
  sectionTitleRow: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 8,
  },
  sectionTitle: {
    fontSize: 16,
    fontWeight: '800',
    color: colors.text,
  },
  badgesScroll: {
    paddingVertical: 4,
  },
  emptySectionBox: {
    backgroundColor: colors.surfaceCard,
    borderRadius: 16,
    padding: 24,
    alignItems: 'center',
    borderWidth: 1,
    borderColor: colors.border,
    gap: 6,
  },
  emptySectionTitle: {
    fontSize: 14,
    fontWeight: '800',
    color: colors.text,
    marginTop: 4,
  },
  emptySectionSubtitle: {
    fontSize: 11,
    color: colors.textDim,
    textAlign: 'center',
    lineHeight: 16,
    maxWidth: 260,
  },
  activitiesList: {
    backgroundColor: colors.surfaceCard,
    borderRadius: 16,
    borderWidth: 1,
    borderColor: colors.border,
    overflow: 'hidden',
  },
  activityItem: {
    flexDirection: 'row',
    alignItems: 'center',
    padding: 12,
    borderBottomWidth: 1,
    borderBottomColor: colors.border,
    gap: 12,
  },
  activityIconBox: {
    width: 32,
    height: 32,
    borderRadius: 16,
    backgroundColor: 'rgba(34, 211, 238, 0.1)',
    alignItems: 'center',
    justifyContent: 'center',
  },
  activityContent: {
    flex: 1,
  },
  activityTitle: {
    fontSize: 12,
    fontWeight: '700',
    color: colors.text,
  },
  activitySubtitle: {
    fontSize: 11,
    color: colors.textDim,
    marginTop: 1,
  },
  activityTime: {
    fontSize: 10,
    color: colors.textDim,
    fontFamily: 'monospace',
  },
});
