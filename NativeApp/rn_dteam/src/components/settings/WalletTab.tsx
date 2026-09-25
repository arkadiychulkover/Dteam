import React, { useState } from 'react';
import { View, Text, StyleSheet, TouchableOpacity, Alert } from 'react-native';
import { colors } from '../../theme/colors';
import { UserProfile, Transaction } from '../../types';
import { Ionicons } from '@expo/vector-icons';

interface WalletTabProps {
  profile: UserProfile;
  transactions: Transaction[];
}

export const WalletTab: React.FC<WalletTabProps> = ({ profile, transactions }) => {
  const [copied, setCopied] = useState(false);

  const handleCopy = () => {
    setCopied(true);
    setTimeout(() => setCopied(false), 2000);
    Alert.alert('Скопійовано', `Адресу гаманця збережено:\n${profile.walletAddress}`);
  };

  const handleDeposit = () => {
    Alert.alert(
      'Поповнення TON',
      'Для поповнення надішліть TON на вашу персональну адресу в мережі The Open Network або скористайтеся Tonkeeper.',
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
        </View>

        <Text style={styles.addressText} numberOfLines={1}>
          {profile.walletAddress}
        </Text>

        <View style={styles.balancesContainer}>
          <View style={styles.balanceBlock}>
            <Text style={styles.balanceLabel}>Бали DTEAM (DTP)</Text>
            <Text style={styles.balanceDtp}>{profile.tokensBalance} DTP</Text>
          </View>

          <View style={styles.balanceDivider} />

          <View style={styles.balanceBlock}>
            <Text style={styles.balanceLabel}>Гривневий баланс</Text>
            <Text style={styles.balanceUah}>₴ {profile.balanceUah.toFixed(2)}</Text>
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
          <Text style={styles.historyCount}>{transactions.length} транзакцій</Text>
        </View>

        <View style={styles.transactionsList}>
          {transactions.map((tx) => (
            <View key={tx.id} style={styles.txRow}>
              <View style={styles.txIconBox}>
                <Ionicons
                  name={tx.isPositive ? 'arrow-down-outline' : 'arrow-up-outline'}
                  size={16}
                  color={tx.isPositive ? colors.accentEmerald : colors.accentRose}
                />
              </View>

              <View style={styles.txInfo}>
                <Text style={styles.txTitle}>{tx.title}</Text>
                <Text style={styles.txDate}>{tx.date}</Text>
              </View>

              <Text
                style={[
                  styles.txAmount,
                  { color: tx.isPositive ? colors.accentEmerald : colors.text },
                ]}
              >
                {tx.amount}
              </Text>
            </View>
          ))}
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
  txAmount: {
    fontSize: 12,
    fontWeight: '800',
    fontFamily: 'monospace',
  },
});
