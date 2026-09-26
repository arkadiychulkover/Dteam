import React from 'react';
import { View, Text, StyleSheet, TouchableOpacity } from 'react-native';
import { colors } from '../../theme/colors';
import { Dialog } from '../../types';
import { BackendImage } from '../BackendImage';
import { Ionicons } from '@expo/vector-icons';
import { usePresenceStore } from '../../store/usePresenceStore';

interface DialogItemProps {
  dialog: Dialog;
  onPress: () => void;
}

export const DialogItem: React.FC<DialogItemProps> = ({ dialog, onPress }) => {
  const isOnline = usePresenceStore((s) => s.isOnline(dialog.friendId));

  const getStatusColor = () => {
    if (isOnline || dialog.friendStatus === 1) return colors.accentEmerald;
    if (dialog.friendStatus === 2) return colors.accentPurple;
    return colors.offline;
  };

  const lastMsg = dialog.messages && dialog.messages.length > 0
    ? dialog.messages[dialog.messages.length - 1]
    : null;

  const isVoice = lastMsg?.type === 'voice';
  const isImage = lastMsg?.type === 'image';
  const isFile = lastMsg?.type === 'file';

  return (
    <TouchableOpacity style={styles.container} onPress={onPress} activeOpacity={0.7}>
      <View style={styles.avatarWrapper}>
        <BackendImage
          src={dialog.friendAvatarUrl}
          style={styles.avatar}
          fallbackText={dialog.friendUsername.slice(0, 2).toUpperCase()}
        />
        <View style={[styles.statusDot, { backgroundColor: getStatusColor() }]} />
      </View>

      <View style={styles.content}>
        <View style={styles.topRow}>
          <Text style={styles.username} numberOfLines={1}>
            {dialog.friendUsername}
          </Text>
          <Text style={styles.timeText}>
            {dialog.lastMessageTime}
          </Text>
        </View>

        <View style={styles.bottomRow}>
          <View style={styles.lastMessageContainer}>
            {isVoice && (
              <Ionicons name="mic-outline" size={13} color={colors.primary} style={{ marginRight: 3 }} />
            )}
            {isImage && (
              <Ionicons name="image-outline" size={13} color={colors.primary} style={{ marginRight: 3 }} />
            )}
            {isFile && (
              <Ionicons name="document-text-outline" size={13} color={colors.primary} style={{ marginRight: 3 }} />
            )}
            <Text
              style={[
                styles.lastMessageText,
                dialog.unreadCount > 0 && styles.lastMessageUnread,
              ]}
              numberOfLines={1}
            >
              {dialog.lastMessage}
            </Text>
          </View>

          {dialog.unreadCount > 0 && (
            <View style={styles.unreadBadge}>
              <Text style={styles.unreadCountText}>
                {dialog.unreadCount > 99 ? '99+' : dialog.unreadCount}
              </Text>
            </View>
          )}
        </View>
      </View>
    </TouchableOpacity>
  );
};

const styles = StyleSheet.create({
  container: {
    flexDirection: 'row',
    alignItems: 'center',
    paddingVertical: 12,
    paddingHorizontal: 16,
    backgroundColor: colors.surfaceCard,
    borderRadius: 16,
    marginBottom: 8,
    borderWidth: 1,
    borderColor: colors.border,
    gap: 12,
  },
  avatarWrapper: {
    position: 'relative',
  },
  avatar: {
    width: 48,
    height: 48,
    borderRadius: 24,
    borderWidth: 1.5,
    borderColor: colors.borderLight,
    backgroundColor: colors.surface,
  },
  statusDot: {
    position: 'absolute',
    bottom: 0,
    right: 0,
    width: 12,
    height: 12,
    borderRadius: 6,
    borderWidth: 2,
    borderColor: colors.surfaceCard,
  },
  content: {
    flex: 1,
    justifyContent: 'center',
    gap: 4,
  },
  topRow: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
  },
  username: {
    fontSize: 15,
    fontWeight: '700',
    color: colors.text,
    flex: 1,
    marginRight: 8,
  },
  timeText: {
    fontSize: 11,
    color: colors.textDim,
    fontFamily: 'monospace',
  },
  bottomRow: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
  },
  lastMessageContainer: {
    flexDirection: 'row',
    alignItems: 'center',
    flex: 1,
    marginRight: 8,
  },
  lastMessageText: {
    fontSize: 13,
    color: colors.textMuted,
  },
  lastMessageUnread: {
    color: colors.text,
    fontWeight: '600',
  },
  unreadBadge: {
    backgroundColor: colors.primary,
    minWidth: 20,
    height: 20,
    borderRadius: 10,
    alignItems: 'center',
    justifyContent: 'center',
    paddingHorizontal: 6,
  },
  unreadCountText: {
    color: '#021118',
    fontSize: 11,
    fontWeight: '900',
  },
});
