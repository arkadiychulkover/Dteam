import React, { useState, useEffect } from 'react';
import { View, Text, StyleSheet, TouchableOpacity, Alert, ActivityIndicator } from 'react-native';
import { colors } from '../../theme/colors';
import { useAuthStore } from '../../store/useAuthStore';
import { nftService } from '../../services/nftService';
import { Ionicons } from '@expo/vector-icons';

interface WalletTabProps {}

export const WalletTab: React.FC<WalletTabProps> = () => {
  const { user } = useAuthStore();
  const [copied, setCopied] = useState(false);
  const [tdpBalance, setTdpBalance] = useState<number>(0);
  const [isLoadingBalance, setIsLoadingBalance] = useState(false);

  const walletAddress = user?.hardhatAddress || user?.walletAddress || '';
  const tonBalance = user?.balanceInNanoTons
    ? (user.balanceInNanoTons / 1e9).toFixed(2)
    : '0.00';

  useEffect(() => {
    if (walletAddress) {
      setIsLoadingBalance(true);
      nftService
        .getTdpBalance(walletAddress)
        .then((b) => setTdpBalance(b))
        .catch(() => setTdpBalance(0))
        .finally(() => setIsLoadingBalance(false));
    }
  }, [walletAddress]);

  const handleCopy = () => {
    if (!walletAddress) {
      Alert.alert('Гаманець', 'У вашому профілі ще не прив\'язано Web3 гаманець.');
      return;
    }
    setCopied(true);
    setTimeout(() => setCopied(false), 2000);
    Alert.alert('Скопійовано', `Адресу гаманця збережено:\n${walletAddress}`);
  };

  const handleDeposit = () => {
    Alert.alert(
      'Поповнення TON',
      'Для поповнення балансу здійсніть платіж через мережу TON або зв\'яжіть ваш гаманець у веб-версії DTEAM.',
      [{ text: 'Зрозуміло' }]
    );
  };

  return (
    <View style={styles.container}>
      <View style={styles.header}>
        <Text style={styles.title}>Web3 Гаманець & Баланс</Text>
        <Text style={styles.subtitle}>Керуйте криптовалютними та бонусно-бальними активами</Text>
      </View>

      <View style={styles.walletCard}>
        <View style={styles.walletTop}>
          <View style={styles.chainRow}>
            <View style={styles.chainDot} />
            <Text style={styles.chainText}>TON MAINNET</Text>
          </View>

          {walletAddress ? (
            <TouchableOpacity style={styles.copyBtn} onPress={handleCopy} activeOpacity={0.7}>
              <Ionicons
                name={copied ? 'checkmark-circle' : 'copy-outline'}
                size={14}
                color={copied ? colors.accentEmerald : colors.primary}
              />
              <Text style={[styles.copyBtnText, copied && { color: colors.accentEmerald }]}>
                {copied ? 'Скопійовано' : 'Копіювати'}
              </Text>
            </TouchableOpacity>
          ) : null}
        </View>

        <Text style={styles.addressText} numberOfLines={1}>
          {walletAddress || 'Гаманець не прив\'язано'}
        </Text>

        <View style={styles.balancesContainer}>
          <View style={styles.balanceBlock}>
            <Text style={styles.balanceLabel}>Бали DTEAM (TDP)</Text>
            {isLoadingBalance ? (
              <ActivityIndicator size="small" color={colors.primary} />
            ) : (
              <Text style={styles.balanceDtp}>{tdpBalance} TDP</Text>
            )}
          </View>

          <View style={styles.balanceDivider} />

          <View style={styles.balanceBlock}>
            <Text style={styles.balanceLabel}>Баланс TON</Text>
            <Text style={styles.balanceUah}>{tonBalance} TON</Text>
          </View>
        </View>

        <TouchableOpacity style={styles.depositBtn} onPress={handleDeposit} activeOpacity={0.8}>
          <Ionicons name="add-circle-outline" size={18} color="#021118" />
          <Text style={styles.depositBtnText}>Поповнити баланс</Text>
        </TouchableOpacity>
      </View>

      <View style={styles.historySection}>
        <View style={styles.historyHeader}>
          <Text style={styles.historyTitle}>Історія операцій</Text>
          <Text style={styles.historyCount}>0 операцій</Text>
        </View>

        <View style={styles.emptyHistoryBox}>
          <Ionicons name="receipt-outline" size={32} color={colors.textDim} />
          <Text style={styles.emptyHistoryTitle}>Історія операцій порожня</Text>
          <Text style={styles.emptyHistorySubtitle}>
            Тут відображатимуться ваші транзакції поповнення та нарахування бонусів.
          </Text>
        </View>
      </View>
    </View>
  );
};

const styles = StyleSheet.create({
  container: {
    padding: 16,
  },
  header: {
    marginBottom: 20,
  },
  title: {
    fontSize: 18,
    fontWeight: '800',
    color: colors.text,
    marginBottom: 4,
  },
  subtitle: {
    fontSize: 12,
    color: colors.textMuted,
  },
  walletCard: {
    backgroundColor: colors.surfaceCard,
    borderRadius: 20,
    padding: 16,
    borderWidth: 1,
    borderColor: colors.border,
    marginBottom: 24,
  },
  walletTop: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    marginBottom: 12,
  },
  chainRow: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 6,
    backgroundColor: 'rgba(34, 211, 238, 0.1)',
    paddingHorizontal: 8,
    paddingVertical: 4,
    borderRadius: 8,
    borderWidth: 1,
    borderColor: 'rgba(34, 211, 238, 0.3)',
  },
  chainDot: {
    width: 6,
    height: 6,
    borderRadius: 3,
    backgroundColor: colors.primary,
  },
  chainText: {
    fontSize: 10,
    fontWeight: '800',
    color: colors.primary,
    fontFamily: 'monospace',
  },
  copyBtn: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 4,
    backgroundColor: colors.background,
    paddingHorizontal: 8,
    paddingVertical: 4,
    borderRadius: 8,
    borderWidth: 1,
    borderColor: colors.border,
  },
  copyBtnText: {
    fontSize: 11,
    fontWeight: '700',
    color: colors.primary,
  },
  addressText: {
    fontSize: 12,
    color: colors.textDim,
    fontFamily: 'monospace',
    backgroundColor: colors.background,
    padding: 10,
    borderRadius: 10,
    marginBottom: 14,
    borderWidth: 1,
    borderColor: colors.border,
  },
  balancesContainer: {
    flexDirection: 'row',
    alignItems: 'center',
    backgroundColor: colors.background,
    borderRadius: 12,
    padding: 12,
    marginBottom: 16,
    borderWidth: 1,
    borderColor: colors.border,
  },
  balanceBlock: {
    flex: 1,
  },
  balanceLabel: {
    fontSize: 10,
    color: colors.textDim,
    marginBottom: 4,
  },
  balanceDtp: {
    fontSize: 16,
    fontWeight: '900',
    color: colors.primary,
    fontFamily: 'monospace',
  },
  balanceUah: {
    fontSize: 16,
    fontWeight: '900',
    color: colors.accentEmerald,
    fontFamily: 'monospace',
  },
  balanceDivider: {
    width: 1,
    height: 32,
    backgroundColor: colors.border,
    marginHorizontal: 12,
  },
  depositBtn: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'center',
    gap: 8,
    backgroundColor: colors.primary,
    borderRadius: 14,
    paddingVertical: 12,
  },
  depositBtnText: {
    color: '#021118',
    fontWeight: '800',
    fontSize: 13,
  },
  historySection: {},
  historyHeader: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    marginBottom: 12,
  },
  historyTitle: {
    fontSize: 16,
    fontWeight: '800',
    color: colors.text,
  },
  historyCount: {
    fontSize: 12,
    color: colors.textDim,
  },
  transactionsList: {
    backgroundColor: colors.surfaceCard,
    borderRadius: 16,
    borderWidth: 1,
    borderColor: colors.border,
    overflow: 'hidden',
  },
  txRow: {
    flexDirection: 'row',
    alignItems: 'center',
    padding: 12,
    borderBottomWidth: 1,
    borderBottomColor: colors.border,
    gap: 12,
  },
  txIconBox: {
    width: 32,
    height: 32,
    borderRadius: 16,
    backgroundColor: colors.background,
    alignItems: 'center',
    justifyContent: 'center',
  },
  txInfo: {
    flex: 1,
  },
  txTitle: {
    fontSize: 12,
    fontWeight: '700',
    color: colors.text,
  },
  txDate: {
    fontSize: 10,
    color: colors.textDim,
    marginTop: 2,
    fontFamily: 'monospace',
  },
  emptyHistoryBox: {
    backgroundColor: colors.surfaceCard,
    borderRadius: 16,
    padding: 24,
    alignItems: 'center',
    borderWidth: 1,
    borderColor: colors.border,
    gap: 8,
  },
  emptyHistoryTitle: {
    fontSize: 14,
    fontWeight: '800',
    color: colors.text,
    marginTop: 4,
  },
  emptyHistorySubtitle: {
    fontSize: 11,
    color: colors.textDim,
    textAlign: 'center',
    lineHeight: 16,
    maxWidth: 260,
  },
});
