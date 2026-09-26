import React from 'react';
import { View, Text, StyleSheet } from 'react-native';
import { colors } from '../../theme/colors';
import { ProfileLevelHexagon } from './ProfileLevelHexagon';
import { Ionicons } from '@expo/vector-icons';

interface ProfileLevelCardProps {
  level: number;
  currentXp: number;
  nextLevelXp: number;
}

export const ProfileLevelCard: React.FC<ProfileLevelCardProps> = ({
  level,
  currentXp,
  nextLevelXp,
}) => {
  const percent = Math.min(100, Math.round((currentXp / Math.max(1, nextLevelXp)) * 100));

  return (
    <View style={styles.card}>
      <View style={styles.topRow}>
        <ProfileLevelHexagon level={level} size={46} />
        <View style={styles.levelInfo}>
          <View style={styles.titleRow}>
            <Text style={styles.levelTitle}>Рівень {level}</Text>
            <View style={styles.statusBadge}>
              <Ionicons name="shield-checkmark" size={12} color={colors.accentEmerald} />
              <Text style={styles.statusBadgeText}>Верифіковано</Text>
            </View>
          </View>
          <Text style={styles.levelSubtitle}>
            {level >= 40 ? 'Кібер-Ветеран' : level >= 20 ? 'Досвідчений Геймер' : 'Новачок'}
          </Text>
        </View>

        <View style={styles.xpTextContainer}>
          <Text style={styles.xpValue}>{currentXp}</Text>
          <Text style={styles.xpMax}>/{nextLevelXp} XP</Text>
        </View>
      </View>

      <View style={styles.progressBarTrack}>
        <View style={[styles.progressBarFill, { width: `${percent}%` }]} />
      </View>

      <View style={styles.bottomRow}>
        <Text style={styles.percentText}>{percent}% до наступного рівня</Text>
        <Text style={styles.rewardText}>
          Залишилось {Math.max(0, nextLevelXp - currentXp)} XP
        </Text>
      </View>
    </View>
  );
};

const styles = StyleSheet.create({
  card: {
    backgroundColor: colors.surfaceCard,
    borderRadius: 20,
    padding: 16,
    borderWidth: 1,
    borderColor: colors.border,
    marginBottom: 16,
  },
  topRow: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 12,
  },
  levelInfo: {
    flex: 1,
  },
  titleRow: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 8,
  },
  levelTitle: {
    fontSize: 16,
    fontWeight: '800',
    color: colors.text,
  },
  statusBadge: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 4,
    backgroundColor: 'rgba(16, 185, 129, 0.15)',
    paddingHorizontal: 6,
    paddingVertical: 2,
    borderRadius: 6,
  },
  statusBadgeText: {
    fontSize: 10,
    fontWeight: '700',
    color: colors.accentEmerald,
  },
  levelSubtitle: {
    fontSize: 12,
    color: colors.textMuted,
    marginTop: 2,
  },
  xpTextContainer: {
    flexDirection: 'row',
    alignItems: 'baseline',
  },
  xpValue: {
    fontSize: 14,
    fontWeight: '800',
    color: colors.primary,
    fontFamily: 'monospace',
  },
  xpMax: {
    fontSize: 11,
    color: colors.textDim,
    fontFamily: 'monospace',
  },
  progressBarTrack: {
    height: 8,
    backgroundColor: colors.background,
    borderRadius: 4,
    marginTop: 14,
    overflow: 'hidden',
    borderWidth: 0.5,
    borderColor: colors.borderLight,
  },
  progressBarFill: {
    height: '100%',
    backgroundColor: colors.primary,
    borderRadius: 4,
    shadowColor: colors.primary,
    shadowOffset: { width: 0, height: 0 },
    shadowOpacity: 0.8,
    shadowRadius: 4,
  },
  bottomRow: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    marginTop: 8,
  },
  percentText: {
    fontSize: 11,
    color: colors.textDim,
    fontWeight: '600',
  },
  rewardText: {
    fontSize: 11,
    color: colors.accentAmber,
    fontWeight: '700',
  },
});
