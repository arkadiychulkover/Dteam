import React, { useState, useEffect } from 'react';
import {
  View,
  Text,
  TouchableOpacity,
  StyleSheet,
  Dimensions,
} from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import type { Game } from '../types';
import { BackendImage } from './BackendImage';
import { formatPrice, formatBasePrice } from '../utils/formatters';
import { theme } from '../styles/theme';

interface FeaturedCarouselProps {
  games: Game[];
  onPress: (game: Game) => void;
}

const { width: SCREEN_WIDTH } = Dimensions.get('window');

export const FeaturedCarousel: React.FC<FeaturedCarouselProps> = ({ games, onPress }) => {
  const [currentIndex, setCurrentIndex] = useState(0);

  const featuredGames = games.slice(0, 5);

  useEffect(() => {
    if (featuredGames.length <= 1) return;
    const interval = setInterval(() => {
      setCurrentIndex((prev) => (prev + 1) % featuredGames.length);
    }, 5000);
    return () => clearInterval(interval);
  }, [featuredGames.length]);

  if (featuredGames.length === 0) return null;

  const activeGame = featuredGames[currentIndex] || featuredGames[0];
  const hasDiscount = (activeGame.discountPercentage || 0) > 0;
  const imageSource = activeGame.headerImageUrl || activeGame.coverImageUrl;

  const nextSlide = () => {
    setCurrentIndex((prev) => (prev + 1) % featuredGames.length);
  };

  const prevSlide = () => {
    setCurrentIndex((prev) => (prev - 1 + featuredGames.length) % featuredGames.length);
  };

  return (
    <View style={styles.container}>
      <TouchableOpacity
        style={styles.bannerWrapper}
        activeOpacity={0.9}
        onPress={() => onPress(activeGame)}
      >
        <BackendImage
          src={imageSource}
          style={styles.bannerImage}
          resizeMode="cover"
        />

        <View style={styles.gradientOverlay} />

        {/* Featured Tag & Discount */}
        <View style={styles.topBadgesRow}>
          <View style={styles.featuredBadge}>
            <Ionicons name="sparkles" size={12} color="#000" />
            <Text style={styles.featuredText}>ПОПУЛЯРНЕ</Text>
          </View>
          {hasDiscount && (
            <View style={styles.discountBadge}>
              <Text style={styles.discountText}>-{activeGame.discountPercentage}%</Text>
            </View>
          )}
        </View>

        {/* Carousel controls */}
        {featuredGames.length > 1 && (
          <View style={styles.arrowControls}>
            <TouchableOpacity
              style={styles.arrowBtn}
              onPress={(e) => {
                e.stopPropagation?.();
                prevSlide();
              }}
              activeOpacity={0.7}
            >
              <Ionicons name="chevron-back" size={18} color="#fff" />
            </TouchableOpacity>

            <TouchableOpacity
              style={styles.arrowBtn}
              onPress={(e) => {
                e.stopPropagation?.();
                nextSlide();
              }}
              activeOpacity={0.7}
            >
              <Ionicons name="chevron-forward" size={18} color="#fff" />
            </TouchableOpacity>
          </View>
        )}

        {/* Bottom details */}
        <View style={styles.bottomContent}>
          <Text style={styles.title} numberOfLines={1}>
            {activeGame.title}
          </Text>

          {activeGame.shortDescription ? (
            <Text style={styles.subtitle} numberOfLines={2}>
              {activeGame.shortDescription}
            </Text>
          ) : null}

          <View style={styles.priceRow}>
            <View style={styles.priceBlock}>
              {hasDiscount && (
                <Text style={styles.originalPrice}>
                  {formatBasePrice(activeGame.priceInNanoTons)}
                </Text>
              )}
              <Text style={[styles.priceText, hasDiscount && styles.discountedPrice]}>
                {formatPrice(activeGame.priceInNanoTons, activeGame.discountPercentage)}
              </Text>
            </View>

            <View style={styles.ctaButton}>
              <Text style={styles.ctaText}>Переглянути</Text>
              <Ionicons name="arrow-forward" size={12} color="#000" />
            </View>
          </View>
        </View>
      </TouchableOpacity>

      {/* Pagination indicators */}
      {featuredGames.length > 1 && (
        <View style={styles.paginationRow}>
          {featuredGames.map((_, idx) => (
            <TouchableOpacity
              key={idx}
              onPress={() => setCurrentIndex(idx)}
              style={[
                styles.dot,
                currentIndex === idx ? styles.activeDot : styles.inactiveDot,
              ]}
            />
          ))}
        </View>
      )}
    </View>
  );
};

const styles = StyleSheet.create({
  container: {
    marginBottom: 20,
  },
  bannerWrapper: {
    height: 220,
    borderRadius: theme.borderRadius.xl,
    overflow: 'hidden',
    position: 'relative',
    backgroundColor: theme.colors.surface,
    borderWidth: 1,
    borderColor: theme.colors.border,
  },
  bannerImage: {
    width: '100%',
    height: '100%',
  },
  gradientOverlay: {
    ...StyleSheet.absoluteFill,
    backgroundColor: 'rgba(3, 13, 18, 0.55)',
  },
  topBadgesRow: {
    position: 'absolute',
    top: 12,
    left: 12,
    right: 12,
    flexDirection: 'row',
    justifyContent: 'space-between',
    zIndex: 2,
  },
  featuredBadge: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 4,
    backgroundColor: theme.colors.primary,
    paddingHorizontal: 8,
    paddingVertical: 4,
    borderRadius: 6,
  },
  featuredText: {
    color: '#000',
    fontSize: 10,
    fontWeight: '900',
    letterSpacing: 0.5,
  },
  discountBadge: {
    backgroundColor: theme.colors.discount,
    paddingHorizontal: 8,
    paddingVertical: 4,
    borderRadius: 6,
  },
  discountText: {
    color: '#000',
    fontSize: 11,
    fontWeight: '900',
  },
  arrowControls: {
    position: 'absolute',
    top: '40%',
    left: 8,
    right: 8,
    flexDirection: 'row',
    justifyContent: 'space-between',
    zIndex: 3,
  },
  arrowBtn: {
    width: 32,
    height: 32,
    borderRadius: 16,
    backgroundColor: 'rgba(6, 24, 32, 0.75)',
    justifyContent: 'center',
    alignItems: 'center',
    borderWidth: 0.5,
    borderColor: theme.colors.borderSubtle,
  },
  bottomContent: {
    position: 'absolute',
    bottom: 0,
    left: 0,
    right: 0,
    padding: 14,
    backgroundColor: 'rgba(3, 13, 18, 0.88)',
    borderTopWidth: 1,
    borderTopColor: 'rgba(255, 255, 255, 0.05)',
  },
  title: {
    fontSize: 18,
    fontWeight: '900',
    color: theme.colors.text,
  },
  subtitle: {
    fontSize: 11,
    color: theme.colors.textSecondary,
    marginTop: 2,
    lineHeight: 15,
  },
  priceRow: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    marginTop: 8,
  },
  priceBlock: {
    flexDirection: 'row',
    alignItems: 'baseline',
    gap: 6,
  },
  originalPrice: {
    fontSize: 11,
    color: theme.colors.textDim,
    textDecorationLine: 'line-through',
  },
  priceText: {
    fontSize: 15,
    fontWeight: '900',
    color: theme.colors.text,
  },
  discountedPrice: {
    color: theme.colors.secondary,
  },
  ctaButton: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 4,
    backgroundColor: theme.colors.secondary,
    paddingHorizontal: 12,
    paddingVertical: 6,
    borderRadius: theme.borderRadius.sm,
  },
  ctaText: {
    fontSize: 11,
    fontWeight: '800',
    color: '#000',
  },
  paginationRow: {
    flexDirection: 'row',
    justifyContent: 'center',
    alignItems: 'center',
    gap: 6,
    marginTop: 10,
  },
  dot: {
    height: 4,
    borderRadius: 2,
  },
  activeDot: {
    width: 22,
    backgroundColor: theme.colors.primary,
  },
  inactiveDot: {
    width: 8,
    backgroundColor: theme.colors.borderSubtle,
  },
});
