import React, { useRef, useEffect, useState, useCallback } from 'react';
import {
  View,
  Text,
  StyleSheet,
  TouchableOpacity,
  FlatList,
  KeyboardAvoidingView,
  Platform,
  ActivityIndicator,
} from 'react-native';
import { colors } from '../theme/colors';
import { Dialog, ChatMessage } from '../types';
import { MessageBubble } from '../components/chat/MessageBubble';
import { ChatInputBar } from '../components/chat/ChatInputBar';
import { BackendImage } from '../components/BackendImage';
import { useAuthStore } from '../store/useAuthStore';
import { useChatStore } from '../store/useChatStore';
import { usePresenceStore } from '../store/usePresenceStore';
import { chatService } from '../services/chatService';
import { Ionicons } from '@expo/vector-icons';

interface ChatDetailScreenProps {
  dialog?: Dialog;
  route?: { params?: { dialog: Dialog } };
  navigation?: any;
  onBack?: () => void;
  onSendMessage?: (dialogId: string, text: string) => void;
  onSendVoice?: (dialogId: string, voiceUri?: string, duration?: number) => void;
  onSendImage?: (dialogId: string, imageUri: string, caption?: string) => void;
  onSendFile?: (dialogId: string, fileName: string, fileSize: string, fileUri?: string) => void;
}

const EMPTY_MESSAGES: ChatMessage[] = [];

export const ChatDetailScreen: React.FC<ChatDetailScreenProps> = ({
  dialog: propDialog,
  route,
  navigation,
  onBack,
  onSendMessage,
  onSendVoice,
  onSendImage,
  onSendFile,
}) => {
  const user = useAuthStore((s) => s.user);
  const currentDialog = propDialog || route?.params?.dialog;
  const flatListRef = useRef<FlatList>(null);
  const [isSending, setIsSending] = useState(false);

  const friendId = currentDialog?.friendId || currentDialog?.id;
  const peerKey = (friendId || '').toLowerCase();

  const messages = useChatStore((s) => s.messages[peerKey] ?? EMPTY_MESSAGES);
  const isLoadingHistory = useChatStore((s) => !!s.isLoadingHistory[peerKey]);
  const isTyping = useChatStore((s) => !!s.typingUsers[peerKey]);
  const isOnline = usePresenceStore((s) => !!(friendId && s.onlineUserIds.has(friendId.toLowerCase())));

  const loadHistory = useChatStore((s) => s.loadHistory);
  const sendMessage = useChatStore((s) => s.sendMessage);
  const sendVoiceMessage = useChatStore((s) => s.sendVoiceMessage);
  const sendImageMessage = useChatStore((s) => s.sendImageMessage);
  const sendFileMessage = useChatStore((s) => s.sendFileMessage);
  const sendOptimisticMessage = useChatStore((s) => s.sendOptimisticMessage);
  const setActivePeerId = useChatStore((s) => s.setActivePeerId);

  useEffect(() => {
    if (friendId) {
      setActivePeerId(friendId);
      loadHistory(friendId);
    }
    return () => {
      setActivePeerId(null);
    };
  }, [friendId, setActivePeerId, loadHistory]);

  useEffect(() => {
    if (messages.length > 0) {
      setTimeout(() => {
        flatListRef.current?.scrollToEnd({ animated: true });
      }, 100);
    }
  }, [messages.length]);

  if (!currentDialog || !friendId) {
    return (
      <View style={styles.container}>
        <View style={styles.header}>
          <TouchableOpacity
            style={styles.backButton}
            onPress={() => (onBack ? onBack() : navigation?.goBack?.())}
            activeOpacity={0.7}
          >
            <Ionicons name="arrow-back" size={20} color={colors.primary} />
          </TouchableOpacity>
          <Text style={styles.username}>Діалог не знайдено</Text>
        </View>
      </View>
    );
  }

  const handleGoBack = () => {
    if (onBack) {
      onBack();
    } else if (navigation?.goBack) {
      navigation.goBack();
    }
  };

  const getCurrentTimeFormatted = () => {
    const now = new Date();
    const hours = now.getHours().toString().padStart(2, '0');
    const minutes = now.getMinutes().toString().padStart(2, '0');
    return `${hours}:${minutes}`;
  };

  const handleSend = async (text: string) => {
    if (!text.trim() || isSending || !friendId) return;

    const tempId = `temp-${Date.now()}`;
    const optimisticMsg: ChatMessage = {
      id: tempId,
      dialogId: friendId,
      senderId: user?.id || 'usr-current',
      text: text.trim(),
      type: 'text',
      timestamp: getCurrentTimeFormatted(),
      isMine: true,
      status: 'sent',
    };

    sendOptimisticMessage(optimisticMsg);
    setIsSending(true);

    try {
      await sendMessage(friendId, text);
      if (onSendMessage) {
        onSendMessage(friendId, text);
      }
    } catch (err) {
      console.warn('[ChatDetailScreen] Error sending message to server:', err);
    } finally {
      setIsSending(false);
    }
  };

  const handleVoice = async (voiceUri?: string, duration?: number) => {
    if (!friendId || !voiceUri) return;

    const tempId = `temp-voice-${Date.now()}`;
    const optimisticMsg: ChatMessage = {
      id: tempId,
      dialogId: friendId,
      senderId: user?.id || 'usr-current',
      text: 'Голосове повідомлення',
      type: 'voice',
      voiceDuration: duration || 10,
      voiceUri,
      timestamp: getCurrentTimeFormatted(),
      isMine: true,
      status: 'sent',
    };
    sendOptimisticMessage(optimisticMsg);

    try {
      await sendVoiceMessage(friendId, voiceUri, duration);
      if (onSendVoice) {
        onSendVoice(friendId, voiceUri, duration);
      }
    } catch (err) {
      console.warn('[ChatDetailScreen] Error sending voice message:', err);
    }
  };

  const handleImage = async (imageUri: string, caption?: string) => {
    if (!friendId || !imageUri) return;

    const tempId = `temp-img-${Date.now()}`;
    const optimisticMsg: ChatMessage = {
      id: tempId,
      dialogId: friendId,
      senderId: user?.id || 'usr-current',
      text: caption || '',
      type: 'image',
      imageUrl: imageUri,
      timestamp: getCurrentTimeFormatted(),
      isMine: true,
      status: 'sent',
    };
    sendOptimisticMessage(optimisticMsg);

    try {
      await sendImageMessage(friendId, imageUri, caption);
      if (onSendImage) {
        onSendImage(friendId, imageUri, caption);
      }
    } catch (err) {
      console.warn('[ChatDetailScreen] Error sending image:', err);
    }
  };

  const handleFile = async (fileName: string, fileSize: string, fileUri?: string) => {
    if (!friendId || !fileUri) return;

    const tempId = `temp-file-${Date.now()}`;
    const optimisticMsg: ChatMessage = {
      id: tempId,
      dialogId: friendId,
      senderId: user?.id || 'usr-current',
      text: fileName,
      type: 'file',
      fileName,
      fileSize,
      timestamp: getCurrentTimeFormatted(),
      isMine: true,
      status: 'sent',
    };
    sendOptimisticMessage(optimisticMsg);

    try {
      await sendFileMessage(friendId, fileUri, fileName);
      if (onSendFile) {
        onSendFile(friendId, fileName, fileSize, fileUri);
      }
    } catch (err) {
      console.warn('[ChatDetailScreen] Error sending file:', err);
    }
  };

  const isFriendOnline = isOnline || currentDialog.friendStatus === 1;

  const getStatusColor = () => {
    if (isFriendOnline) return colors.accentEmerald;
    if (currentDialog.friendStatus === 2) return colors.accentPurple;
    return colors.offline;
  };

  const getStatusLabel = () => {
    if (isTyping) return 'Друкує...';
    if (isFriendOnline) return 'У мережі';
    if (currentDialog.friendStatus === 2) return currentDialog.gameStatus || 'У грі';
    return 'Не в мережі';
  };

  return (
    <KeyboardAvoidingView
      style={styles.container}
      behavior={Platform.OS === 'ios' ? 'padding' : undefined}
      keyboardVerticalOffset={Platform.OS === 'ios' ? 90 : 0}
    >
      <View style={styles.header}>
        <TouchableOpacity style={styles.backButton} onPress={handleGoBack} activeOpacity={0.7}>
          <Ionicons name="arrow-back" size={20} color={colors.primary} />
        </TouchableOpacity>

        <View style={styles.avatarWrapper}>
          <BackendImage
            src={currentDialog.friendAvatarUrl}
            style={styles.avatar}
            fallbackText={currentDialog.friendUsername.slice(0, 2).toUpperCase()}
          />
          <View style={[styles.statusDot, { backgroundColor: getStatusColor() }]} />
        </View>

        <View style={styles.headerInfo}>
          <Text style={styles.username} numberOfLines={1}>
            {currentDialog.friendUsername}
          </Text>
          <Text style={[styles.statusText, { color: getStatusColor() }]}>
            {getStatusLabel()}
          </Text>
        </View>

        <TouchableOpacity style={styles.callButton} activeOpacity={0.7}>
          <Ionicons name="ellipsis-vertical" size={18} color={colors.textMuted} />
        </TouchableOpacity>
      </View>

      {isLoadingHistory ? (
        <View style={styles.centerLoading}>
          <ActivityIndicator size="large" color={colors.primary} />
          <Text style={styles.loadingText}>Завантаження історії повідомлень...</Text>
        </View>
      ) : (
        <FlatList
          ref={flatListRef}
          data={messages}
          keyExtractor={(item, index) => `${item.id}-${index}`}
          renderItem={({ item }) => <MessageBubble message={item} />}
          contentContainerStyle={styles.messagesList}
          showsVerticalScrollIndicator={false}
          ListEmptyComponent={
            <View style={styles.emptyContainer}>
              <View style={styles.emptyIconCircle}>
                <Ionicons name="chatbubble-ellipses-outline" size={32} color={colors.primary} />
              </View>
              <Text style={styles.emptyTitle}>Початок листування</Text>
              <Text style={styles.emptySubtitle}>
                Надішліть перше повідомлення користувачеві {currentDialog.friendUsername}!
              </Text>
            </View>
          }
        />
      )}

      <ChatInputBar
        onSendMessage={handleSend}
        onSendVoice={handleVoice}
        onSendImage={handleImage}
        onSendFile={handleFile}
      />
    </KeyboardAvoidingView>
  );
};

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: colors.background,
  },
  header: {
    flexDirection: 'row',
    alignItems: 'center',
    paddingHorizontal: 16,
    paddingTop: Platform.OS === 'ios' ? 12 : 16,
    paddingBottom: 12,
    backgroundColor: colors.surfaceCard,
    borderBottomWidth: 1,
    borderBottomColor: colors.border,
    gap: 12,
  },
  backButton: {
    padding: 6,
    borderRadius: 8,
  },
  avatarWrapper: {
    position: 'relative',
  },
  avatar: {
    width: 40,
    height: 40,
    borderRadius: 20,
    borderWidth: 1.5,
    borderColor: colors.borderLight,
  },
  statusDot: {
    position: 'absolute',
    bottom: 0,
    right: 0,
    width: 10,
    height: 10,
    borderRadius: 5,
    borderWidth: 2,
    borderColor: colors.surfaceCard,
  },
  headerInfo: {
    flex: 1,
  },
  username: {
    fontSize: 16,
    fontWeight: '800',
    color: colors.text,
  },
  statusText: {
    fontSize: 11,
    fontWeight: '600',
    marginTop: 1,
  },
  callButton: {
    padding: 8,
  },
  messagesList: {
    paddingVertical: 16,
    flexGrow: 1,
  },
  centerLoading: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
  },
  loadingText: {
    color: colors.textMuted,
    fontSize: 12,
    marginTop: 8,
  },
  emptyContainer: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
    padding: 32,
    marginTop: 60,
  },
  emptyIconCircle: {
    width: 60,
    height: 60,
    borderRadius: 30,
    backgroundColor: 'rgba(34, 211, 238, 0.1)',
    justifyContent: 'center',
    alignItems: 'center',
    marginBottom: 12,
  },
  emptyTitle: {
    fontSize: 15,
    fontWeight: '800',
    color: colors.text,
    marginBottom: 4,
  },
  emptySubtitle: {
    fontSize: 12,
    color: colors.textMuted,
    textAlign: 'center',
    lineHeight: 18,
    maxWidth: 240,
  },
});
