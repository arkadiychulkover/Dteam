import React, { useState, useEffect } from 'react';
import {
  View,
  Text,
  ScrollView,
  TouchableOpacity,
  StyleSheet,
  Alert,
  StatusBar,
  Dimensions,
  ActivityIndicator,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { Ionicons } from '@expo/vector-icons';
import type { Game } from '../types';
import { gamesService } from '../services/gamesService';
import { BackendImage } from '../components/BackendImage';
import {
  formatPrice,
  formatBasePrice,
  formatDate,
  formatBytes,
} from '../utils/formatters';
import { theme } from '../styles/theme';

interface GameDetailsScreenProps {
  game?: Game;
  onBack?: () => void;
  onSelectGame?: (game: Game) => void;
  navigation?: any;
  route?: { params?: { game: Game } };
}

const { width: SCREEN_WIDTH } = Dimensions.get('window');

export const GameDetailsScreen: React.FC<GameDetailsScreenProps> = (props) => {
  const initialGame = props.route?.params?.game || props.game;
  if (!initialGame) {
    return (
      <SafeAreaView style={styles.safeArea}>
        <View style={styles.loaderCenter}>
          <Text style={styles.emptyText}>Гру не знайдено</Text>
        </View>
      </SafeAreaView>
    );
  }

  const handleBack = () => {
    if (props.navigation?.canGoBack?.()) {
      props.navigation.goBack();
    } else if (props.onBack) {
      props.onBack();
    }
  };

  const handleSelectGame = (g: Game) => {
    if (props.navigation?.push) {
      props.navigation.push('GameDetails', { game: g });
    } else if (props.onSelectGame) {
      props.onSelectGame(g);
    }
  };
  const [game, setGame] = useState<Game>(initialGame);
  const [selectedMediaUrl, setSelectedMediaUrl] = useState<string | null>(
    initialGame.headerImageUrl || initialGame.coverImageUrl || null
  );
  const [isLoadingDetails, setIsLoadingDetails] = useState(false);
  const [isWishlisted, setIsWishlisted] = useState(false);

  useEffect(() => {
    let isMounted = true;
    setIsLoadingDetails(true);

    gamesService
      .getGameById(initialGame.id)
      .then((fullGame) => {
        if (isMounted && fullGame) {
          setGame(fullGame);
          if (!selectedMediaUrl) {
            setSelectedMediaUrl(fullGame.headerImageUrl || fullGame.coverImageUrl || null);
          }
        }
      })
      .catch((err) => {
        console.warn('[GameDetails] Failed to refresh game details:', err);
      })
      .finally(() => {
        if (isMounted) setIsLoadingDetails(false);
      });

    return () => {
      isMounted = false;
    };
  }, [initialGame.id]);

  const hasDiscount = (game.discountPercentage || 0) > 0;

  const mediaList: string[] = [];
  if (game.headerImageUrl) mediaList.push(game.headerImageUrl);
  if (game.coverImageUrl && !mediaList.includes(game.coverImageUrl)) {
    mediaList.push(game.coverImageUrl);
  }
  if (game.screenshotUrls) {
    game.screenshotUrls.forEach((url) => {
      if (url && !mediaList.includes(url)) mediaList.push(url);
    });
  }

  const handleBuyPress = () => {
    Alert.alert(
      'Купівля гри',
      `Оплата гри "${game.title}" буде доступна після підключення TON Wallet у наступному релізі.`,
      [{ text: 'Зрозуміло', style: 'default' }]
    );
  };

  const handleWishlistToggle = () => {
    setIsWishlisted(!isWishlisted);
  };

  return (
    <SafeAreaView style={styles.safeArea} edges={['top', 'left', 'right']}>
      <StatusBar barStyle="light-content" backgroundColor={theme.colors.background} />

      <View style={styles.navBar}>
        <TouchableOpacity
          style={styles.backButton}
          onPress={handleBack}
          activeOpacity={0.7}
        >
          <Ionicons name="arrow-back" size={20} color={theme.colors.text} />
          <Text style={styles.backButtonText}>Назад</Text>
        </TouchableOpacity>

        <Text style={styles.navTitle} numberOfLines={1}>
          {game.title}
        </Text>

        <TouchableOpacity
          style={styles.wishlistHeaderBtn}
          onPress={handleWishlistToggle}
          activeOpacity={0.7}
        >
          <Ionicons
            name={isWishlisted ? 'heart' : 'heart-outline'}
            size={22}
            color={isWishlisted ? '#f43f5e' : theme.colors.textMuted}
          />
        </TouchableOpacity>
      </View>

      <ScrollView
        showsVerticalScrollIndicator={false}
        contentContainerStyle={styles.scrollContent}
      >
        <View style={styles.mainMediaWrapper}>
          <BackendImage
            src={selectedMediaUrl}
            style={styles.mainMediaImage}
            resizeMode="cover"
          />

          {hasDiscount && (
            <View style={styles.discountBadge}>
              <Text style={styles.discountText}>-{game.discountPercentage}%</Text>
            </View>
          )}

          {isLoadingDetails && (
            <View style={styles.detailsLoader}>
              <ActivityIndicator size="small" color={theme.colors.primary} />
            </View>
          )}
        </View>

        {mediaList.length > 1 && (
          <ScrollView
            horizontal
            showsHorizontalScrollIndicator={false}
            contentContainerStyle={styles.thumbnailsScroll}
          >
            {mediaList.map((url, index) => {
              const isSelected = selectedMediaUrl === url;
              return (
                <TouchableOpacity
                  key={index}
                  style={[
                    styles.thumbnailItem,
                    isSelected && styles.thumbnailItemActive,
                  ]}
                  onPress={() => setSelectedMediaUrl(url)}
                  activeOpacity={0.8}
                >
                  <BackendImage
                    src={url}
                    style={styles.thumbnailImage}
                    resizeMode="cover"
                  />
                </TouchableOpacity>
              );
            })}
          </ScrollView>
        )}

        <View style={styles.infoBlock}>
          <Text style={styles.gameTitle}>{game.title}</Text>

          <View style={styles.metaRow}>
            <View style={styles.ratingBadge}>
              <Ionicons name="star" size={14} color={theme.colors.star} />
              <Text style={styles.ratingValue}>
                {game.averageRating > 0 ? game.averageRating.toFixed(2) : 'NEW'}
              </Text>
              {game.reviewsCount > 0 && (
                <Text style={styles.reviewsCountText}>({game.reviewsCount})</Text>
              )}
            </View>

            <View style={styles.downloadsBadge}>
              <Ionicons name="download-outline" size={14} color={theme.colors.textMuted} />
              <Text style={styles.downloadsText}>
                {game.downloadCount.toLocaleString('uk-UA')} завантажень
              </Text>
            </View>

            {game.version ? (
              <View style={styles.versionBadge}>
                <Text style={styles.versionText}>v{game.version}</Text>
              </View>
            ) : null}
          </View>

          {game.genres && game.genres.length > 0 && (
            <View style={styles.genresRow}>
              {game.genres.map((genre, idx) => (
                <View key={idx} style={styles.genreChip}>
                  <Text style={styles.genreChipText}>{genre}</Text>
                </View>
              ))}
            </View>
          )}

          <View style={styles.platformsRow}>
            <Text style={styles.platformLabel}>Платформи:</Text>
            {game.platforms?.includes('Windows') && (
              <View style={styles.platformBadge}>
                <Ionicons name="logo-windows" size={13} color={theme.colors.primary} />
                <Text style={styles.platformBadgeText}>Windows</Text>
              </View>
            )}
            {game.platforms?.includes('MacOS') && (
              <View style={styles.platformBadge}>
                <Ionicons name="logo-apple" size={13} color={theme.colors.primary} />
                <Text style={styles.platformBadgeText}>macOS</Text>
              </View>
            )}
            {game.sizeInBytes > 0 && (
              <Text style={styles.sizeText}>Розмір: {formatBytes(game.sizeInBytes)}</Text>
            )}
          </View>
        </View>

        <View style={styles.buyCard}>
          <View style={styles.buyCardHeader}>
            <View>
              <Text style={styles.buyCardLabel}>Ціна гри:</Text>
              <View style={styles.priceRow}>
                {hasDiscount && (
                  <Text style={styles.originalPriceText}>
                    {formatBasePrice(game.priceInNanoTons)}
                  </Text>
                )}
                <Text style={[styles.finalPriceText, hasDiscount && styles.discountedFinalPrice]}>
                  {formatPrice(game.priceInNanoTons, game.discountPercentage)}
                </Text>
              </View>
            </View>

            {hasDiscount && (
              <View style={styles.discountCardBadge}>
                <Text style={styles.discountCardText}>-{game.discountPercentage}%</Text>
              </View>
            )}
          </View>

          <View style={styles.actionsContainer}>
            <TouchableOpacity
              style={styles.buyButton}
              onPress={handleBuyPress}
              activeOpacity={0.8}
            >
              <Ionicons name="cart" size={18} color="#000" />
              <Text style={styles.buyButtonText}>Придбати гру</Text>
            </TouchableOpacity>

            <TouchableOpacity
              style={[
                styles.wishlistButton,
                isWishlisted && styles.wishlistButtonActive,
              ]}
              onPress={handleWishlistToggle}
              activeOpacity={0.7}
            >
              <Ionicons
                name={isWishlisted ? 'heart' : 'heart-outline'}
                size={18}
                color={isWishlisted ? '#f43f5e' : theme.colors.primary}
              />
              <Text
                style={[
                  styles.wishlistButtonText,
                  isWishlisted && styles.wishlistButtonTextActive,
                ]}
              >
                {isWishlisted ? 'У списку бажань' : 'До бажаного'}
              </Text>
            </TouchableOpacity>
          </View>

          <View style={styles.blockchainNotice}>
            <Ionicons name="information-circle-outline" size={14} color={theme.colors.textDim} />
            <Text style={styles.blockchainNoticeText}>
              Кнопки активні для демонстрації. Повна Web3 смарт-контракт оплата буде доступна незабаром.
            </Text>
          </View>
        </View>

        <View style={styles.descriptionSection}>
          <Text style={styles.sectionHeading}>Про гру</Text>
          {game.shortDescription ? (
            <View style={styles.shortDescBox}>
              <Text style={styles.shortDescText}>{game.shortDescription}</Text>
            </View>
          ) : null}

          <Text style={styles.fullDescText}>
            {game.description || 'Опис гри наразі відсутній.'}
          </Text>
        </View>

        {game.dlcs && game.dlcs.length > 0 && (
          <View style={styles.dlcSection}>
            <View style={styles.dlcHeaderRow}>
              <Ionicons name="extension-puzzle-outline" size={18} color={theme.colors.primary} />
              <Text style={styles.sectionHeading}>Доповнення та DLC ({game.dlcs.length})</Text>
            </View>

            {game.dlcs.map((dlc) => (
              <TouchableOpacity
                key={dlc.id}
                style={styles.dlcItem}
                onPress={() => handleSelectGame(dlc)}
                activeOpacity={0.8}
              >
                <View style={styles.dlcImageWrap}>
                  <BackendImage
                    src={dlc.coverImageUrl || dlc.headerImageUrl}
                    style={styles.dlcImage}
                    resizeMode="cover"
                  />
                </View>
                <View style={styles.dlcInfo}>
                  <Text style={styles.dlcTitle} numberOfLines={1}>
                    {dlc.title}
                  </Text>
                  <Text style={styles.dlcDesc} numberOfLines={2}>
                    {dlc.shortDescription || dlc.description}
                  </Text>
                  <Text style={styles.dlcPrice}>
                    {formatPrice(dlc.priceInNanoTons, dlc.discountPercentage)}
                  </Text>
                </View>
              </TouchableOpacity>
            ))}
          </View>
        )}

        {game.tags && game.tags.length > 0 && (
          <View style={styles.tagsSection}>
            <Text style={styles.sectionHeading}>Теги гри</Text>
            <View style={styles.tagsContainer}>
              {game.tags.map((tag, idx) => (
                <View key={idx} style={styles.tagBadge}>
                  <Text style={styles.tagText}>#{tag}</Text>
                </View>
              ))}
            </View>
          </View>
        )}

        {game.supportedLanguages && game.supportedLanguages.length > 0 && (
          <View style={styles.languagesSection}>
            <Text style={styles.sectionHeading}>Підтримувані мови</Text>
            <View style={styles.langGrid}>
              {game.supportedLanguages.map((lang, idx) => (
                <View key={idx} style={styles.langItem}>
                  <Text style={styles.langName}>{lang.language}</Text>
                  <View style={styles.langBadges}>
                    {lang.interface && <Text style={styles.langTag}>Інтерфейс</Text>}
                    {lang.fullAudio && <Text style={styles.langTag}>Озвучення</Text>}
                    {lang.subtitles && <Text style={styles.langTag}>Субтитри</Text>}
                  </View>
                </View>
              ))}
            </View>
          </View>
        )}
      </ScrollView>
    </SafeAreaView>
  );
};

const styles = StyleSheet.create({
  safeArea: {
    flex: 1,
    backgroundColor: theme.colors.background,
  },
  navBar: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    paddingHorizontal: 16,
    paddingVertical: 12,
    borderBottomWidth: 1,
    borderBottomColor: theme.colors.borderSubtle,
    backgroundColor: theme.colors.surface,
  },
  backButton: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 4,
    paddingVertical: 4,
    paddingRight: 8,
  },
  backButtonText: {
    fontSize: 14,
    fontWeight: '700',
    color: theme.colors.text,
  },
  navTitle: {
    fontSize: 15,
    fontWeight: '800',
    color: theme.colors.text,
    flex: 1,
    textAlign: 'center',
    marginHorizontal: 8,
  },
  wishlistHeaderBtn: {
    padding: 4,
  },
  scrollContent: {
    paddingBottom: 40,
  },
  mainMediaWrapper: {
    width: '100%',
    height: 230,
    backgroundColor: theme.colors.surface,
    position: 'relative',
  },
  mainMediaImage: {
    width: '100%',
    height: '100%',
  },
  discountBadge: {
    position: 'absolute',
    top: 12,
    right: 12,
    backgroundColor: theme.colors.discount,
    paddingHorizontal: 10,
    paddingVertical: 5,
    borderRadius: 6,
  },
  discountText: {
    color: '#000',
    fontWeight: '900',
    fontSize: 13,
  },
  detailsLoader: {
    position: 'absolute',
    top: 12,
    left: 12,
    backgroundColor: 'rgba(6, 24, 32, 0.7)',
    borderRadius: 14,
    padding: 6,
  },
  thumbnailsScroll: {
    paddingHorizontal: 16,
    paddingVertical: 12,
    gap: 10,
    backgroundColor: theme.colors.surface,
  },
  thumbnailItem: {
    width: 76,
    height: 48,
    borderRadius: 6,
    overflow: 'hidden',
    borderWidth: 1.5,
    borderColor: 'transparent',
    backgroundColor: theme.colors.card,
  },
  thumbnailItemActive: {
    borderColor: theme.colors.primary,
  },
  thumbnailImage: {
    width: '100%',
    height: '100%',
  },
  infoBlock: {
    padding: 16,
  },
  gameTitle: {
    fontSize: 22,
    fontWeight: '900',
    color: theme.colors.text,
    marginBottom: 8,
  },
  metaRow: {
    flexDirection: 'row',
    alignItems: 'center',
    flexWrap: 'wrap',
    gap: 10,
    marginBottom: 12,
  },
  ratingBadge: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 4,
    backgroundColor: 'rgba(251, 191, 36, 0.15)',
    paddingHorizontal: 8,
    paddingVertical: 4,
    borderRadius: 6,
  },
  ratingValue: {
    fontSize: 12,
    fontWeight: '800',
    color: theme.colors.star,
  },
  reviewsCountText: {
    fontSize: 11,
    color: theme.colors.textMuted,
  },
  downloadsBadge: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 4,
  },
  downloadsText: {
    fontSize: 12,
    color: theme.colors.textMuted,
  },
  versionBadge: {
    backgroundColor: theme.colors.card,
    paddingHorizontal: 6,
    paddingVertical: 2,
    borderRadius: 4,
    borderWidth: 0.5,
    borderColor: theme.colors.borderSubtle,
  },
  versionText: {
    fontSize: 10,
    color: theme.colors.textDim,
    fontWeight: '600',
  },
  genresRow: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    gap: 6,
    marginBottom: 12,
  },
  genreChip: {
    backgroundColor: theme.colors.badgeBg,
    paddingHorizontal: 8,
    paddingVertical: 4,
    borderRadius: 6,
    borderWidth: 1,
    borderColor: theme.colors.badgeBorder,
  },
  genreChipText: {
    fontSize: 11,
    fontWeight: '700',
    color: theme.colors.primary,
  },
  platformsRow: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 8,
    flexWrap: 'wrap',
  },
  platformLabel: {
    fontSize: 12,
    color: theme.colors.textDim,
  },
  platformBadge: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 4,
    backgroundColor: theme.colors.card,
    paddingHorizontal: 6,
    paddingVertical: 3,
    borderRadius: 4,
  },
  platformBadgeText: {
    fontSize: 11,
    color: theme.colors.textSecondary,
    fontWeight: '600',
  },
  sizeText: {
    fontSize: 11,
    color: theme.colors.textDim,
    marginLeft: 6,
  },
  buyCard: {
    marginHorizontal: 16,
    marginVertical: 12,
    padding: 16,
    backgroundColor: theme.colors.card,
    borderRadius: theme.borderRadius.xl,
    borderWidth: 1,
    borderColor: theme.colors.border,
  },
  buyCardHeader: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    marginBottom: 14,
  },
  buyCardLabel: {
    fontSize: 11,
    color: theme.colors.textDim,
    fontWeight: '600',
    marginBottom: 2,
  },
  priceRow: {
    flexDirection: 'row',
    alignItems: 'baseline',
    gap: 8,
  },
  originalPriceText: {
    fontSize: 13,
    color: theme.colors.textDim,
    textDecorationLine: 'line-through',
  },
  finalPriceText: {
    fontSize: 20,
    fontWeight: '900',
    color: theme.colors.text,
  },
  discountedFinalPrice: {
    color: theme.colors.secondary,
  },
  discountCardBadge: {
    backgroundColor: theme.colors.discount,
    paddingHorizontal: 10,
    paddingVertical: 5,
    borderRadius: 6,
  },
  discountCardText: {
    color: '#000',
    fontWeight: '900',
    fontSize: 12,
  },
  actionsContainer: {
    gap: 10,
  },
  buyButton: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'center',
    gap: 8,
    backgroundColor: theme.colors.primary,
    paddingVertical: 13,
    borderRadius: theme.borderRadius.md,
  },
  buyButtonText: {
    fontSize: 14,
    fontWeight: '900',
    color: '#000',
    letterSpacing: 0.5,
  },
  wishlistButton: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'center',
    gap: 8,
    backgroundColor: 'rgba(0, 242, 254, 0.08)',
    borderWidth: 1,
    borderColor: theme.colors.primary,
    paddingVertical: 11,
    borderRadius: theme.borderRadius.md,
  },
  wishlistButtonActive: {
    backgroundColor: 'rgba(244, 63, 94, 0.1)',
    borderColor: '#f43f5e',
  },
  wishlistButtonText: {
    fontSize: 13,
    fontWeight: '700',
    color: theme.colors.primary,
  },
  wishlistButtonTextActive: {
    color: '#f43f5e',
  },
  blockchainNotice: {
    flexDirection: 'row',
    alignItems: 'flex-start',
    gap: 6,
    marginTop: 12,
    paddingTop: 10,
    borderTopWidth: 1,
    borderTopColor: theme.colors.borderSubtle,
  },
  blockchainNoticeText: {
    fontSize: 10,
    color: theme.colors.textDim,
    lineHeight: 14,
    flex: 1,
  },
  descriptionSection: {
    padding: 16,
  },
  sectionHeading: {
    fontSize: 16,
    fontWeight: '800',
    color: theme.colors.text,
    marginBottom: 10,
  },
  shortDescBox: {
    backgroundColor: theme.colors.card,
    borderLeftWidth: 3,
    borderLeftColor: theme.colors.primary,
    padding: 12,
    borderRadius: 4,
    marginBottom: 12,
  },
  shortDescText: {
    fontSize: 12,
    color: theme.colors.textSecondary,
    lineHeight: 18,
    fontStyle: 'italic',
  },
  fullDescText: {
    fontSize: 13,
    color: theme.colors.textMuted,
    lineHeight: 20,
  },
  dlcSection: {
    padding: 16,
    borderTopWidth: 1,
    borderTopColor: theme.colors.borderSubtle,
  },
  dlcHeaderRow: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 8,
    marginBottom: 12,
  },
  dlcItem: {
    flexDirection: 'row',
    backgroundColor: theme.colors.card,
    borderRadius: theme.borderRadius.md,
    borderWidth: 1,
    borderColor: theme.colors.borderSubtle,
    padding: 8,
    marginBottom: 10,
    gap: 10,
  },
  dlcImageWrap: {
    width: 65,
    height: 65,
    borderRadius: 6,
    overflow: 'hidden',
  },
  dlcImage: {
    width: '100%',
    height: '100%',
  },
  dlcInfo: {
    flex: 1,
    justifyContent: 'center',
  },
  dlcTitle: {
    fontSize: 13,
    fontWeight: '700',
    color: theme.colors.text,
  },
  dlcDesc: {
    fontSize: 10,
    color: theme.colors.textDim,
    marginTop: 2,
    lineHeight: 14,
  },
  dlcPrice: {
    fontSize: 12,
    fontWeight: '800',
    color: theme.colors.secondary,
    marginTop: 4,
  },
  tagsSection: {
    padding: 16,
    borderTopWidth: 1,
    borderTopColor: theme.colors.borderSubtle,
  },
  tagsContainer: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    gap: 8,
  },
  tagBadge: {
    backgroundColor: theme.colors.card,
    paddingHorizontal: 10,
    paddingVertical: 5,
    borderRadius: theme.borderRadius.full,
    borderWidth: 1,
    borderColor: theme.colors.borderSubtle,
  },
  tagText: {
    fontSize: 11,
    color: theme.colors.textMuted,
  },
  languagesSection: {
    padding: 16,
    borderTopWidth: 1,
    borderTopColor: theme.colors.borderSubtle,
  },
  langGrid: {
    gap: 8,
  },
  langItem: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    backgroundColor: theme.colors.card,
    paddingHorizontal: 12,
    paddingVertical: 8,
    borderRadius: 6,
  },
  langName: {
    fontSize: 12,
    color: theme.colors.text,
    fontWeight: '600',
  },
  langBadges: {
    flexDirection: 'row',
    gap: 6,
  },
  langTag: {
    fontSize: 10,
    color: theme.colors.textDim,
    backgroundColor: theme.colors.surface,
    paddingHorizontal: 6,
    paddingVertical: 2,
    borderRadius: 4,
  },
  loaderCenter: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
    padding: 32,
  },
  emptyText: {
    fontSize: 14,
    color: theme.colors.textMuted,
  },
});
