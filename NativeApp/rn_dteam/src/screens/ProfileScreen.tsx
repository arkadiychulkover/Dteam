import React, { useState } from 'react';
import {
  View,
  Text,
  StyleSheet,
  ScrollView,
  Image,
  TouchableOpacity,
  StatusBar,
  Alert,
} from 'react-native';
import { colors } from '../theme/colors';
import { UserProfile, Badge } from '../types';
import { ProfileLevelCard } from '../components/common/ProfileLevelCard';
import { BadgeCard } from '../components/common/BadgeCard';
import { BadgeDetailModal } from '../components/common/BadgeDetailModal';
import { useAuthStore } from '../store/useAuthStore';
import { Ionicons } from '@expo/vector-icons';

interface ProfileScreenProps {
  profile: UserProfile;
  onNavigateToSettings: () => void;
  onNavigateToLibrary?: () => void;
  onNavigateToLogin?: () => void;
}

export const ProfileScreen: React.FC<ProfileScreenProps> = ({
  profile,
  onNavigateToSettings,
  onNavigateToLibrary,
  onNavigateToLogin,
}) => {
  const { user, isAuthenticated, logout } = useAuthStore();
  const [selectedBadge, setSelectedBadge] = useState<Badge | null>(null);
  const [isBadgeModalOpen, setIsBadgeModalOpen] = useState(false);

  const effectiveUsername = user?.username || profile.username;
  const effectiveEmail = user?.email || profile.email;

  const handleOpenBadge = (badge: Badge) => {
    setSelectedBadge(badge);
    setIsBadgeModalOpen(true);
  };

  const handleCopyWallet = () => {
    Alert.alert('Гаманець', `Адресу скопійовано:\n${profile.walletAddress}`);
  };

  return (
    <View style={styles.container}>
      <StatusBar barStyle="light-content" backgroundColor={colors.background} />

      <ScrollView showsVerticalScrollIndicator={false} contentContainerStyle={styles.scrollContent}>
        <View style={styles.bannerContainer}>
          <Image
            source={{ uri: 'https://images.unsplash.com/photo-1542751371-adc38448a05e?w=800&auto=format&fit=crop&q=80' }}
            style={styles.bannerImage}
            resizeMode="cover"
          />
          <View style={styles.bannerOverlay} />

          <View style={styles.topActionsRow}>
            <View style={styles.networkBadge}>
              <View style={styles.networkDot} />
              <Text style={styles.networkText}>TON NETWORK</Text>
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

        <View style={styles.profileHeader}>
          <View style={styles.avatarRow}>
            <View style={styles.avatarWrapper}>
              <Image source={{ uri: profile.avatarUrl }} style={styles.avatar} />
              <View style={styles.onlineDot} />
            </View>

            <View style={styles.headerButtons}>
              <TouchableOpacity
                style={styles.editProfileBtn}
                onPress={onNavigateToSettings}
                activeOpacity={0.8}
              >
                <Ionicons name="create-outline" size={16} color="#021118" />
                <Text style={styles.editProfileText}>Редагувати</Text>
              </TouchableOpacity>
            </View>
          </View>

          <View style={styles.identityBlock}>
            <View style={styles.nameRow}>
              <Text style={styles.displayName}>{profile.displayName || effectiveUsername}</Text>
              <Ionicons name="checkmark-circle" size={18} color={colors.primary} />
            </View>
            <Text style={styles.usernameHandle}>@{effectiveUsername.toLowerCase()}</Text>

            {profile.gameStatus ? (
              <View style={styles.gameStatusRow}>
                <Ionicons name="game-controller-outline" size={14} color={colors.accentPurple} />
                <Text style={styles.gameStatusText}>{profile.gameStatus}</Text>
              </View>
            ) : null}

            <Text style={styles.bioText}>{profile.bio}</Text>
          </View>

          <View style={styles.statsGrid}>
            <TouchableOpacity
              style={styles.statItem}
              onPress={onNavigateToLibrary}
              activeOpacity={0.7}
            >
              <Text style={styles.statNumber}>{profile.stats.gamesCount}</Text>
              <Text style={styles.statLabel}>Ігор</Text>
            </TouchableOpacity>

            <View style={styles.statDivider} />

            <View style={styles.statItem}>
              <Text style={styles.statNumber}>{profile.badges.length}</Text>
              <Text style={styles.statLabel}>Значків</Text>
            </View>

            <View style={styles.statDivider} />

            <View style={styles.statItem}>
              <Text style={styles.statNumber}>{profile.stats.reviewsCount}</Text>
              <Text style={styles.statLabel}>Рецензій</Text>
            </View>

            <View style={styles.statDivider} />

            <View style={styles.statItem}>
              <Text style={styles.statNumber}>{profile.stats.friendsCount}</Text>
              <Text style={styles.statLabel}>Друзів</Text>
            </View>
          </View>
        </View>

        <ProfileLevelCard
          level={profile.level}
          currentXp={profile.currentXp}
          nextLevelXp={profile.nextLevelXp}
        />

        <View style={styles.walletCard}>
          <View style={styles.walletHeader}>
            <View style={styles.walletTitleRow}>
              <Ionicons name="wallet-outline" size={18} color={colors.primary} />
              <Text style={styles.walletTitle}>DTEAM Web3 Баланс</Text>
            </View>
            <TouchableOpacity onPress={handleCopyWallet} activeOpacity={0.7}>
              <Ionicons name="copy-outline" size={16} color={colors.textDim} />
            </TouchableOpacity>
          </View>

          <View style={styles.balanceRow}>
            <View style={styles.balanceItem}>
              <Text style={styles.balanceLabel}>Бали DTP</Text>
              <Text style={styles.balanceValueDtp}>{profile.tokensBalance} DTP</Text>
            </View>
            <View style={styles.balanceItem}>
              <Text style={styles.balanceLabel}>UAH Баланс</Text>
              <Text style={styles.balanceValueUah}>₴ {profile.balanceUah.toFixed(2)}</Text>
            </View>
          </View>

          <Text style={styles.walletAddressText} numberOfLines={1}>
            {profile.walletAddress}
          </Text>
        </View>

        <View style={styles.section}>
          <View style={styles.sectionHeader}>
            <View style={styles.sectionTitleRow}>
              <Ionicons name="trophy-outline" size={18} color={colors.accentAmber} />
              <Text style={styles.sectionTitle}>Колекція значків ({profile.badges.length})</Text>
            </View>
          </View>

          <ScrollView horizontal showsHorizontalScrollIndicator={false} style={styles.badgesScroll}>
            {profile.badges.map((badge) => (
              <BadgeCard key={badge.id} badge={badge} onPress={handleOpenBadge} />
            ))}
          </ScrollView>
        </View>

        <View style={styles.section}>
          <View style={styles.sectionHeader}>
            <View style={styles.sectionTitleRow}>
              <Ionicons name="pulse-outline" size={18} color={colors.primary} />
              <Text style={styles.sectionTitle}>Нещодавня активність</Text>
            </View>
          </View>

          <View style={styles.activitiesList}>
            {profile.recentActivity.map((act) => (
              <View key={act.id} style={styles.activityItem}>
                <View style={styles.activityIconBox}>
                  <Ionicons
                    name={
                      act.type === 'achievement'
                        ? 'ribbon'
                        : act.type === 'game_purchased'
                        ? 'cart'
                        : 'sparkles'
                    }
                    size={16}
                    color={colors.primary}
                  />
                </View>
                <View style={styles.activityContent}>
                  <Text style={styles.activityTitle}>{act.title}</Text>
                  <Text style={styles.activitySubtitle}>{act.subtitle}</Text>
                </View>
                <Text style={styles.activityTime}>{act.timestamp}</Text>
              </View>
            ))}
          </View>
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
  },
  bannerImage: {
    width: '100%',
    height: '100%',
  },
  bannerOverlay: {
    position: 'absolute',
    top: 0,
    left: 0,
    right: 0,
    bottom: 0,
    backgroundColor: 'rgba(2, 17, 24, 0.45)',
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
    backgroundColor: 'rgba(3, 33, 44, 0.75)',
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
  gameStatusRow: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 6,
    marginTop: 6,
  },
  gameStatusText: {
    fontSize: 12,
    fontWeight: '700',
    color: colors.accentPurple,
  },
  bioText: {
    fontSize: 13,
    color: colors.textSecondary,
    lineHeight: 18,
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
