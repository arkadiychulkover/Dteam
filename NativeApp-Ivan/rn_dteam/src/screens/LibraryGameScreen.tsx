import React, { useState, useEffect } from 'react';
import {
  View,
  Text,
  ScrollView,
  TouchableOpacity,
  StyleSheet,
  Alert,
  StatusBar,
  ActivityIndicator,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { Ionicons } from '@expo/vector-icons';
import type { Game, UserGame, Review } from '../types';
import { gamesService } from '../services/gamesService';
import { libraryService } from '../services/libraryService';
import { BackendImage } from '../components/BackendImage';
import { formatBytes, formatPlayTime, formatDate, formatPrice } from '../utils/formatters';
import { theme } from '../styles/theme';

interface LibraryGameScreenProps {
  game?: Game;
  userGame?: UserGame;
  onBack?: () => void;
  navigation?: any;
  route?: { params?: { game?: Game; userGame?: UserGame } };
}

export const LibraryGameScreen: React.FC<LibraryGameScreenProps> = (props) => {
  const initialGame = props.route?.params?.game || props.game;
  const initialUserGame = props.route?.params?.userGame || props.userGame;

  if (!initialGame) {
    return (
      <SafeAreaView style={styles.safeArea}>
        <View style={styles.loaderCenter}>
          <Text style={styles.emptyText}>Гру не знайдено у бібліотеці</Text>
        </View>
      </SafeAreaView>
    );
  }

  const [game, setGame] = useState<Game>(initialGame);
  const [isFavorite, setIsFavorite] = useState(initialUserGame?.isFavorite ?? false);
  const [activeTab, setActiveTab] = useState<'info' | 'dlc' | 'reviews'>('info');

  const [reviews, setReviews] = useState<Review[]>([]);
  const [dlcs, setDlcs] = useState<Game[]>([]);
  const [isLoadingDetails, setIsLoadingDetails] = useState(false);
  const [isDownloading, setIsDownloading] = useState(false);

  useEffect(() => {
    let isMounted = true;
    setIsLoadingDetails(true);

    Promise.all([
      gamesService.getGameById(initialGame.id).catch(() => null),
      gamesService.getReviews(initialGame.id).catch(() => []),
      gamesService.getDlcs(initialGame.id).catch(() => []),
    ])
      .then(([fullGame, fetchedReviews, fetchedDlcs]) => {
        if (!isMounted) return;
        if (fullGame) setGame(fullGame);
        if (fetchedReviews) setReviews(fetchedReviews);
        if (fetchedDlcs) setDlcs(fetchedDlcs);
      })
      .finally(() => {
        if (isMounted) setIsLoadingDetails(false);
      });

    return () => {
      isMounted = false;
    };
  }, [initialGame.id]);

  const handleBack = () => {
    if (props.navigation?.canGoBack?.()) {
      props.navigation.goBack();
    } else if (props.onBack) {
      props.onBack();
    }
  };

  const handleToggleFavorite = async () => {
    setIsFavorite(!isFavorite);
    try {
      const res = await libraryService.toggleFavorite(game.id);
      setIsFavorite(res.isFavorite);
    } catch {
      setIsFavorite(!isFavorite);
    }
  };

  const handleDownload = () => {
    setIsDownloading(true);
    setTimeout(() => {
      setIsDownloading(false);
      Alert.alert(
        'Завантаження розпочато',
        `Підготовка архіву гри "${game.title}" (${formatBytes(game.sizeInBytes)}).\nЗавантаження буде відстежуватись у фоновому режимі.`,
        [{ text: 'Чудово', style: 'default' }]
      );
    }, 1000);
  };

  return (
    <SafeAreaView style={styles.safeArea} edges={['top', 'left', 'right']}>
      <StatusBar barStyle="light-content" backgroundColor={theme.colors.background} />

      <View style={styles.navBar}>
        <TouchableOpacity style={styles.backButton} onPress={handleBack} activeOpacity={0.7}>
          <Ionicons name="arrow-back" size={20} color={theme.colors.text} />
          <Text style={styles.backButtonText}>Назад</Text>
        </TouchableOpacity>

        <Text style={styles.navTitle} numberOfLines={1}>
          {game.title}
        </Text>

        <TouchableOpacity
          style={styles.favHeaderBtn}
          onPress={handleToggleFavorite}
          activeOpacity={0.7}
        >
          <Ionicons
            name={isFavorite ? 'heart' : 'heart-outline'}
            size={22}
            color={isFavorite ? '#f43f5e' : theme.colors.textMuted}
          />
        </TouchableOpacity>
      </View>

      <ScrollView showsVerticalScrollIndicator={false} contentContainerStyle={styles.scrollContent}>
        <View style={styles.heroWrapper}>
          <BackendImage
            src={game.headerImageUrl || game.coverImageUrl}
            style={styles.heroImage}
            resizeMode="cover"
          />
          <View style={styles.heroOverlay} />

          <View style={styles.heroContent}>
            <Text style={styles.heroTitle}>{game.title}</Text>
            {initialUserGame?.playTimeMinutes ? (
              <View style={styles.playTimeBadge}>
                <Ionicons name="time-outline" size={12} color={theme.colors.primary} />
                <Text style={styles.playTimeText}>
                  Час у грі: {formatPlayTime(initialUserGame.playTimeMinutes)}
                </Text>
              </View>
            ) : null}
          </View>
        </View>

        <View style={styles.actionPanel}>
          <TouchableOpacity
            style={[styles.downloadButton, isDownloading && styles.disabledBtn]}
            onPress={handleDownload}
            disabled={isDownloading}
            activeOpacity={0.8}
          >
            {isDownloading ? (
              <ActivityIndicator size="small" color="#000" />
            ) : (
              <>
                <Ionicons name="cloud-download-outline" size={18} color="#000" />
                <Text style={styles.downloadButtonText}>Завантажити гру</Text>
              </>
            )}
          </TouchableOpacity>

          <View style={styles.metaStatBox}>
            <Text style={styles.statLabel}>Розмір файлу</Text>
            <Text style={styles.statValue}>
              {game.sizeInBytes > 0 ? formatBytes(game.sizeInBytes) : '—'}
            </Text>
          </View>
        </View>

        <View style={styles.tabsContainer}>
          <TouchableOpacity
            style={[styles.tabButton, activeTab === 'info' && styles.tabButtonActive]}
            onPress={() => setActiveTab('info')}
            activeOpacity={0.8}
          >
            <Text style={[styles.tabText, activeTab === 'info' && styles.tabTextActive]}>
              Про гру
            </Text>
          </TouchableOpacity>

          <TouchableOpacity
            style={[styles.tabButton, activeTab === 'dlc' && styles.tabButtonActive]}
            onPress={() => setActiveTab('dlc')}
            activeOpacity={0.8}
          >
            <Text style={[styles.tabText, activeTab === 'dlc' && styles.tabTextActive]}>
              DLC ({dlcs.length})
            </Text>
          </TouchableOpacity>

          <TouchableOpacity
            style={[styles.tabButton, activeTab === 'reviews' && styles.tabButtonActive]}
            onPress={() => setActiveTab('reviews')}
            activeOpacity={0.8}
          >
            <Text style={[styles.tabText, activeTab === 'reviews' && styles.tabTextActive]}>
              Рецензії ({reviews.length})
            </Text>
          </TouchableOpacity>
        </View>

        {activeTab === 'info' && (
          <View style={styles.sectionContainer}>
            <View style={styles.metaGrid}>
              <View style={styles.metaTile}>
                <Ionicons name="star" size={14} color={theme.colors.star} />
                <Text style={styles.tileTitle}>
                  {game.averageRating > 0 ? game.averageRating.toFixed(2) : 'NEW'}
                </Text>
                <Text style={styles.tileSub}>Рейтинг</Text>
              </View>

              <View style={styles.metaTile}>
                <Ionicons name="download-outline" size={14} color={theme.colors.primary} />
                <Text style={styles.tileTitle}>{game.downloadCount.toLocaleString('uk-UA')}</Text>
                <Text style={styles.tileSub}>Завантажень</Text>
              </View>

              {game.version ? (
                <View style={styles.metaTile}>
                  <Ionicons name="code-working-outline" size={14} color={theme.colors.secondary} />
                  <Text style={styles.tileTitle}>v{game.version}</Text>
                  <Text style={styles.tileSub}>Версія</Text>
                </View>
              ) : null}
            </View>

            {game.genres && game.genres.length > 0 && (
              <View style={styles.genresRow}>
                {game.genres.map((g, idx) => (
                  <View key={idx} style={styles.genreChip}>
                    <Text style={styles.genreChipText}>{g}</Text>
                  </View>
                ))}
              </View>
            )}

            <Text style={styles.sectionTitle}>Опис гри</Text>
            {game.shortDescription ? (
              <View style={styles.shortDescBox}>
                <Text style={styles.shortDescText}>{game.shortDescription}</Text>
              </View>
            ) : null}

            <Text style={styles.fullDescText}>
              {game.description || 'Детальний опис гри наразі відсутній.'}
            </Text>
          </View>
        )}

        {activeTab === 'dlc' && (
          <View style={styles.sectionContainer}>
            {isLoadingDetails ? (
              <ActivityIndicator size="small" color={theme.colors.primary} style={{ margin: 20 }} />
            ) : dlcs.length === 0 ? (
              <View style={styles.emptyBox}>
                <Ionicons name="extension-puzzle-outline" size={32} color={theme.colors.textDim} />
                <Text style={styles.emptyBoxText}>Для цієї гри немає доступних DLC.</Text>
              </View>
            ) : (
              dlcs.map((dlc) => (
                <View key={dlc.id} style={styles.dlcCard}>
                  <BackendImage
                    src={dlc.coverImageUrl || dlc.headerImageUrl}
                    style={styles.dlcImage}
                    resizeMode="cover"
                  />
                  <View style={styles.dlcInfo}>
                    <Text style={styles.dlcTitle} numberOfLines={1}>
                      {dlc.title}
                    </Text>
                    <Text style={styles.dlcPrice}>
                      {formatPrice(dlc.priceInNanoTons, dlc.discountPercentage)}
                    </Text>
                  </View>
                </View>
              ))
            )}
          </View>
        )}

        {activeTab === 'reviews' && (
          <View style={styles.sectionContainer}>
            {isLoadingDetails ? (
              <ActivityIndicator size="small" color={theme.colors.primary} style={{ margin: 20 }} />
            ) : reviews.length === 0 ? (
              <View style={styles.emptyBox}>
                <Ionicons name="chatbubbles-outline" size={32} color={theme.colors.textDim} />
                <Text style={styles.emptyBoxText}>Рецензій на цю гру ще немає.</Text>
              </View>
            ) : (
              reviews.map((rev) => (
                <View key={rev.id} style={styles.reviewCard}>
                  <View style={styles.reviewHeader}>
                    <View style={styles.reviewerAvatar}>
                      <Text style={styles.avatarChar}>
                        {(rev.username || 'U').charAt(0).toUpperCase()}
                      </Text>
                    </View>
                    <View style={styles.reviewUserMeta}>
                      <Text style={styles.reviewerName}>{rev.username || 'Користувач'}</Text>
                      <Text style={styles.reviewDate}>{formatDate(rev.createdAt)}</Text>
                    </View>

                    <View style={styles.ratingRow}>
                      <Ionicons name="star" size={13} color={theme.colors.star} />
                      <Text style={styles.ratingVal}>{rev.rating}</Text>
                    </View>
                  </View>

                  <Text style={styles.reviewContent}>{rev.content}</Text>
                </View>
              ))
            )}
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
  loaderCenter: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
  },
  emptyText: {
    fontSize: 14,
    color: theme.colors.textMuted,
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
  favHeaderBtn: {
    padding: 4,
  },
  scrollContent: {
    paddingBottom: 40,
  },
  heroWrapper: {
    height: 200,
    position: 'relative',
    justifyContent: 'flex-end',
  },
  heroImage: {
    ...StyleSheet.absoluteFill,
  },
  heroOverlay: {
    ...StyleSheet.absoluteFill,
    backgroundColor: 'rgba(3, 13, 18, 0.65)',
  },
  heroContent: {
    padding: 16,
  },
  heroTitle: {
    fontSize: 22,
    fontWeight: '900',
    color: theme.colors.text,
    marginBottom: 6,
  },
  playTimeBadge: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 4,
    backgroundColor: 'rgba(6, 24, 32, 0.85)',
    paddingHorizontal: 8,
    paddingVertical: 4,
    borderRadius: 6,
    alignSelf: 'flex-start',
  },
  playTimeText: {
    fontSize: 11,
    color: theme.colors.primary,
    fontWeight: '700',
  },
  actionPanel: {
    flexDirection: 'row',
    alignItems: 'center',
    padding: 16,
    gap: 12,
    backgroundColor: theme.colors.surface,
    borderBottomWidth: 1,
    borderBottomColor: theme.colors.borderSubtle,
  },
  downloadButton: {
    flex: 1,
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'center',
    gap: 8,
    backgroundColor: theme.colors.primary,
    paddingVertical: 12,
    borderRadius: theme.borderRadius.md,
  },
  disabledBtn: {
    opacity: 0.6,
  },
  downloadButtonText: {
    fontSize: 14,
    fontWeight: '900',
    color: '#000',
  },
  metaStatBox: {
    alignItems: 'flex-end',
  },
  statLabel: {
    fontSize: 10,
    color: theme.colors.textDim,
  },
  statValue: {
    fontSize: 13,
    fontWeight: '800',
    color: theme.colors.text,
  },
  tabsContainer: {
    flexDirection: 'row',
    backgroundColor: theme.colors.surface,
    borderBottomWidth: 1,
    borderBottomColor: theme.colors.borderSubtle,
    paddingHorizontal: 16,
  },
  tabButton: {
    paddingVertical: 12,
    marginRight: 20,
    borderBottomWidth: 2,
    borderBottomColor: 'transparent',
  },
  tabButtonActive: {
    borderBottomColor: theme.colors.primary,
  },
  tabText: {
    fontSize: 13,
    fontWeight: '700',
    color: theme.colors.textMuted,
  },
  tabTextActive: {
    color: theme.colors.primary,
  },
  sectionContainer: {
    padding: 16,
  },
  metaGrid: {
    flexDirection: 'row',
    gap: 12,
    marginBottom: 16,
  },
  metaTile: {
    flex: 1,
    backgroundColor: theme.colors.card,
    borderRadius: theme.borderRadius.md,
    padding: 10,
    alignItems: 'center',
    borderWidth: 1,
    borderColor: theme.colors.borderSubtle,
  },
  tileTitle: {
    fontSize: 14,
    fontWeight: '800',
    color: theme.colors.text,
    marginVertical: 2,
  },
  tileSub: {
    fontSize: 10,
    color: theme.colors.textDim,
  },
  genresRow: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    gap: 6,
    marginBottom: 16,
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
  sectionTitle: {
    fontSize: 15,
    fontWeight: '800',
    color: theme.colors.text,
    marginBottom: 8,
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
  emptyBox: {
    backgroundColor: theme.colors.card,
    borderRadius: theme.borderRadius.lg,
    padding: 30,
    alignItems: 'center',
    borderWidth: 1,
    borderColor: theme.colors.borderSubtle,
    marginVertical: 10,
  },
  emptyBoxText: {
    fontSize: 12,
    color: theme.colors.textMuted,
    marginTop: 8,
  },
  dlcCard: {
    flexDirection: 'row',
    alignItems: 'center',
    backgroundColor: theme.colors.card,
    borderRadius: theme.borderRadius.md,
    padding: 10,
    marginBottom: 10,
    gap: 12,
    borderWidth: 1,
    borderColor: theme.colors.borderSubtle,
  },
  dlcImage: {
    width: 60,
    height: 40,
    borderRadius: 6,
  },
  dlcInfo: {
    flex: 1,
  },
  dlcTitle: {
    fontSize: 13,
    fontWeight: '700',
    color: theme.colors.text,
  },
  dlcPrice: {
    fontSize: 12,
    fontWeight: '800',
    color: theme.colors.primary,
    marginTop: 2,
  },
  reviewCard: {
    backgroundColor: theme.colors.card,
    borderRadius: theme.borderRadius.md,
    padding: 12,
    marginBottom: 12,
    borderWidth: 1,
    borderColor: theme.colors.borderSubtle,
  },
  reviewHeader: {
    flexDirection: 'row',
    alignItems: 'center',
    marginBottom: 8,
  },
  reviewerAvatar: {
    width: 28,
    height: 28,
    borderRadius: 14,
    backgroundColor: theme.colors.surface,
    justifyContent: 'center',
    alignItems: 'center',
    marginRight: 8,
  },
  avatarChar: {
    fontSize: 12,
    fontWeight: '800',
    color: theme.colors.primary,
  },
  reviewUserMeta: {
    flex: 1,
  },
  reviewerName: {
    fontSize: 12,
    fontWeight: '800',
    color: theme.colors.text,
  },
  reviewDate: {
    fontSize: 10,
    color: theme.colors.textDim,
  },
  ratingRow: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 4,
    backgroundColor: 'rgba(251, 191, 36, 0.15)',
    paddingHorizontal: 6,
    paddingVertical: 2,
    borderRadius: 4,
  },
  ratingVal: {
    fontSize: 11,
    fontWeight: '800',
    color: theme.colors.star,
  },
  reviewContent: {
    fontSize: 12,
    color: theme.colors.textMuted,
    lineHeight: 18,
  },
});
