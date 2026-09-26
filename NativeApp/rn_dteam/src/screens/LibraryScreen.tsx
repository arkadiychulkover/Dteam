import React, { useState, useEffect, useCallback } from 'react';
import {
  View,
  Text,
  TextInput,
  TouchableOpacity,
  ScrollView,
  FlatList,
  StyleSheet,
  ActivityIndicator,
  StatusBar,
  RefreshControl,
  Dimensions,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { Ionicons } from '@expo/vector-icons';
import { libraryService } from '../services/libraryService';
import { useAuthStore } from '../store/useAuthStore';
import { BackendImage } from '../components/BackendImage';
import { AuthRequiredView } from '../components/common/AuthRequiredView';
import { formatPlayTime } from '../utils/formatters';
import { theme } from '../styles/theme';
import type { UserGame, Game } from '../types';

interface LibraryScreenProps {
  navigation?: any;
  onSelectLibraryGame?: (game: Game, userGame: UserGame) => void;
  onNavigateCatalog?: () => void;
  onNavigateLogin?: () => void;
}

const { width: SCREEN_WIDTH } = Dimensions.get('window');
const GRID_ITEM_WIDTH = (SCREEN_WIDTH - 48) / 2;

export const LibraryScreen: React.FC<LibraryScreenProps> = ({
  navigation,
  onSelectLibraryGame,
  onNavigateCatalog,
  onNavigateLogin,
}) => {
  const { isAuthenticated } = useAuthStore();
  const [items, setItems] = useState<UserGame[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isRefreshing, setIsRefreshing] = useState(false);
  const [error, setError] = useState('');

  const [searchQuery, setSearchQuery] = useState('');
  const [activeTab, setActiveTab] = useState<'all' | 'favorites'>('all');
  const [viewMode, setViewMode] = useState<'grid' | 'list'>('grid');

  const fetchLibrary = useCallback(async () => {
    if (!isAuthenticated) return;
    try {
      setError('');
      setIsLoading(true);
      const data = await libraryService.getUserLibrary();
      setItems(data || []);
    } catch (err: any) {
      console.warn('[LibraryScreen] Failed to load library:', err);
      setError(err.message || 'Не вдалося завантажити бібліотеку');
    } finally {
      setIsLoading(false);
      setIsRefreshing(false);
    }
  }, [isAuthenticated]);

  useEffect(() => {
    if (isAuthenticated) {
      fetchLibrary();
    } else {
      setIsLoading(false);
      setItems([]);
    }
  }, [fetchLibrary, isAuthenticated]);

  if (!isAuthenticated) {
    return (
      <AuthRequiredView
        screenTitle="МОЯ БІБЛІОТЕКА"
        screenSubtitle="Колекція ваших куплених ігор"
        icon="game-controller-outline"
        title="Потрібна авторизація"
        description="Увійдіть у ваш акаунт DTEAM, щоб переглядати, завантажувати та грати у придбані ігри."
        onLogin={() => {
          if (onNavigateLogin) {
            onNavigateLogin();
          } else if (navigation?.navigate) {
            navigation.navigate('Login');
          }
        }}
      />
    );
  }

  const handleRefresh = () => {
    setIsRefreshing(true);
    fetchLibrary();
  };

  const handleToggleFavorite = async (gameId: string) => {
    setItems((prev) =>
      prev.map((item) =>
        item.gameId === gameId ? { ...item, isFavorite: !item.isFavorite } : item
      )
    );

    try {
      const res = await libraryService.toggleFavorite(gameId);
      setItems((prev) =>
        prev.map((item) =>
          item.gameId === gameId ? { ...item, isFavorite: res.isFavorite } : item
        )
      );
    } catch (err) {
      console.warn('[LibraryScreen] Failed to toggle favorite:', err);
      setItems((prev) =>
        prev.map((item) =>
          item.gameId === gameId ? { ...item, isFavorite: !item.isFavorite } : item
        )
      );
    }
  };

  const handleOpenGame = (item: UserGame) => {
    if (!item.game) return;
    if (onSelectLibraryGame) {
      onSelectLibraryGame(item.game, item);
    } else if (navigation?.navigate) {
      navigation.navigate('LibraryGame', { game: item.game, userGame: item });
    }
  };

  const goToCatalog = () => {
    if (onNavigateCatalog) {
      onNavigateCatalog();
    } else if (navigation?.navigate) {
      navigation.navigate('Home');
    }
  };

  const filteredItems = items.filter((item) => {
    if (!item.game) return false;
    if (activeTab === 'favorites' && !item.isFavorite) return false;
    if (searchQuery.trim()) {
      const q = searchQuery.toLowerCase();
      return item.game.title.toLowerCase().includes(q);
    }
    return true;
  });

  const favoritesCount = items.filter((i) => i.isFavorite).length;

  return (
    <SafeAreaView style={styles.safeArea} edges={['top', 'left', 'right']}>
      <StatusBar barStyle="light-content" backgroundColor={theme.colors.background} />

      <View style={styles.header}>
        <View style={styles.logoRow}>
          <TouchableOpacity
            style={styles.backBtn}
            onPress={() => (navigation?.canGoBack?.() ? navigation.goBack() : navigation?.navigate?.('Home'))}
            activeOpacity={0.7}
          >
            <Ionicons name="arrow-back" size={20} color={theme.colors.text} />
          </TouchableOpacity>
          <View style={styles.logoIcon}>
            <Ionicons name="game-controller-outline" size={18} color="#000" />
          </View>
          <View>
            <Text style={styles.logoTitle}>МОЯ БІБЛІОТЕКА</Text>
            <Text style={styles.logoSubtitle}>{items.length} ІГОР У КОЛЕКЦІЇ</Text>
          </View>
        </View>

        <View style={styles.headerRight}>
          <TouchableOpacity
            style={[styles.viewModeBtn, viewMode === 'grid' && styles.viewModeBtnActive]}
            onPress={() => setViewMode('grid')}
            activeOpacity={0.7}
          >
            <Ionicons
              name="grid-outline"
              size={16}
              color={viewMode === 'grid' ? theme.colors.primary : theme.colors.textMuted}
            />
          </TouchableOpacity>
          <TouchableOpacity
            style={[styles.viewModeBtn, viewMode === 'list' && styles.viewModeBtnActive]}
            onPress={() => setViewMode('list')}
            activeOpacity={0.7}
          >
            <Ionicons
              name="list-outline"
              size={16}
              color={viewMode === 'list' ? theme.colors.primary : theme.colors.textMuted}
            />
          </TouchableOpacity>
        </View>
      </View>

      <View style={styles.controlsSection}>
        <View style={styles.searchWrapper}>
          <Ionicons name="search-outline" size={16} color={theme.colors.primary} style={styles.searchIcon} />
          <TextInput
            style={styles.searchInput}
            value={searchQuery}
            onChangeText={setSearchQuery}
            placeholder="Пошук у бібліотеці..."
            placeholderTextColor={theme.colors.textDim}
          />
          {searchQuery ? (
            <TouchableOpacity onPress={() => setSearchQuery('')} activeOpacity={0.7}>
              <Ionicons name="close-circle" size={16} color={theme.colors.textDim} />
            </TouchableOpacity>
          ) : null}
        </View>

        <View style={styles.tabsRow}>
          <TouchableOpacity
            style={[styles.tabPill, activeTab === 'all' && styles.tabPillActive]}
            onPress={() => setActiveTab('all')}
            activeOpacity={0.8}
          >
            <Text style={[styles.tabPillText, activeTab === 'all' && styles.tabPillTextActive]}>
              Усі ігри ({items.length})
            </Text>
          </TouchableOpacity>

          <TouchableOpacity
            style={[styles.tabPill, activeTab === 'favorites' && styles.tabPillActive]}
            onPress={() => setActiveTab('favorites')}
            activeOpacity={0.8}
          >
            <Ionicons
              name="heart"
              size={13}
              color={activeTab === 'favorites' ? theme.colors.primary : theme.colors.textMuted}
              style={{ marginRight: 4 }}
            />
            <Text
              style={[styles.tabPillText, activeTab === 'favorites' && styles.tabPillTextActive]}
            >
              Обране ({favoritesCount})
            </Text>
          </TouchableOpacity>
        </View>
      </View>

      <ScrollView
        showsVerticalScrollIndicator={false}
        contentContainerStyle={styles.scrollContent}
        refreshControl={
          <RefreshControl
            refreshing={isRefreshing}
            onRefresh={handleRefresh}
            tintColor={theme.colors.primary}
            colors={[theme.colors.primary]}
          />
        }
      >
        {isLoading ? (
          <View style={styles.loaderCenter}>
            <ActivityIndicator size="large" color={theme.colors.primary} />
            <Text style={styles.loaderText}>Завантаження бібліотеки...</Text>
          </View>
        ) : error ? (
          <View style={styles.errorContainer}>
            <Ionicons name="lock-closed-outline" size={44} color={theme.colors.primary} />
            <Text style={styles.errorTitle}>Потрібна авторизація</Text>
            <Text style={styles.errorSubtitle}>
              Увійдіть у ваш акаунт DTEAM, щоб переглядати та грати в куплені ігри.
            </Text>
            <TouchableOpacity
              style={styles.retryButton}
              onPress={() => navigation?.navigate?.('Login')}
              activeOpacity={0.8}
            >
              <Ionicons name="log-in-outline" size={16} color="#000" />
              <Text style={styles.retryText}>Увійти у DTEAM</Text>
            </TouchableOpacity>
          </View>
        ) : filteredItems.length === 0 ? (
          <View style={styles.emptyContainer}>
            <View style={styles.emptyIconCircle}>
              <Ionicons
                name={activeTab === 'favorites' ? 'heart-outline' : 'game-controller-outline'}
                size={36}
                color={theme.colors.primary}
              />
            </View>
            <Text style={styles.emptyTitle}>
              {activeTab === 'favorites' ? 'Немає обраних ігор' : 'Бібліотека порожня'}
            </Text>
            <Text style={styles.emptySubtitle}>
              {activeTab === 'favorites'
                ? 'Позначайте улюблені ігри сердечком ♥, щоб швидко знаходити їх тут.'
                : 'Придбайте ігри в крамниці Dteam, щоб вони з’явилися у вашій бібліотеці.'}
            </Text>
            <TouchableOpacity style={styles.catalogButton} onPress={goToCatalog} activeOpacity={0.8}>
              <Ionicons name="planet-outline" size={16} color="#000" />
              <Text style={styles.catalogButtonText}>Перейти до каталогу ігор</Text>
            </TouchableOpacity>
          </View>
        ) : viewMode === 'grid' ? (
          <View style={styles.gridContainer}>
            {filteredItems.map((item) => {
              const g = item.game!;
              return (
                <TouchableOpacity
                  key={item.gameId}
                  style={styles.gridCard}
                  onPress={() => handleOpenGame(item)}
                  activeOpacity={0.8}
                >
                  <View style={styles.gridImageWrapper}>
                    <BackendImage
                      src={g.coverImageUrl || g.headerImageUrl}
                      style={styles.gridImage}
                      resizeMode="cover"
                    />
                    <TouchableOpacity
                      style={[
                        styles.favoriteHeartBtn,
                        item.isFavorite && styles.favoriteHeartBtnActive,
                      ]}
                      onPress={() => handleToggleFavorite(item.gameId)}
                      activeOpacity={0.8}
                    >
                      <Ionicons
                        name={item.isFavorite ? 'heart' : 'heart-outline'}
                        size={16}
                        color={item.isFavorite ? '#f43f5e' : theme.colors.textMuted}
                      />
                    </TouchableOpacity>
                  </View>
                  <View style={styles.gridInfo}>
                    <Text style={styles.gridTitle} numberOfLines={1}>
                      {g.title}
                    </Text>
                    <Text style={styles.gridPlayTime}>
                      {formatPlayTime(item.playTimeMinutes)}
                    </Text>
                  </View>
                </TouchableOpacity>
              );
            })}
          </View>
        ) : (
          <View style={styles.listContainer}>
            {filteredItems.map((item) => {
              const g = item.game!;
              return (
                <TouchableOpacity
                  key={item.gameId}
                  style={styles.listCard}
                  onPress={() => handleOpenGame(item)}
                  activeOpacity={0.8}
                >
                  <BackendImage
                    src={g.headerImageUrl || g.coverImageUrl}
                    style={styles.listImage}
                    resizeMode="cover"
                  />
                  <View style={styles.listInfo}>
                    <Text style={styles.listTitle} numberOfLines={1}>
                      {g.title}
                    </Text>
                    <View style={styles.listMetaRow}>
                      <Ionicons name="time-outline" size={13} color={theme.colors.textDim} />
                      <Text style={styles.listPlayTime}>
                        {formatPlayTime(item.playTimeMinutes)}
                      </Text>
                    </View>
                  </View>

                  <TouchableOpacity
                    style={[
                      styles.listFavBtn,
                      item.isFavorite && styles.listFavBtnActive,
                    ]}
                    onPress={() => handleToggleFavorite(item.gameId)}
                    activeOpacity={0.8}
                  >
                    <Ionicons
                      name={item.isFavorite ? 'heart' : 'heart-outline'}
                      size={18}
                      color={item.isFavorite ? '#f43f5e' : theme.colors.textMuted}
                    />
                  </TouchableOpacity>
                </TouchableOpacity>
              );
            })}
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
  backBtn: {
    paddingRight: 4,
    paddingVertical: 4,
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
    fontSize: 15,
    fontWeight: '900',
    color: theme.colors.text,
    letterSpacing: 0.8,
  },
  logoSubtitle: {
    fontSize: 8,
    fontWeight: '800',
    color: theme.colors.primary,
    letterSpacing: 0.8,
  },
  headerRight: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 4,
    backgroundColor: theme.colors.card,
    borderRadius: 8,
    padding: 3,
    borderWidth: 1,
    borderColor: theme.colors.borderSubtle,
  },
  viewModeBtn: {
    paddingHorizontal: 8,
    paddingVertical: 4,
    borderRadius: 6,
  },
  viewModeBtnActive: {
    backgroundColor: 'rgba(0, 242, 254, 0.15)',
  },
  controlsSection: {
    backgroundColor: theme.colors.surface,
    paddingHorizontal: 16,
    paddingVertical: 10,
    borderBottomWidth: 1,
    borderBottomColor: theme.colors.borderSubtle,
    gap: 10,
  },
  searchWrapper: {
    flexDirection: 'row',
    alignItems: 'center',
    backgroundColor: theme.colors.inputBg,
    borderRadius: theme.borderRadius.md,
    borderWidth: 1,
    borderColor: theme.colors.borderSubtle,
    paddingHorizontal: 12,
    height: 40,
  },
  searchIcon: {
    marginRight: 8,
  },
  searchInput: {
    flex: 1,
    fontSize: 13,
    color: theme.colors.text,
  },
  tabsRow: {
    flexDirection: 'row',
    gap: 8,
  },
  tabPill: {
    flexDirection: 'row',
    alignItems: 'center',
    paddingHorizontal: 14,
    paddingVertical: 6,
    borderRadius: theme.borderRadius.full,
    backgroundColor: theme.colors.card,
    borderWidth: 1,
    borderColor: theme.colors.borderSubtle,
  },
  tabPillActive: {
    backgroundColor: 'rgba(0, 242, 254, 0.18)',
    borderColor: theme.colors.primary,
  },
  tabPillText: {
    fontSize: 12,
    fontWeight: '600',
    color: theme.colors.textMuted,
  },
  tabPillTextActive: {
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
  emptyContainer: {
    backgroundColor: theme.colors.card,
    borderRadius: theme.borderRadius.xl,
    padding: 30,
    alignItems: 'center',
    borderWidth: 1,
    borderColor: theme.colors.borderSubtle,
    marginVertical: 20,
  },
  emptyIconCircle: {
    width: 64,
    height: 64,
    borderRadius: 20,
    backgroundColor: 'rgba(0, 242, 254, 0.1)',
    borderWidth: 1,
    borderColor: 'rgba(0, 242, 254, 0.25)',
    justifyContent: 'center',
    alignItems: 'center',
    marginBottom: 14,
  },
  emptyTitle: {
    fontSize: 17,
    fontWeight: '900',
    color: theme.colors.text,
    marginBottom: 6,
  },
  emptySubtitle: {
    fontSize: 12,
    color: theme.colors.textMuted,
    textAlign: 'center',
    lineHeight: 18,
    marginBottom: 18,
    maxWidth: 280,
  },
  catalogButton: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 8,
    backgroundColor: theme.colors.primary,
    paddingHorizontal: 18,
    paddingVertical: 10,
    borderRadius: theme.borderRadius.md,
  },
  catalogButtonText: {
    fontSize: 13,
    fontWeight: '900',
    color: '#000',
  },
  gridContainer: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    gap: 16,
  },
  gridCard: {
    width: GRID_ITEM_WIDTH,
    backgroundColor: theme.colors.card,
    borderRadius: theme.borderRadius.lg,
    overflow: 'hidden',
    borderWidth: 1,
    borderColor: theme.colors.borderSubtle,
  },
  gridImageWrapper: {
    width: '100%',
    height: 140,
    position: 'relative',
  },
  gridImage: {
    width: '100%',
    height: '100%',
  },
  favoriteHeartBtn: {
    position: 'absolute',
    top: 8,
    right: 8,
    width: 30,
    height: 30,
    borderRadius: 15,
    backgroundColor: 'rgba(6, 24, 32, 0.8)',
    borderWidth: 1,
    borderColor: theme.colors.borderSubtle,
    alignItems: 'center',
    justifyContent: 'center',
  },
  favoriteHeartBtnActive: {
    backgroundColor: 'rgba(244, 63, 94, 0.2)',
    borderColor: 'rgba(244, 63, 94, 0.5)',
  },
  gridInfo: {
    padding: 10,
  },
  gridTitle: {
    fontSize: 13,
    fontWeight: '800',
    color: theme.colors.text,
    marginBottom: 4,
  },
  gridPlayTime: {
    fontSize: 11,
    color: theme.colors.textDim,
    fontWeight: '600',
  },
  listContainer: {
    gap: 12,
  },
  listCard: {
    flexDirection: 'row',
    alignItems: 'center',
    backgroundColor: theme.colors.card,
    borderRadius: theme.borderRadius.lg,
    padding: 10,
    borderWidth: 1,
    borderColor: theme.colors.borderSubtle,
    gap: 12,
  },
  listImage: {
    width: 80,
    height: 52,
    borderRadius: 8,
  },
  listInfo: {
    flex: 1,
  },
  listTitle: {
    fontSize: 14,
    fontWeight: '800',
    color: theme.colors.text,
    marginBottom: 4,
  },
  listMetaRow: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 4,
  },
  listPlayTime: {
    fontSize: 11,
    color: theme.colors.textMuted,
    fontWeight: '500',
  },
  listFavBtn: {
    width: 36,
    height: 36,
    borderRadius: 18,
    backgroundColor: theme.colors.surface,
    alignItems: 'center',
    justifyContent: 'center',
  },
  listFavBtnActive: {
    backgroundColor: 'rgba(244, 63, 94, 0.15)',
  },
});
