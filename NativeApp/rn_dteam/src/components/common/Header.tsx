import React from 'react';
import { View, Text, StyleSheet, TouchableOpacity } from 'react-native';
import { colors } from '../../theme/colors';
import { useAuthStore } from '../../store/useAuthStore';
import { Ionicons } from '@expo/vector-icons';

interface HeaderProps {
  isDemoMode?: boolean;
  onToggleDemoMode?: () => void;
  onNavigateLogin?: () => void;
  onNavigateProfile?: () => void;
}

export const Header: React.FC<HeaderProps> = ({
  isDemoMode = false,
  onToggleDemoMode,
  onNavigateLogin,
  onNavigateProfile,
}) => {
  const { user, isAuthenticated } = useAuthStore();

  return (
    <View style={styles.container}>
      <View style={styles.logoRow}>
        <View style={styles.logoIcon}>
          <Ionicons name="game-controller" size={18} color="#021118" />
        </View>
        <View>
          <Text style={styles.logoTitle}>DTEAM</Text>
          <Text style={styles.logoSubtitle}>WEB3 GAMING</Text>
        </View>
      </View>

      <View style={styles.rightActions}>
        {onToggleDemoMode && (
          <TouchableOpacity
            style={[styles.demoBadge, isDemoMode && styles.demoBadgeActive]}
            onPress={onToggleDemoMode}
            activeOpacity={0.7}
          >
            <View style={[styles.demoDot, isDemoMode && styles.demoDotActive]} />
            <Text style={[styles.demoText, isDemoMode && styles.demoTextActive]}>
              {isDemoMode ? 'ДЕМО' : 'API'}
            </Text>
          </TouchableOpacity>
        )}

        {isAuthenticated ? (
          <TouchableOpacity
            style={styles.userChip}
            onPress={onNavigateProfile}
            activeOpacity={0.7}
          >
            <View style={styles.avatarCircle}>
              <Text style={styles.avatarLetter}>
                {(user?.username || 'U').charAt(0).toUpperCase()}
              </Text>
            </View>
            <Text style={styles.userName} numberOfLines={1}>
              {user?.username}
            </Text>
          </TouchableOpacity>
        ) : onNavigateLogin ? (
          <TouchableOpacity
            style={styles.loginBtn}
            onPress={onNavigateLogin}
            activeOpacity={0.7}
          >
            <Ionicons name="log-in-outline" size={16} color={colors.primary} />
            <Text style={styles.loginBtnText}>Вхід</Text>
          </TouchableOpacity>
        ) : null}
      </View>
    </View>
  );
};

const styles = StyleSheet.create({
  container: {
    height: 56,
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    paddingHorizontal: 16,
    backgroundColor: colors.surfaceCard,
    borderBottomWidth: 1,
    borderBottomColor: colors.border,
  },
  logoRow: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 10,
  },
  logoIcon: {
    width: 32,
    height: 32,
    borderRadius: 10,
    backgroundColor: colors.primary,
    alignItems: 'center',
    justifyContent: 'center',
    shadowColor: colors.primary,
    shadowOffset: { width: 0, height: 0 },
    shadowOpacity: 0.6,
    shadowRadius: 6,
    elevation: 4,
  },
  logoTitle: {
    fontSize: 16,
    fontWeight: '900',
    color: colors.text,
    letterSpacing: 1.5,
  },
  logoSubtitle: {
    fontSize: 8,
    fontWeight: '800',
    color: colors.primary,
    letterSpacing: 1,
    fontFamily: 'monospace',
  },
  rightActions: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 10,
  },
  demoBadge: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 5,
    paddingHorizontal: 8,
    paddingVertical: 4,
    borderRadius: 8,
    backgroundColor: colors.background,
    borderWidth: 1,
    borderColor: colors.border,
  },
  demoBadgeActive: {
    borderColor: colors.accentAmber,
    backgroundColor: 'rgba(245, 158, 11, 0.1)',
  },
  demoDot: {
    width: 6,
    height: 6,
    borderRadius: 3,
    backgroundColor: colors.accentEmerald,
  },
  demoDotActive: {
    backgroundColor: colors.accentAmber,
  },
  demoText: {
    fontSize: 10,
    fontWeight: '800',
    color: colors.textDim,
    fontFamily: 'monospace',
  },
  demoTextActive: {
    color: colors.accentAmber,
  },
  userChip: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 6,
    backgroundColor: 'rgba(34, 211, 238, 0.1)',
    borderWidth: 1,
    borderColor: 'rgba(34, 211, 238, 0.3)',
    paddingHorizontal: 8,
    paddingVertical: 4,
    borderRadius: 12,
  },
  avatarCircle: {
    width: 22,
    height: 22,
    borderRadius: 11,
    backgroundColor: colors.primary,
    alignItems: 'center',
    justifyContent: 'center',
  },
  avatarLetter: {
    color: '#021118',
    fontSize: 11,
    fontWeight: '900',
  },
  userName: {
    fontSize: 12,
    fontWeight: '700',
    color: colors.text,
    maxWidth: 90,
  },
  loginBtn: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 5,
    backgroundColor: 'rgba(34, 211, 238, 0.1)',
    borderWidth: 1,
    borderColor: 'rgba(34, 211, 238, 0.3)',
    paddingHorizontal: 12,
    paddingVertical: 6,
    borderRadius: 10,
  },
  loginBtnText: {
    fontSize: 12,
    fontWeight: '800',
    color: colors.primary,
  },
});
