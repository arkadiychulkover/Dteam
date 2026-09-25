import React, { useState } from 'react';
import {
  View,
  Text,
  StyleSheet,
  Modal,
  TouchableOpacity,
  Image,
  Alert,
} from 'react-native';
import { colors } from '../../theme/colors';
import { Badge } from '../../types';
import { getRarityConfig } from './BadgeCard';
import { Ionicons } from '@expo/vector-icons';

interface BadgeDetailModalProps {
  badge: Badge | null;
  isOpen: boolean;
  onClose: () => void;
}

export const BadgeDetailModal: React.FC<BadgeDetailModalProps> = ({
  badge,
  isOpen,
  onClose,
}) => {
  const [copied, setCopied] = useState(false);

  if (!badge) return null;

  const rarityCfg = getRarityConfig(badge.rarity);
  const contractAddr = badge.contractAddress || '0x9fE46736679d2D9a65F0992F2272dE9f3c7fa6e0';

  const handleCopyContract = () => {
    setCopied(true);
    setTimeout(() => setCopied(false), 2000);
    Alert.alert('Скопійовано', `Адресу смарт-контракту збережено:\n${contractAddr}`);
  };

  return (
    <Modal
      visible={isOpen}
      transparent
      animationType="fade"
      onRequestClose={onClose}
    >
      <View style={styles.overlay}>
        <View
          style={[
            styles.modalContent,
            { borderColor: rarityCfg.borderColor },
          ]}
        >
          <TouchableOpacity style={styles.closeBtn} onPress={onClose} activeOpacity={0.7}>
            <Ionicons name="close" size={20} color={colors.textMuted} />
          </TouchableOpacity>

          <View style={styles.headerRow}>
            <Text style={styles.tokenIdBadge}>#{badge.tokenId || '001'}</Text>
            <View style={[styles.rarityPill, { borderColor: rarityCfg.borderColor }]}>
              <Text style={[styles.rarityLabel, { color: rarityCfg.textColor }]}>
                {rarityCfg.label}
              </Text>
            </View>
          </View>

          <View style={styles.imageBox}>
            {badge.imageUrl ? (
              <Image source={{ uri: badge.imageUrl }} style={styles.badgeImage} resizeMode="cover" />
            ) : (
              <Ionicons name="sparkles" size={48} color={rarityCfg.textColor} />
            )}
          </View>

          <Text style={styles.title}>{badge.title}</Text>
          <Text style={styles.description}>{badge.description}</Text>

          <View style={styles.infoGrid}>
            <View style={styles.infoCol}>
              <Text style={styles.infoLabel}>Рівень значка</Text>
              <Text style={styles.infoValue}>Рівень {badge.level || 1}</Text>
            </View>
            <View style={styles.infoCol}>
              <Text style={styles.infoLabel}>Дата отримання</Text>
              <Text style={styles.infoValue}>{badge.earnedDate}</Text>
            </View>
          </View>

          <View style={styles.contractSection}>
            <View style={styles.contractHeader}>
              <Text style={styles.contractTitle}>Контракт (ERC-721 / DNFT)</Text>
              <Ionicons name="shield-checkmark" size={14} color={colors.accentEmerald} />
            </View>

            <TouchableOpacity
              style={styles.contractCopyBtn}
              onPress={handleCopyContract}
              activeOpacity={0.7}
            >
              <Text style={styles.contractAddressText} numberOfLines={1}>
                {contractAddr}
              </Text>
              <Ionicons
                name={copied ? 'checkmark-circle' : 'copy-outline'}
                size={16}
                color={copied ? colors.accentEmerald : colors.primary}
              />
            </TouchableOpacity>
          </View>
        </View>
      </View>
    </Modal>
  );
};

const styles = StyleSheet.create({
  overlay: {
    flex: 1,
    backgroundColor: 'rgba(0, 0, 0, 0.8)',
    justifyContent: 'center',
    alignItems: 'center',
    padding: 20,
  },
  modalContent: {
    width: '100%',
    maxWidth: 360,
    backgroundColor: colors.surfaceCard,
    borderRadius: 24,
    padding: 20,
    borderWidth: 2,
    alignItems: 'center',
  },
  closeBtn: {
    position: 'absolute',
    top: 14,
    right: 14,
    width: 32,
    height: 32,
    borderRadius: 16,
    backgroundColor: colors.background,
    alignItems: 'center',
    justifyContent: 'center',
    zIndex: 10,
  },
  headerRow: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    width: '100%',
    paddingRight: 40,
    marginBottom: 16,
  },
  tokenIdBadge: {
    fontSize: 14,
    fontWeight: '900',
    color: colors.text,
    backgroundColor: colors.background,
    paddingHorizontal: 10,
    paddingVertical: 4,
    borderRadius: 8,
    fontFamily: 'monospace',
  },
  rarityPill: {
    paddingHorizontal: 8,
    paddingVertical: 3,
    borderRadius: 8,
    borderWidth: 1,
  },
  rarityLabel: {
    fontSize: 10,
    fontWeight: '800',
    textTransform: 'uppercase',
  },
  imageBox: {
    width: 140,
    height: 140,
    borderRadius: 20,
    overflow: 'hidden',
    backgroundColor: colors.background,
    borderWidth: 1,
    borderColor: colors.borderLight,
    alignItems: 'center',
    justifyContent: 'center',
    marginBottom: 16,
  },
  badgeImage: {
    width: '100%',
    height: '100%',
  },
  title: {
    fontSize: 18,
    fontWeight: '800',
    color: colors.text,
    textAlign: 'center',
    marginBottom: 6,
  },
  description: {
    fontSize: 13,
    color: colors.textMuted,
    textAlign: 'center',
    lineHeight: 18,
    marginBottom: 16,
    paddingHorizontal: 8,
  },
  infoGrid: {
    flexDirection: 'row',
    width: '100%',
    backgroundColor: colors.background,
    borderRadius: 12,
    padding: 12,
    marginBottom: 14,
    borderWidth: 1,
    borderColor: colors.border,
  },
  infoCol: {
    flex: 1,
    alignItems: 'center',
  },
  infoLabel: {
    fontSize: 10,
    color: colors.textDim,
    marginBottom: 2,
  },
  infoValue: {
    fontSize: 12,
    fontWeight: '700',
    color: colors.text,
  },
  contractSection: {
    width: '100%',
    backgroundColor: colors.background,
    borderRadius: 12,
    padding: 12,
    borderWidth: 1,
    borderColor: colors.border,
  },
  contractHeader: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    marginBottom: 6,
  },
  contractTitle: {
    fontSize: 10,
    fontWeight: '700',
    color: colors.textDim,
    textTransform: 'uppercase',
    letterSpacing: 0.5,
  },
  contractCopyBtn: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    backgroundColor: colors.surfaceCard,
    paddingHorizontal: 10,
    paddingVertical: 8,
    borderRadius: 8,
  },
  contractAddressText: {
    fontSize: 11,
    color: colors.textMuted,
    fontFamily: 'monospace',
    flex: 1,
    marginRight: 8,
  },
});
