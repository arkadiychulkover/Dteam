import React from 'react';
import {
  View,
  Text,
  TouchableOpacity,
  StyleSheet,
} from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import type { Game } from '../types';
import { BackendImage } from './BackendImage';
import { formatPrice, formatBasePrice } from '../utils/formatters';
import { theme } from '../styles/theme';

interface GameCardProps {
  game: Game;
  onPress: (game: Game) => void;
}

export const GameCard: React.FC<GameCardProps> = ({ game, onPress }) => {
  const hasDiscount = (game.discountPercentage || 0) > 0;
  const imageSource = game.coverImageUrl || game.headerImageUrl;

  return (
    <TouchableOpacity
      style={styles.card}
      activeOpacity={0.8}
      onPress={() => onPress(game)}
    >
      <View style={styles.imageWrapper}>
        <BackendImage
          src={imageSource}
          style={styles.image}
          resizeMode="cover"
        />
        {hasDiscount && (
          <View style={styles.discountBadge}>
            <Text style={styles.discountText}>-{game.discountPercentage}%</Text>
          </View>
        )}
      </View>

      <View style={styles.content}>
        <View style={styles.headerRow}>
          <Text style={styles.title} numberOfLines={1}>
            {game.title}
          </Text>
          <View style={styles.ratingBadge}>
            <Ionicons name="star" size={11} color={theme.colors.star} />
            <Text style={styles.ratingText}>
              {game.averageRating > 0 ? game.averageRating.toFixed(1) : 'NEW'}
            </Text>
          </View>
        </View>

        {game.shortDescription ? (
          <Text style={styles.description} numberOfLines={2}>
            {game.shortDescription}
          </Text>
        ) : null}

        <View style={styles.genresRow}>
          {game.genres?.slice(0, 3).map((genre, idx) => (
            <View key={idx} style={styles.genreChip}>
              <Text style={styles.genreChipText}>{genre}</Text>
            </View>
          ))}
        </View>

        <View style={styles.footerRow}>
          <View style={styles.platformsRow}>
            {game.platforms?.includes('Windows') && (
              <Ionicons name="logo-windows" size={14} color={theme.colors.textDim} style={styles.platformIcon} />
            )}
            {game.platforms?.includes('MacOS') && (
              <Ionicons name="logo-apple" size={14} color={theme.colors.textDim} style={styles.platformIcon} />
            )}
            {game.isDlc && (
              <View style={styles.dlcBadge}>
                <Text style={styles.dlcText}>DLC</Text>
              </View>
            )}
          </View>

          <View style={styles.priceContainer}>
            {hasDiscount && (
              <Text style={styles.originalPrice}>
                {formatBasePrice(game.priceInNanoTons)}
              </Text>
            )}
            <Text style={[styles.price, hasDiscount && styles.discountedPrice]}>
              {formatPrice(game.priceInNanoTons, game.discountPercentage)}
            </Text>
          </View>
        </View>
      </View>
    </TouchableOpacity>
  );
};

const styles = StyleSheet.create({
  card: {
    flexDirection: 'row',
    backgroundColor: theme.colors.card,
    borderRadius: theme.borderRadius.lg,
    borderWidth: 1,
    borderColor: theme.colors.borderSubtle,
    marginBottom: 12,
    overflow: 'hidden',
  },
  imageWrapper: {
    width: 105,
    minHeight: 125,
    position: 'relative',
    backgroundColor: theme.colors.surface,
  },
  image: {
    width: '100%',
    height: '100%',
  },
  discountBadge: {
    position: 'absolute',
    top: 6,
    left: 6,
    backgroundColor: theme.colors.discount,
    paddingHorizontal: 5,
    paddingVertical: 2,
    borderRadius: 4,
    zIndex: 2,
  },
  discountText: {
    color: '#000',
    fontWeight: '900',
    fontSize: 10,
  },
  content: {
    flex: 1,
    padding: 10,
    justifyContent: 'space-between',
  },
  headerRow: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    gap: 8,
  },
  title: {
    fontSize: 14,
    fontWeight: '700',
    color: theme.colors.text,
    flex: 1,
  },
  ratingBadge: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 2,
    backgroundColor: 'rgba(251, 191, 36, 0.12)',
    paddingHorizontal: 5,
    paddingVertical: 2,
    borderRadius: 4,
  },
  ratingText: {
    fontSize: 10,
    fontWeight: '700',
    color: theme.colors.star,
  },
  description: {
    fontSize: 11,
    color: theme.colors.textMuted,
    lineHeight: 15,
    marginVertical: 4,
  },
  genresRow: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    gap: 4,
    marginVertical: 4,
  },
  genreChip: {
    backgroundColor: theme.colors.badgeBg,
    paddingHorizontal: 6,
    paddingVertical: 2,
    borderRadius: 4,
    borderWidth: 0.5,
    borderColor: theme.colors.badgeBorder,
  },
  genreChipText: {
    fontSize: 9,
    color: theme.colors.primary,
    fontWeight: '600',
  },
  footerRow: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    marginTop: 4,
  },
  platformsRow: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 6,
  },
  platformIcon: {
    marginRight: 2,
  },
  dlcBadge: {
    backgroundColor: '#3b82f6',
    paddingHorizontal: 4,
    paddingVertical: 1,
    borderRadius: 3,
  },
  dlcText: {
    fontSize: 8,
    fontWeight: '800',
    color: '#fff',
  },
  priceContainer: {
    alignItems: 'flex-end',
  },
  originalPrice: {
    fontSize: 10,
    color: theme.colors.textDim,
    textDecorationLine: 'line-through',
  },
  price: {
    fontSize: 13,
    fontWeight: '800',
    color: theme.colors.text,
  },
  discountedPrice: {
    color: theme.colors.secondary,
  },
});
