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

interface SpecialOfferCardProps {
  game: Game;
  onPress: (game: Game) => void;
}

export const SpecialOfferCard: React.FC<SpecialOfferCardProps> = ({ game, onPress }) => {
  const hasDiscount = (game.discountPercentage || 0) > 0;
  const imageSource = game.headerImageUrl || game.coverImageUrl;

  return (
    <TouchableOpacity
      style={styles.card}
      activeOpacity={0.85}
      onPress={() => onPress(game)}
    >
      <View style={styles.imageContainer}>
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

      <View style={styles.infoContainer}>
        <Text style={styles.title} numberOfLines={1}>
          {game.title}
        </Text>

        <View style={styles.metaRow}>
          <View style={styles.ratingBadge}>
            <Ionicons name="star" size={12} color={theme.colors.star} />
            <Text style={styles.ratingText}>
              {game.averageRating > 0 ? game.averageRating.toFixed(1) : 'NEW'}
            </Text>
          </View>

          {game.genres && game.genres.length > 0 && (
            <Text style={styles.genreText} numberOfLines={1}>
              {game.genres.slice(0, 2).join(' • ')}
            </Text>
          )}
        </View>

        <View style={styles.priceRow}>
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
    </TouchableOpacity>
  );
};

const styles = StyleSheet.create({
  card: {
    width: 250,
    backgroundColor: theme.colors.card,
    borderRadius: theme.borderRadius.lg,
    borderWidth: 1,
    borderColor: theme.colors.borderSubtle,
    overflow: 'hidden',
    marginRight: 14,
  },
  imageContainer: {
    width: '100%',
    height: 135,
    position: 'relative',
    backgroundColor: theme.colors.surface,
  },
  image: {
    width: '100%',
    height: '100%',
  },
  discountBadge: {
    position: 'absolute',
    top: 8,
    right: 8,
    backgroundColor: theme.colors.discount,
    paddingHorizontal: 8,
    paddingVertical: 3,
    borderRadius: 6,
    zIndex: 2,
  },
  discountText: {
    color: '#000',
    fontWeight: '900',
    fontSize: 11,
  },
  infoContainer: {
    padding: 12,
  },
  title: {
    fontSize: 14,
    fontWeight: '700',
    color: theme.colors.text,
    marginBottom: 6,
  },
  metaRow: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 8,
    marginBottom: 8,
  },
  ratingBadge: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 3,
    backgroundColor: 'rgba(251, 191, 36, 0.12)',
    paddingHorizontal: 6,
    paddingVertical: 2,
    borderRadius: 4,
  },
  ratingText: {
    fontSize: 11,
    fontWeight: '700',
    color: theme.colors.star,
  },
  genreText: {
    fontSize: 11,
    color: theme.colors.textMuted,
    flex: 1,
  },
  priceRow: {
    flexDirection: 'row',
    alignItems: 'baseline',
    gap: 6,
  },
  originalPrice: {
    fontSize: 11,
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
