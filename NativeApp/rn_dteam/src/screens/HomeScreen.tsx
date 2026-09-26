import React, { useEffect } from 'react';
import {
  View,
  Text,
  ScrollView,
  TouchableOpacity,
  RefreshControl,
  ActivityIndicator,
  StyleSheet,
  StatusBar,
  Alert,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { Ionicons } from '@expo/vector-icons';
import { useGamesStore } from '../store/useGamesStore';
import { useAuthStore } from '../store/useAuthStore';
import { useCartStore } from '../store/useCartStore';
import { FeaturedCarousel } from '../components/FeaturedCarousel';
import { SpecialOfferCard } from '../components/SpecialOfferCard';
import { GameCard } from '../components/GameCard';
import { GENRES } from '../utils/constants';
import { theme } from '../styles/theme';
import type { Game } from '../types';

interface HomeScreenProps {
  onSelectGame: (game: Game) => void;
  navigation?: any;
  onNavigateLibrary?: () => void;
  onNavigateLogin?: () => void;
}

export const HomeScreen: React.FC<HomeScreenProps> = ({
  onSelectGame,
  navigation,
  onNavigateLibrary,
  onNavigateLogin,
}) => {
  const {
    games,
    selectedGenre,
    isLoading,
    isRefreshing,
    error,
    loadGames,
    refresh,
    setSelectedGenre,
  } = useGamesStore();

  const { user, isAuthenticated, logout } = useAuthStore();
  const { items: cartItems, loadCart } = useCartStore();

  useEffect(() => {
    loadGames();
    if (isAuthenticated) {
      loadCart();
    }
  }, [loadGames, loadCart, isAuthenticated]);

  const discountedGames = games.filter((g) => (g.discountPercentage || 0) > 0);

  const goToLibrary = () => {
    if (onNavigateLibrary) {
      onNavigateLibrary();
    } else if (navigation?.navigate) {
      navigation.navigate('Library');
    }
  };

  const goToCart = () => {
    if (navigation?.navigate) {
      navigation.navigate('Cart');
    }
  };

  const goToLogin = () => {
    if (onNavigateLogin) {
      onNavigateLogin();
    } else if (navigation?.navigate) {
      navigation.navigate('Login');
    }
  };

  const handleLogoutConfirm = () => {
    Alert.alert(
      'Вихід з акаунта',
      `Ви дійсно бажаєте вийти з акаунта ${user?.username || ''}?`,
      [
        { text: 'Скасувати', style: 'cancel' },
        {
          text: 'Вийти',
          style: 'destructive',
          onPress: () => logout(),
        },
      ]
    );
  };

  return (
    <SafeAreaView style={styles.safeArea} edges={['top', 'left', 'right']}>
      <StatusBar barStyle="light-content" backgroundColor={theme.colors.background} />

      <View style={styles.header}>
        <View style={styles.logoRow}>
          <View style={styles.logoIcon}>
            <Ionicons name="game-controller" size={18} color="#000" />
          </View>
          <View>
            <Text style={styles.logoTitle}>DTEAM</Text>
            <Text style={styles.logoSubtitle}>WEB3 GAMING PLATFORM</Text>
          </View>
        </View>

        <View style={styles.headerRight}>
          <TouchableOpacity
            style={styles.cartIconBtn}
            onPress={goToCart}
            activeOpacity={0.7}
            accessibilityLabel="Кошик"
          >
            <Ionicons name="cart-outline" size={18} color={theme.colors.text} />
            {cartItems.length > 0 && (
              <View style={styles.cartBadge}>
                <Text style={styles.cartBadgeText}>
                  {cartItems.length > 99 ? '99+' : cartItems.length}
                </Text>
              </View>
            )}
          </TouchableOpacity>

          <TouchableOpacity
            style={styles.headerActionBtn}
            onPress={goToLibrary}
            activeOpacity={0.7}
          >
            <Ionicons name="library-outline" size={17} color={theme.colors.primary} />
            <Text style={styles.headerActionBtnText}>Бібліотека</Text>
          </TouchableOpacity>

          {isAuthenticated ? (
            <View style={styles.userChip}>
              <View style={styles.userAvatarCircle}>
                <Text style={styles.userAvatarText}>
                  {(user?.username || 'U').charAt(0).toUpperCase()}
                </Text>
              </View>
              <Text style={styles.userNameText} numberOfLines={1}>
                {user?.username || 'User'}
              </Text>
              <TouchableOpacity
                style={styles.logoutBtn}
                onPress={handleLogoutConfirm}
                activeOpacity={0.7}
              >
                <Ionicons name="log-out-outline" size={15} color={theme.colors.error} />
              </TouchableOpacity>
            </View>
          ) : (
            <TouchableOpacity
              style={styles.headerActionBtn}
              onPress={goToLogin}
              activeOpacity={0.7}
            >
              <Ionicons name="person-circle-outline" size={18} color={theme.colors.primary} />
              <Text style={styles.headerActionBtnText}>Вхід</Text>
            </TouchableOpacity>
          )}

          <View style={styles.networkBadge}>
            <View style={styles.networkDot} />
            <Text style={styles.networkText}>TON</Text>
          </View>
        </View>
      </View>

      <View style={styles.genreFilterContainer}>
        <ScrollView
          horizontal
          showsHorizontalScrollIndicator={false}
          contentContainerStyle={styles.genreScrollContent}
        >
          {GENRES.map((genre) => {
            const isSelected = selectedGenre === genre;
            return (
              <TouchableOpacity
                key={genre}
                style={[
                  styles.genrePill,
                  isSelected && styles.genrePillActive,
                ]}
                onPress={() => setSelectedGenre(genre)}
                activeOpacity={0.7}
              >
                <Text
                  style={[
                    styles.genrePillText,
                    isSelected && styles.genrePillTextActive,
                  ]}
                >
                  {genre}
                </Text>
              </TouchableOpacity>
            );
          })}
        </ScrollView>
      </View>

      <ScrollView
        showsVerticalScrollIndicator={false}
        contentContainerStyle={styles.scrollContent}
        refreshControl={
          <RefreshControl
            refreshing={isRefreshing}
            onRefresh={refresh}
            tintColor={theme.colors.primary}
            colors={[theme.colors.primary]}
          />
        }
      >
        {isLoading && games.length === 0 ? (
          <View style={styles.loaderCenter}>
            <ActivityIndicator size="large" color={theme.colors.primary} />
            <Text style={styles.loaderText}>Завантаження крамниці Dteam...</Text>
          </View>
        ) : error && games.length === 0 ? (
          <View style={styles.errorContainer}>
            <Ionicons name="alert-circle-outline" size={44} color={theme.colors.error} />
            <Text style={styles.errorTitle}>Помилка підключення</Text>
            <Text style={styles.errorSubtitle}>{error}</Text>
            <TouchableOpacity
              style={styles.retryButton}
              onPress={() => loadGames()}
              activeOpacity={0.8}
            >
              <Ionicons name="refresh" size={16} color="#000" />
              <Text style={styles.retryText}>Спробувати знову</Text>
            </TouchableOpacity>
          </View>
        ) : (
          <>
            <FeaturedCarousel games={games} onPress={onSelectGame} />

            {discountedGames.length > 0 && (
              <View style={styles.section}>
                <View style={styles.sectionHeader}>
                  <View style={styles.sectionTitleRow}>
                    <Ionicons name="flame" size={18} color={theme.colors.secondary} />
                    <Text style={styles.sectionTitle}>Особливі пропозиції</Text>
                  </View>
                  <View style={styles.discountCounter}>
                    <Text style={styles.discountCounterText}>
                      {discountedGames.length} зі знижкою
                    </Text>
                  </View>
                </View>

                <ScrollView
                  horizontal
                  showsHorizontalScrollIndicator={false}
                  contentContainerStyle={styles.horizontalList}
                >
                  {discountedGames.map((game) => (
                    <SpecialOfferCard
                      key={game.id}
                      game={game}
                      onPress={onSelectGame}
                    />
                  ))}
                </ScrollView>
              </View>
            )}

            <View style={styles.section}>
              <View style={styles.sectionHeader}>
                <View style={styles.sectionTitleRow}>
                  <Ionicons name="grid-outline" size={17} color={theme.colors.primary} />
                  <Text style={styles.sectionTitle}>Каталог ігор</Text>
                </View>
                <Text style={styles.gameCountText}>
                  {games.length} {games.length === 1 ? 'гра' : games.length < 5 ? 'гри' : 'ігор'}
                </Text>
              </View>

              {games.length === 0 ? (
                <View style={styles.emptyContainer}>
                  <Ionicons name="search-outline" size={36} color={theme.colors.textDim} />
                  <Text style={styles.emptyText}>У цій категорії поки немає ігор</Text>
                  <TouchableOpacity
                    style={styles.resetFilterButton}
                    onPress={() => setSelectedGenre('Всі ігри')}
                  >
                    <Text style={styles.resetFilterText}>Показати всі ігри</Text>
                  </TouchableOpacity>
                </View>
              ) : (
                games.map((game) => (
                  <GameCard
                    key={game.id}
                    game={game}
                    onPress={onSelectGame}
                  />
                ))
              )}
            </View>
          </>
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
  header: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    paddingHorizontal: 16,
    paddingVertical: 12,
    borderBottomWidth: 1,
    borderBottomColor: theme.colors.borderSubtle,
    backgroundColor: theme.colors.surface,
  },
  logoRow: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 10,
  },
  logoIcon: {
    width: 32,
    height: 32,
    borderRadius: 8,
    backgroundColor: theme.colors.primary,
    justifyContent: 'center',
    alignItems: 'center',
  },
  logoTitle: {
    fontSize: 16,
    fontWeight: '900',
    color: theme.colors.text,
    letterSpacing: 1,
  },
  logoSubtitle: {
    fontSize: 8,
    fontWeight: '700',
    color: theme.colors.primary,
    letterSpacing: 0.8,
  },
  headerRight: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 8,
  },
  cartIconBtn: {
    width: 30,
    height: 30,
    borderRadius: 8,
    backgroundColor: 'rgba(255, 255, 255, 0.05)',
    borderWidth: 1,
    borderColor: theme.colors.borderSubtle,
    alignItems: 'center',
    justifyContent: 'center',
    position: 'relative',
  },
  cartBadge: {
    position: 'absolute',
    top: -5,
    right: -5,
    backgroundColor: theme.colors.secondary,
    borderRadius: 8,
    minWidth: 16,
    height: 16,
    alignItems: 'center',
    justifyContent: 'center',
    paddingHorizontal: 3,
  },
  cartBadgeText: {
    fontSize: 9,
    fontWeight: '900',
    color: '#000',
  },
  headerActionBtn: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 4,
    backgroundColor: 'rgba(0, 242, 254, 0.1)',
    borderWidth: 1,
    borderColor: 'rgba(0, 242, 254, 0.3)',
    paddingHorizontal: 8,
    paddingVertical: 5,
    borderRadius: 8,
  },
  headerActionBtnText: {
    fontSize: 11,
    fontWeight: '700',
    color: theme.colors.text,
  },
  userChip: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 6,
    backgroundColor: 'rgba(0, 242, 254, 0.12)',
    borderWidth: 1,
    borderColor: 'rgba(0, 242, 254, 0.35)',
    paddingHorizontal: 8,
    paddingVertical: 4,
    borderRadius: 8,
  },
  userAvatarCircle: {
    width: 20,
    height: 20,
    borderRadius: 10,
    backgroundColor: theme.colors.primary,
    alignItems: 'center',
    justifyContent: 'center',
  },
  userAvatarText: {
    color: '#000',
    fontSize: 10,
    fontWeight: '900',
  },
  userNameText: {
    fontSize: 11,
    fontWeight: '800',
    color: theme.colors.text,
    maxWidth: 90,
  },
  logoutBtn: {
    paddingLeft: 2,
    paddingVertical: 2,
  },
  networkBadge: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 6,
    backgroundColor: 'rgba(0, 152, 234, 0.15)',
    borderWidth: 1,
    borderColor: 'rgba(0, 152, 234, 0.4)',
    paddingHorizontal: 8,
    paddingVertical: 4,
    borderRadius: theme.borderRadius.full,
  },
  networkDot: {
    width: 6,
    height: 6,
    borderRadius: 3,
    backgroundColor: theme.colors.tonBlue,
  },
  networkText: {
    fontSize: 11,
    fontWeight: '800',
    color: theme.colors.tonBlue,
  },
  genreFilterContainer: {
    backgroundColor: theme.colors.surface,
    paddingVertical: 10,
    borderBottomWidth: 1,
    borderBottomColor: theme.colors.borderSubtle,
  },
  genreScrollContent: {
    paddingHorizontal: 16,
    gap: 8,
  },
  genrePill: {
    paddingHorizontal: 14,
    paddingVertical: 6,
    borderRadius: theme.borderRadius.full,
    backgroundColor: theme.colors.card,
    borderWidth: 1,
    borderColor: theme.colors.borderSubtle,
  },
  genrePillActive: {
    backgroundColor: 'rgba(0, 242, 254, 0.18)',
    borderColor: theme.colors.primary,
  },
  genrePillText: {
    fontSize: 12,
    fontWeight: '600',
    color: theme.colors.textMuted,
  },
  genrePillTextActive: {
    color: theme.colors.primary,
    fontWeight: '800',
  },
  scrollContent: {
    padding: 16,
    paddingBottom: 40,
  },
  loaderCenter: {
    alignItems: 'center',
    justifyContent: 'center',
    paddingVertical: 80,
    gap: 12,
  },
  loaderText: {
    fontSize: 13,
    color: theme.colors.textMuted,
    fontWeight: '600',
  },
  errorContainer: {
    backgroundColor: theme.colors.card,
    borderRadius: theme.borderRadius.lg,
    padding: 24,
    alignItems: 'center',
    borderWidth: 1,
    borderColor: 'rgba(244, 63, 94, 0.3)',
    marginVertical: 40,
  },
  errorTitle: {
    fontSize: 16,
    fontWeight: '800',
    color: theme.colors.text,
    marginTop: 10,
  },
  errorSubtitle: {
    fontSize: 12,
    color: theme.colors.textMuted,
    textAlign: 'center',
    marginTop: 6,
    marginBottom: 16,
  },
  retryButton: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 6,
    backgroundColor: theme.colors.primary,
    paddingHorizontal: 16,
    paddingVertical: 9,
    borderRadius: theme.borderRadius.md,
  },
  retryText: {
    fontSize: 12,
    fontWeight: '800',
    color: '#000',
  },
  section: {
    marginBottom: 24,
  },
  sectionHeader: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    marginBottom: 12,
  },
  sectionTitleRow: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 6,
  },
  sectionTitle: {
    fontSize: 17,
    fontWeight: '900',
    color: theme.colors.text,
  },
  discountCounter: {
    backgroundColor: 'rgba(16, 185, 129, 0.15)',
    paddingHorizontal: 8,
    paddingVertical: 3,
    borderRadius: 6,
    borderWidth: 0.5,
    borderColor: 'rgba(16, 185, 129, 0.4)',
  },
  discountCounterText: {
    fontSize: 10,
    fontWeight: '800',
    color: theme.colors.discount,
  },
  gameCountText: {
    fontSize: 12,
    color: theme.colors.textMuted,
    fontWeight: '600',
  },
  horizontalList: {
    paddingRight: 16,
  },
  emptyContainer: {
    backgroundColor: theme.colors.card,
    borderRadius: theme.borderRadius.lg,
    padding: 30,
    alignItems: 'center',
    borderWidth: 1,
    borderColor: theme.colors.borderSubtle,
  },
  emptyText: {
    fontSize: 13,
    color: theme.colors.textMuted,
    marginTop: 8,
    marginBottom: 14,
  },
  resetFilterButton: {
    paddingHorizontal: 14,
    paddingVertical: 7,
    backgroundColor: 'rgba(0, 242, 254, 0.12)',
    borderRadius: theme.borderRadius.sm,
    borderWidth: 1,
    borderColor: theme.colors.primary,
  },
  resetFilterText: {
    fontSize: 12,
    color: theme.colors.primary,
    fontWeight: '700',
  },
});
