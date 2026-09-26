import React, { useEffect, useState } from 'react';
import {
  View,
  Text,
  StyleSheet,
  ScrollView,
  TouchableOpacity,
  StatusBar,
  Alert,
  ActivityIndicator,
  Modal,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { Ionicons } from '@expo/vector-icons';
import { useCartStore } from '../store/useCartStore';
import { useAuthStore } from '../store/useAuthStore';
import { BackendImage } from '../components/BackendImage';
import { AuthRequiredView } from '../components/common/AuthRequiredView';
import { formatPrice, formatBasePrice } from '../utils/formatters';
import { colors } from '../theme/colors';
import { theme } from '../styles/theme';
import type { CheckoutResult } from '../types';

interface CartScreenProps {
  navigation: any;
}

export const CartScreen: React.FC<CartScreenProps> = ({ navigation }) => {
  const { user, isAuthenticated, checkAuth } = useAuthStore();
  const {
    items,
    isLoading,
    isCheckingOut,
    totalBasePriceInNanoTons,
    totalEffectivePriceInNanoTons,
    totalSavingsInNanoTons,
    loadCart,
    removeFromCart,
    clearCart,
    checkout,
  } = useCartStore();

  const [isProcessing, setIsProcessing] = useState(false);
  const [modalType, setModalType] = useState<
    'none' | 'confirm' | 'insufficient' | 'success' | 'error' | 'clear'
  >('none');
  const [errorMessage, setErrorMessage] = useState('');
  const [successData, setSuccessData] = useState<CheckoutResult | null>(null);

  useEffect(() => {
    if (isAuthenticated) {
      loadCart();
    }
  }, [isAuthenticated, loadCart]);

  if (!isAuthenticated) {
    return (
      <AuthRequiredView
        screenTitle="Кошик"
        screenSubtitle="Оформлення та оплата замовлення"
        icon="cart-outline"
        title="Потрібна авторизація"
        description="Увійдіть у свій акаунт DTEAM, щоб переглядати додані до кошика ігри та здійснювати покупки."
        onLogin={() => navigation.navigate('Login')}
      />
    );
  }

  const userBalanceNanoTons = user?.balanceInNanoTons || 0;
  const userBalanceTon = (userBalanceNanoTons / 1e9).toFixed(2);
  const totalTon = (totalEffectivePriceInNanoTons / 1e9).toFixed(2);
  const isBalanceSufficient = userBalanceNanoTons >= totalEffectivePriceInNanoTons;

  const handleClearConfirm = () => {
    setModalType('clear');
  };

  const handleCheckoutPress = () => {
    console.log('[CartScreen] handleCheckoutPress clicked', {
      itemsCount: items.length,
      userBalanceNanoTons,
      userBalanceTon,
      totalEffectivePriceInNanoTons,
      totalTon,
      isBalanceSufficient,
    });

    if (items.length === 0) {
      console.warn('[CartScreen] Cart is empty, ignoring checkout press');
      return;
    }

    if (!isBalanceSufficient) {
      console.warn('[CartScreen] Insufficient balance for checkout');
      setModalType('insufficient');
      return;
    }

    setModalType('confirm');
  };

  const handleConfirmCheckout = async () => {
    console.log('[CartScreen] Starting checkout transaction on backend...');
    setIsProcessing(true);
    try {
      const res = await checkout();
      console.log('[CartScreen] Checkout finished successfully:', res);
      setSuccessData(res);
      setModalType('success');
    } catch (err: any) {
      console.error('[CartScreen] Checkout error:', err);
      setErrorMessage(
        err.message || 'Не вдалося здійснити покупку. Перевірте баланс або спробуйте пізніше.'
      );
      setModalType('error');
    } finally {
      setIsProcessing(false);
    }
  };

  return (
    <SafeAreaView style={styles.safeArea} edges={['top', 'left', 'right']}>
      <StatusBar barStyle="light-content" backgroundColor={colors.background} />

      {/* Верхня панель */}
      <View style={styles.header}>
        <TouchableOpacity
          style={styles.backBtn}
          onPress={() => navigation.goBack()}
          activeOpacity={0.7}
        >
          <Ionicons name="arrow-back" size={20} color={colors.primary} />
          <Text style={styles.backBtnText}>Назад</Text>
        </TouchableOpacity>

        <View style={styles.headerTitleRow}>
          <Text style={styles.headerTitle}>Кошик</Text>
          {items.length > 0 && (
            <View style={styles.badgeCount}>
              <Text style={styles.badgeCountText}>{items.length}</Text>
            </View>
          )}
        </View>

        {items.length > 0 ? (
          <TouchableOpacity
            style={styles.clearBtn}
            onPress={handleClearConfirm}
            activeOpacity={0.7}
          >
            <Ionicons name="trash-outline" size={18} color={colors.textDim} />
          </TouchableOpacity>
        ) : (
          <View style={{ width: 40 }} />
        )}
      </View>

      {isLoading ? (
        <View style={styles.centerBox}>
          <ActivityIndicator size="large" color={colors.primary} />
          <Text style={styles.loadingText}>Завантаження кошика...</Text>
        </View>
      ) : items.length === 0 ? (
        <View style={styles.emptyContainer}>
          <View style={styles.emptyIconCircle}>
            <Ionicons name="cart-outline" size={48} color={colors.primary} />
          </View>
          <Text style={styles.emptyTitle}>Ваш кошик порожній</Text>
          <Text style={styles.emptySubtitle}>
            Досліджуйте каталог DTEAM, знаходьте найгарячіші новинки та додавайте їх до кошика!
          </Text>

          <TouchableOpacity
            style={styles.exploreCatalogBtn}
            onPress={() => navigation.navigate('MainTabs', { initialTab: 'store' })}
            activeOpacity={0.8}
          >
            <Ionicons name="game-controller-outline" size={18} color="#000" />
            <Text style={styles.exploreCatalogBtnText}>Перейти до каталогу</Text>
          </TouchableOpacity>
        </View>
      ) : (
        <ScrollView
          showsVerticalScrollIndicator={false}
          contentContainerStyle={styles.scrollContent}
        >
          {/* Список товарів */}
          <View style={styles.itemsSection}>
            <Text style={styles.sectionTitle}>Обрані ігри ({items.length})</Text>

            {items.map((item) => {
              const game = item.game;
              if (!game) return null;
              const hasDiscount = (game.discountPercentage || 0) > 0;

              return (
                <View key={item.gameId} style={styles.cartCard}>
                  <BackendImage
                    src={game.coverImageUrl || game.headerImageUrl}
                    style={styles.cartCardImage}
                    resizeMode="cover"
                  />

                  <View style={styles.cartCardInfo}>
                    <Text style={styles.cartCardTitle} numberOfLines={1}>
                      {game.title}
                    </Text>

                    {game.genres && game.genres.length > 0 && (
                      <Text style={styles.cartCardGenre} numberOfLines={1}>
                        {game.genres.slice(0, 2).join(' • ')}
                      </Text>
                    )}

                    <View style={styles.cartCardPriceRow}>
                      {hasDiscount && (
                        <Text style={styles.cartCardOriginalPrice}>
                          {formatBasePrice(game.priceInNanoTons)}
                        </Text>
                      )}
                      <Text
                        style={[
                          styles.cartCardFinalPrice,
                          hasDiscount && styles.cartCardDiscountedPrice,
                        ]}
                      >
                        {formatPrice(game.priceInNanoTons, game.discountPercentage)}
                      </Text>

                      {hasDiscount && (
                        <View style={styles.cartCardDiscountBadge}>
                          <Text style={styles.cartCardDiscountText}>
                            -{game.discountPercentage}%
                          </Text>
                        </View>
                      )}
                    </View>
                  </View>

                  <TouchableOpacity
                    style={styles.removeBtn}
                    onPress={() => removeFromCart(item.gameId)}
                    activeOpacity={0.7}
                  >
                    <Ionicons name="close-circle-outline" size={22} color={colors.textDim} />
                  </TouchableOpacity>
                </View>
              );
            })}
          </View>

          {/* Підсумок замовлення */}
          <View style={styles.summaryCard}>
            <Text style={styles.summaryTitle}>Підсумок замовлення</Text>

            <View style={styles.summaryRow}>
              <Text style={styles.summaryLabel}>Кількість ігор</Text>
              <Text style={styles.summaryValue}>{items.length}</Text>
            </View>

            {totalSavingsInNanoTons > 0 && (
              <View style={styles.summaryRow}>
                <Text style={styles.summaryLabel}>Заощаджено на знижках</Text>
                <Text style={styles.summarySavings}>
                  -{(totalSavingsInNanoTons / 1e9).toFixed(2)} TON
                </Text>
              </View>
            )}

            <View style={styles.summaryDivider} />

            <View style={styles.summaryRowTotal}>
              <Text style={styles.summaryTotalLabel}>Загальна сума:</Text>
              <Text style={styles.summaryTotalValue}>{totalTon} TON</Text>
            </View>

            {/* Баланс користувача */}
            <View
              style={[
                styles.balanceBanner,
                !isBalanceSufficient && styles.balanceBannerWarning,
              ]}
            >
              <View style={styles.balanceBannerLeft}>
                <Ionicons
                  name={isBalanceSufficient ? 'wallet-outline' : 'warning-outline'}
                  size={18}
                  color={isBalanceSufficient ? colors.primary : colors.accentRose}
                />
                <Text style={styles.balanceBannerText}>
                  Ваш баланс: <Text style={{ fontWeight: '800' }}>{userBalanceTon} TON</Text>
                </Text>
              </View>

              {!isBalanceSufficient && (
                <Text style={styles.balanceBannerShortfall}>
                  Не вистачає {(Math.max(0, totalEffectivePriceInNanoTons - userBalanceNanoTons) / 1e9).toFixed(2)} TON
                </Text>
              )}
            </View>

            {/* Велика кнопка покупки */}
            <TouchableOpacity
              style={[
                styles.checkoutBtn,
                (isProcessing || isCheckingOut) && styles.checkoutBtnDisabled,
              ]}
              onPress={handleCheckoutPress}
              disabled={isProcessing || isCheckingOut}
              activeOpacity={0.8}
            >
              {isProcessing || isCheckingOut ? (
                <View style={styles.btnRow}>
                  <ActivityIndicator size="small" color="#000" />
                  <Text style={styles.checkoutBtnText}>Обробка оплати...</Text>
                </View>
              ) : (
                <View style={styles.btnRow}>
                  <Ionicons name="card-outline" size={18} color="#000" />
                  <Text style={styles.checkoutBtnText}>
                    {isBalanceSufficient
                      ? `Оплатити замовлення (${totalTon} TON)`
                      : `Поповнити баланс (${totalTon} TON)`}
                  </Text>
                </View>
              )}
            </TouchableOpacity>

            {!isBalanceSufficient && (
              <TouchableOpacity
                style={styles.depositLinkBtn}
                onPress={() => navigation.navigate('Settings')}
                activeOpacity={0.7}
              >
                <Text style={styles.depositLinkText}>
                  Поповнити баланс у налаштуваннях →
                </Text>
              </TouchableOpacity>
            )}
          </View>
        </ScrollView>
      )}

      {/* Інтерактивні модальні діалоги замість ненадійних браузерних Alert */}
      <Modal
        visible={modalType !== 'none'}
        transparent
        animationType="fade"
        onRequestClose={() => {
          if (!isProcessing) setModalType('none');
        }}
      >
        <View style={styles.modalBackdrop}>
          <View style={styles.modalCard}>
            {modalType === 'confirm' && (
              <>
                <View
                  style={[
                    styles.modalIconWrap,
                    {
                      backgroundColor: 'rgba(0, 242, 254, 0.15)',
                      borderColor: colors.primary,
                    },
                  ]}
                >
                  <Ionicons name="card-outline" size={32} color={colors.primary} />
                </View>
                <Text style={styles.modalTitle}>Підтвердження покупки</Text>
                <Text style={styles.modalDesc}>
                  Ви збираєтесь списати кошти з вашого TON-балансу для оплати {items.length}{' '}
                  {items.length === 1 ? 'гри' : 'ігор'}.
                </Text>

                <View style={styles.modalInfoBox}>
                  <View style={styles.modalInfoRow}>
                    <Text style={styles.modalInfoLabel}>Сума до сплати:</Text>
                    <Text style={[styles.modalInfoVal, { color: colors.primary }]}>
                      {totalTon} TON
                    </Text>
                  </View>
                  <View style={styles.modalInfoRow}>
                    <Text style={styles.modalInfoLabel}>Поточний баланс:</Text>
                    <Text style={styles.modalInfoVal}>{userBalanceTon} TON</Text>
                  </View>
                  <View style={styles.modalInfoDivider} />
                  <View style={styles.modalInfoRow}>
                    <Text style={styles.modalInfoLabel}>Залишок після покупки:</Text>
                    <Text style={[styles.modalInfoVal, { color: colors.accentEmerald }]}>
                      {(
                        Math.max(0, userBalanceNanoTons - totalEffectivePriceInNanoTons) / 1e9
                      ).toFixed(2)}{' '}
                      TON
                    </Text>
                  </View>
                </View>

                <TouchableOpacity
                  style={[styles.modalActionBtn, styles.modalActionBtnPrimary]}
                  onPress={handleConfirmCheckout}
                  disabled={isProcessing}
                  activeOpacity={0.8}
                >
                  {isProcessing ? (
                    <ActivityIndicator size="small" color="#000" />
                  ) : (
                    <>
                      <Ionicons name="checkmark-circle-outline" size={18} color="#000" />
                      <Text style={styles.modalActionBtnPrimaryText}>
                        Оплатити {totalTon} TON
                      </Text>
                    </>
                  )}
                </TouchableOpacity>

                <TouchableOpacity
                  style={styles.modalActionBtnCancel}
                  onPress={() => setModalType('none')}
                  disabled={isProcessing}
                  activeOpacity={0.7}
                >
                  <Text style={styles.modalActionBtnCancelText}>Скасувати</Text>
                </TouchableOpacity>
              </>
            )}

            {modalType === 'insufficient' && (
              <>
                <View
                  style={[
                    styles.modalIconWrap,
                    {
                      backgroundColor: 'rgba(255, 75, 75, 0.15)',
                      borderColor: colors.accentRose,
                    },
                  ]}
                >
                  <Ionicons name="warning-outline" size={32} color={colors.accentRose} />
                </View>
                <Text style={styles.modalTitle}>Недостатньо коштів</Text>
                <Text style={styles.modalDesc}>
                  Для оплати замовлення необхідно {totalTon} TON, а на вашому балансі лише{' '}
                  {userBalanceTon} TON.
                </Text>

                <View style={styles.modalInfoBox}>
                  <View style={styles.modalInfoRow}>
                    <Text style={styles.modalInfoLabel}>Не вистачає:</Text>
                    <Text style={[styles.modalInfoVal, { color: colors.accentRose }]}>
                      {(
                        Math.max(0, totalEffectivePriceInNanoTons - userBalanceNanoTons) / 1e9
                      ).toFixed(2)}{' '}
                      TON
                    </Text>
                  </View>
                </View>

                <TouchableOpacity
                  style={[styles.modalActionBtn, styles.modalActionBtnPrimary]}
                  onPress={() => {
                    setModalType('none');
                    navigation.navigate('Settings');
                  }}
                  activeOpacity={0.8}
                >
                  <Ionicons name="wallet-outline" size={18} color="#000" />
                  <Text style={styles.modalActionBtnPrimaryText}>Поповнити баланс</Text>
                </TouchableOpacity>

                <TouchableOpacity
                  style={styles.modalActionBtnCancel}
                  onPress={() => setModalType('none')}
                  activeOpacity={0.7}
                >
                  <Text style={styles.modalActionBtnCancelText}>Зрозуміло</Text>
                </TouchableOpacity>
              </>
            )}

            {modalType === 'success' && (
              <>
                <View
                  style={[
                    styles.modalIconWrap,
                    {
                      backgroundColor: 'rgba(16, 185, 129, 0.15)',
                      borderColor: colors.accentEmerald,
                    },
                  ]}
                >
                  <Ionicons name="trophy-outline" size={34} color={colors.accentEmerald} />
                </View>
                <Text style={styles.modalTitle}>Успішна покупка! 🎉</Text>
                <Text style={styles.modalDesc}>
                  {successData?.message || 'Ігри успішно додано до вашої бібліотеки Dteam!'}
                </Text>

                <View style={styles.modalInfoBox}>
                  <View style={styles.modalInfoRow}>
                    <Text style={styles.modalInfoLabel}>Придбано ігор:</Text>
                    <Text style={styles.modalInfoVal}>
                      {successData?.purchasedGamesCount || items.length}
                    </Text>
                  </View>
                  {Boolean(successData?.awardedTokens) && (
                    <View style={styles.modalInfoRow}>
                      <Text style={styles.modalInfoLabel}>Бонусний кешбек:</Text>
                      <Text style={[styles.modalInfoVal, { color: colors.accentAmber }]}>
                        +{successData?.awardedTokens} TDP
                      </Text>
                    </View>
                  )}
                  <View style={styles.modalInfoRow}>
                    <Text style={styles.modalInfoLabel}>Новий баланс TON:</Text>
                    <Text style={[styles.modalInfoVal, { color: colors.primary }]}>
                      {(
                        (successData?.newBalanceInNanoTons ?? userBalanceNanoTons) / 1e9
                      ).toFixed(2)}{' '}
                      TON
                    </Text>
                  </View>
                </View>

                <TouchableOpacity
                  style={[styles.modalActionBtn, styles.modalActionBtnPrimary]}
                  onPress={() => {
                    setModalType('none');
                    navigation.navigate('MainTabs', { initialTab: 'store' });
                  }}
                  activeOpacity={0.8}
                >
                  <Ionicons name="game-controller-outline" size={18} color="#000" />
                  <Text style={styles.modalActionBtnPrimaryText}>До каталогу ігор</Text>
                </TouchableOpacity>

                <TouchableOpacity
                  style={styles.modalActionBtnCancel}
                  onPress={() => {
                    setModalType('none');
                    navigation.navigate('Library');
                  }}
                  activeOpacity={0.7}
                >
                  <Text
                    style={[
                      styles.modalActionBtnCancelText,
                      { color: colors.primary, fontWeight: '700' },
                    ]}
                  >
                    Переглянути бібліотеку →
                  </Text>
                </TouchableOpacity>
              </>
            )}

            {modalType === 'error' && (
              <>
                <View
                  style={[
                    styles.modalIconWrap,
                    {
                      backgroundColor: 'rgba(255, 75, 75, 0.15)',
                      borderColor: colors.accentRose,
                    },
                  ]}
                >
                  <Ionicons name="alert-circle-outline" size={32} color={colors.accentRose} />
                </View>
                <Text style={styles.modalTitle}>Помилка оплати</Text>
                <Text style={styles.modalDesc}>{errorMessage}</Text>

                <TouchableOpacity
                  style={[styles.modalActionBtn, styles.modalActionBtnPrimary]}
                  onPress={() => setModalType('none')}
                  activeOpacity={0.8}
                >
                  <Text style={styles.modalActionBtnPrimaryText}>Зрозуміло</Text>
                </TouchableOpacity>
              </>
            )}

            {modalType === 'clear' && (
              <>
                <View
                  style={[
                    styles.modalIconWrap,
                    {
                      backgroundColor: 'rgba(255, 75, 75, 0.15)',
                      borderColor: colors.accentRose,
                    },
                  ]}
                >
                  <Ionicons name="trash-outline" size={32} color={colors.accentRose} />
                </View>
                <Text style={styles.modalTitle}>Очистити кошик?</Text>
                <Text style={styles.modalDesc}>
                  Ви впевнені, що хочете видалити всі ігри зі свого кошика?
                </Text>

                <TouchableOpacity
                  style={[styles.modalActionBtn, { backgroundColor: colors.accentRose }]}
                  onPress={async () => {
                    await clearCart();
                    setModalType('none');
                  }}
                  activeOpacity={0.8}
                >
                  <Text style={[styles.modalActionBtnPrimaryText, { color: '#fff' }]}>
                    Видалити все
                  </Text>
                </TouchableOpacity>

                <TouchableOpacity
                  style={styles.modalActionBtnCancel}
                  onPress={() => setModalType('none')}
                  activeOpacity={0.7}
                >
                  <Text style={styles.modalActionBtnCancelText}>Скасувати</Text>
                </TouchableOpacity>
              </>
            )}
          </View>
        </View>
      </Modal>
    </SafeAreaView>
  );
};

const styles = StyleSheet.create({
  safeArea: {
    flex: 1,
    backgroundColor: colors.background,
  },
  header: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    paddingHorizontal: 16,
    paddingVertical: 12,
    borderBottomWidth: 1,
    borderBottomColor: colors.border,
    backgroundColor: colors.surfaceCard,
  },
  backBtn: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 4,
    paddingVertical: 4,
  },
  backBtnText: {
    fontSize: 14,
    fontWeight: '700',
    color: colors.primary,
  },
  headerTitleRow: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 8,
  },
  headerTitle: {
    fontSize: 18,
    fontWeight: '900',
    color: colors.text,
  },
  badgeCount: {
    backgroundColor: colors.primary,
    paddingHorizontal: 8,
    paddingVertical: 2,
    borderRadius: 10,
  },
  badgeCountText: {
    fontSize: 11,
    fontWeight: '900',
    color: '#000',
  },
  clearBtn: {
    padding: 6,
  },
  centerBox: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
    gap: 12,
  },
  loadingText: {
    color: colors.textMuted,
    fontSize: 13,
  },
  emptyContainer: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
    paddingHorizontal: 32,
    gap: 12,
  },
  emptyIconCircle: {
    width: 88,
    height: 88,
    borderRadius: 44,
    backgroundColor: 'rgba(34, 211, 238, 0.1)',
    justifyContent: 'center',
    alignItems: 'center',
    marginBottom: 8,
    borderWidth: 1,
    borderColor: 'rgba(34, 211, 238, 0.25)',
  },
  emptyTitle: {
    fontSize: 18,
    fontWeight: '900',
    color: colors.text,
  },
  emptySubtitle: {
    fontSize: 13,
    color: colors.textMuted,
    textAlign: 'center',
    lineHeight: 18,
  },
  exploreCatalogBtn: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 8,
    backgroundColor: colors.primary,
    paddingHorizontal: 20,
    paddingVertical: 14,
    borderRadius: 14,
    marginTop: 12,
  },
  exploreCatalogBtnText: {
    color: '#000',
    fontSize: 14,
    fontWeight: '800',
  },
  scrollContent: {
    padding: 16,
    paddingBottom: 40,
  },
  itemsSection: {
    marginBottom: 20,
  },
  sectionTitle: {
    fontSize: 15,
    fontWeight: '800',
    color: colors.text,
    marginBottom: 12,
  },
  cartCard: {
    flexDirection: 'row',
    backgroundColor: colors.surfaceCard,
    borderRadius: 16,
    padding: 12,
    marginBottom: 10,
    borderWidth: 1,
    borderColor: colors.border,
    alignItems: 'center',
    gap: 12,
  },
  cartCardImage: {
    width: 72,
    height: 52,
    borderRadius: 10,
    backgroundColor: colors.surface,
  },
  cartCardInfo: {
    flex: 1,
  },
  cartCardTitle: {
    fontSize: 14,
    fontWeight: '800',
    color: colors.text,
    marginBottom: 2,
  },
  cartCardGenre: {
    fontSize: 11,
    color: colors.textDim,
    marginBottom: 4,
  },
  cartCardPriceRow: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 6,
  },
  cartCardOriginalPrice: {
    fontSize: 11,
    color: colors.textDim,
    textDecorationLine: 'line-through',
  },
  cartCardFinalPrice: {
    fontSize: 13,
    fontWeight: '800',
    color: colors.text,
    fontFamily: 'monospace',
  },
  cartCardDiscountedPrice: {
    color: colors.accentEmerald,
  },
  cartCardDiscountBadge: {
    backgroundColor: colors.accentEmerald,
    paddingHorizontal: 5,
    paddingVertical: 1,
    borderRadius: 4,
  },
  cartCardDiscountText: {
    fontSize: 9,
    fontWeight: '900',
    color: '#000',
  },
  removeBtn: {
    padding: 6,
  },
  summaryCard: {
    backgroundColor: colors.surfaceCard,
    borderRadius: 20,
    padding: 18,
    borderWidth: 1,
    borderColor: colors.border,
  },
  summaryTitle: {
    fontSize: 16,
    fontWeight: '900',
    color: colors.text,
    marginBottom: 14,
  },
  summaryRow: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    marginBottom: 8,
  },
  summaryLabel: {
    fontSize: 13,
    color: colors.textMuted,
  },
  summaryValue: {
    fontSize: 13,
    fontWeight: '700',
    color: colors.text,
  },
  summarySavings: {
    fontSize: 13,
    fontWeight: '700',
    color: colors.accentEmerald,
  },
  summaryDivider: {
    height: 1,
    backgroundColor: colors.border,
    marginVertical: 12,
  },
  summaryRowTotal: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    marginBottom: 16,
  },
  summaryTotalLabel: {
    fontSize: 16,
    fontWeight: '900',
    color: colors.text,
  },
  summaryTotalValue: {
    fontSize: 20,
    fontWeight: '900',
    color: colors.primary,
    fontFamily: 'monospace',
  },
  balanceBanner: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    backgroundColor: 'rgba(34, 211, 238, 0.1)',
    borderRadius: 12,
    padding: 10,
    marginBottom: 16,
    borderWidth: 1,
    borderColor: 'rgba(34, 211, 238, 0.25)',
  },
  balanceBannerWarning: {
    backgroundColor: 'rgba(244, 63, 94, 0.1)',
    borderColor: 'rgba(244, 63, 94, 0.3)',
  },
  balanceBannerLeft: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 6,
  },
  balanceBannerText: {
    fontSize: 12,
    color: colors.text,
  },
  balanceBannerShortfall: {
    fontSize: 11,
    fontWeight: '700',
    color: colors.accentRose,
  },
  checkoutBtn: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'center',
    gap: 8,
    backgroundColor: colors.primary,
    borderRadius: 14,
    paddingVertical: 14,
  },
  checkoutBtnDisabled: {
    opacity: 0.6,
  },
  checkoutBtnText: {
    fontSize: 15,
    fontWeight: '900',
    color: '#000',
  },
  depositLinkBtn: {
    alignItems: 'center',
    marginTop: 10,
    paddingVertical: 6,
  },
  depositLinkText: {
    fontSize: 12,
    color: colors.primary,
    fontWeight: '700',
  },
  modalBackdrop: {
    flex: 1,
    backgroundColor: 'rgba(0, 0, 0, 0.85)',
    justifyContent: 'center',
    alignItems: 'center',
    padding: 20,
  },
  modalCard: {
    width: '100%',
    maxWidth: 360,
    backgroundColor: '#0c101c',
    borderRadius: 20,
    borderWidth: 1,
    borderColor: 'rgba(0, 242, 254, 0.25)',
    padding: 22,
    alignItems: 'center',
    shadowColor: '#00f2fe',
    shadowOffset: { width: 0, height: 10 },
    shadowOpacity: 0.3,
    shadowRadius: 20,
    elevation: 10,
  },
  modalIconWrap: {
    width: 62,
    height: 62,
    borderRadius: 31,
    borderWidth: 1.5,
    alignItems: 'center',
    justifyContent: 'center',
    marginBottom: 14,
  },
  modalTitle: {
    fontSize: 18,
    fontWeight: '800',
    color: colors.text,
    textAlign: 'center',
    marginBottom: 8,
  },
  modalDesc: {
    fontSize: 13,
    color: colors.textMuted,
    textAlign: 'center',
    lineHeight: 18,
    marginBottom: 16,
  },
  modalInfoBox: {
    width: '100%',
    backgroundColor: 'rgba(255, 255, 255, 0.03)',
    borderWidth: 1,
    borderColor: 'rgba(255, 255, 255, 0.08)',
    borderRadius: 12,
    padding: 12,
    gap: 8,
    marginBottom: 18,
  },
  modalInfoRow: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
  },
  modalInfoLabel: {
    fontSize: 12,
    color: colors.textDim,
  },
  modalInfoVal: {
    fontSize: 13,
    fontWeight: '700',
    color: colors.text,
  },
  modalInfoDivider: {
    height: 1,
    backgroundColor: 'rgba(255, 255, 255, 0.06)',
    marginVertical: 4,
  },
  modalActionBtn: {
    width: '100%',
    height: 46,
    borderRadius: 12,
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'center',
    gap: 8,
    marginBottom: 8,
  },
  modalActionBtnPrimary: {
    backgroundColor: colors.primary,
  },
  modalActionBtnPrimaryText: {
    fontSize: 14,
    fontWeight: '800',
    color: '#000',
  },
  modalActionBtnCancel: {
    paddingVertical: 8,
    paddingHorizontal: 16,
  },
  modalActionBtnCancelText: {
    fontSize: 13,
    fontWeight: '600',
    color: colors.textMuted,
  },
  btnRow: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 8,
  },
});
