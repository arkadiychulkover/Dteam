import React from 'react';
import { View, Text, StyleSheet, TouchableOpacity } from 'react-native';
import { colors } from '../../theme/colors';
import { Badge } from '../../types';
import { BackendImage } from '../BackendImage';
import { Ionicons } from '@expo/vector-icons';

interface BadgeCardProps {
  badge: Badge;
  onPress: (badge: Badge) => void;
}

export const getRarityConfig = (rarity: Badge['rarity']) => {
  switch (rarity) {
    case 'mythic':
      return {
        label: 'Міфічний',
        borderColor: colors.accentRose,
        bgColor: 'rgba(244, 63, 94, 0.12)',
        textColor: colors.accentRose,
      };
    case 'legendary':
      return {
        label: 'Легендарний',
        borderColor: colors.accentAmber,
        bgColor: 'rgba(245, 158, 11, 0.12)',
        textColor: colors.accentAmber,
      };
    case 'epic':
      return {
        label: 'Епічний',
        borderColor: colors.accentPurple,
        bgColor: 'rgba(168, 85, 247, 0.12)',
        textColor: colors.accentPurple,
      };
    case 'rare':
      return {
        label: 'Рідкісний',
        borderColor: colors.primary,
        bgColor: 'rgba(34, 211, 238, 0.12)',
        textColor: colors.primary,
      };
    default:
      return {
        label: 'Звичайний',
        borderColor: colors.borderLight,
        bgColor: 'rgba(148, 163, 184, 0.1)',
        textColor: colors.textMuted,
      };
  }
};

export const BadgeCard: React.FC<BadgeCardProps> = ({ badge, onPress }) => {
  const rarityCfg = getRarityConfig(badge.rarity);

  return (
    <TouchableOpacity
      style={[
        styles.card,
        {
          borderColor: rarityCfg.borderColor,
          backgroundColor: rarityCfg.bgColor,
        },
      ]}
      onPress={() => onPress(badge)}
      activeOpacity={0.8}
    >
      <View style={styles.topRow}>
        <Text style={styles.tokenIdText}>#{badge.tokenId || '001'}</Text>
        <View style={[styles.rarityBadge, { borderColor: rarityCfg.borderColor }]}>
          <Text style={[styles.rarityText, { color: rarityCfg.textColor }]}>
            {rarityCfg.label}
          </Text>
        </View>
      </View>

      <View style={styles.imageContainer}>
        {badge.imageUrl ? (
          <BackendImage
            src={badge.imageUrl}
            style={styles.badgeImage}
            resizeMode="cover"
            fallbackText={badge.title}
          />
        ) : (
          <View style={styles.placeholderIcon}>
            <Ionicons name="sparkles" size={24} color={rarityCfg.textColor} />
          </View>
        )}
      </View>

      <Text style={styles.title} numberOfLines={1}>
        {badge.title}
      </Text>

      <Text style={styles.dateText}>
        {badge.earnedDate}
      </Text>
    </TouchableOpacity>
  );
};

const styles = StyleSheet.create({
  card: {
    width: 140,
    borderRadius: 16,
    padding: 10,
    borderWidth: 1.5,
    marginRight: 10,
  },
  topRow: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    marginBottom: 8,
  },
  tokenIdText: {
    fontSize: 10,
    fontWeight: '900',
    color: colors.textDim,
    fontFamily: 'monospace',
  },
  rarityBadge: {
    paddingHorizontal: 6,
    paddingVertical: 1,
    borderRadius: 6,
    borderWidth: 0.8,
  },
  rarityText: {
    fontSize: 9,
    fontWeight: '800',
    textTransform: 'uppercase',
  },
  imageContainer: {
    width: '100%',
    aspectRatio: 1,
    borderRadius: 12,
    overflow: 'hidden',
    backgroundColor: '#021118',
    marginBottom: 8,
    alignItems: 'center',
    justifyContent: 'center',
  },
  badgeImage: {
    width: '100%',
    height: '100%',
  },
  placeholderIcon: {
    alignItems: 'center',
    justifyContent: 'center',
  },
  title: {
    fontSize: 12,
    fontWeight: '800',
    color: colors.text,
    marginBottom: 2,
  },
  dateText: {
    fontSize: 10,
    color: colors.textDim,
    fontFamily: 'monospace',
  },
});
