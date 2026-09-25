import React, { useRef, useEffect } from 'react';
import {
  View,
  Text,
  StyleSheet,
  TouchableOpacity,
  FlatList,
  Image,
  KeyboardAvoidingView,
  Platform,
} from 'react-native';
import { colors } from '../theme/colors';
import { Dialog, ChatMessage } from '../types';
import { MessageBubble } from '../components/chat/MessageBubble';
import { ChatInputBar } from '../components/chat/ChatInputBar';
import { Ionicons } from '@expo/vector-icons';

interface ChatDetailScreenProps {
  dialog: Dialog;
  onBack: () => void;
  onSendMessage: (dialogId: string, text: string) => void;
  onSendVoice: (dialogId: string, voiceUri?: string, duration?: number) => void;
  onSendImage: (dialogId: string, imageUri: string, caption?: string) => void;
  onSendFile: (dialogId: string, fileName: string, fileSize: string, fileUri?: string) => void;
}

export const ChatDetailScreen: React.FC<ChatDetailScreenProps> = ({
  dialog,
  onBack,
  onSendMessage,
  onSendVoice,
  onSendImage,
  onSendFile,
}) => {
  const flatListRef = useRef<FlatList>(null);

  useEffect(() => {
    // Scroll to end when messages change
    setTimeout(() => {
      flatListRef.current?.scrollToEnd({ animated: true });
    }, 100);
  }, [dialog.messages.length]);

  const handleSend = (text: string) => {
    onSendMessage(dialog.id, text);
  };

  const handleVoice = (voiceUri?: string, duration?: number) => {
    onSendVoice(dialog.id, voiceUri, duration);
  };

  const handleImage = (imageUri: string, caption?: string) => {
    onSendImage(dialog.id, imageUri, caption);
  };

  const handleFile = (fileName: string, fileSize: string, fileUri?: string) => {
    onSendFile(dialog.id, fileName, fileSize, fileUri);
  };

  const getStatusColor = () => {
    if (dialog.friendStatus === 1) return colors.accentEmerald;
    if (dialog.friendStatus === 2) return colors.accentPurple;
    return colors.offline;
  };

  return (
    <KeyboardAvoidingView
      style={styles.container}
      behavior={Platform.OS === 'ios' ? 'padding' : undefined}
      keyboardVerticalOffset={Platform.OS === 'ios' ? 90 : 0}
    >
      {/* Top Chat Header */}
      <View style={styles.header}>
        <TouchableOpacity style={styles.backButton} onPress={onBack} activeOpacity={0.7}>
          <Ionicons name="arrow-back" size={20} color={colors.primary} />
        </TouchableOpacity>

        <View style={styles.avatarWrapper}>
          <Image source={{ uri: dialog.friendAvatarUrl }} style={styles.avatar} />
          <View style={[styles.statusDot, { backgroundColor: getStatusColor() }]} />
        </View>

        <View style={styles.headerInfo}>
          <Text style={styles.username} numberOfLines={1}>
            {dialog.friendUsername}
          </Text>
          <Text style={[styles.statusText, { color: getStatusColor() }]} numberOfLines={1}>
            {dialog.gameStatus || (dialog.friendStatus === 1 ? 'У мережі' : 'Офлайн')}
          </Text>
        </View>

        <TouchableOpacity style={styles.infoButton} activeOpacity={0.7}>
          <Ionicons name="ellipsis-vertical" size={18} color={colors.textMuted} />
        </TouchableOpacity>
      </View>

      {/* Messages List */}
      <FlatList
        ref={flatListRef}
        data={dialog.messages}
        keyExtractor={(item) => item.id}
        renderItem={({ item }) => <MessageBubble message={item} />}
        contentContainerStyle={styles.messagesContent}
        style={styles.messagesList}
        showsVerticalScrollIndicator={false}
      />

      {/* Input Bar */}
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
    height: 58,
    flexDirection: 'row',
    alignItems: 'center',
    paddingHorizontal: 12,
    backgroundColor: colors.surface,
    borderBottomWidth: 1,
    borderBottomColor: colors.border,
    gap: 10,
  },
  backButton: {
    width: 36,
    height: 36,
    borderRadius: 18,
    backgroundColor: colors.surfaceCard,
    alignItems: 'center',
    justifyContent: 'center',
    borderWidth: 1,
    borderColor: colors.border,
  },
  avatarWrapper: {
    position: 'relative',
  },
  avatar: {
    width: 38,
    height: 38,
    borderRadius: 19,
    borderWidth: 1.5,
    borderColor: colors.borderLight,
  },
  statusDot: {
    position: 'absolute',
    bottom: -1,
    right: -1,
    width: 10,
    height: 10,
    borderRadius: 5,
    borderWidth: 1.5,
    borderColor: colors.surface,
  },
  headerInfo: {
    flex: 1,
  },
  username: {
    fontSize: 14,
    fontWeight: '800',
    color: colors.text,
  },
  statusText: {
    fontSize: 10,
    fontWeight: '600',
    fontFamily: 'monospace',
  },
  infoButton: {
    padding: 6,
  },
  messagesList: {
    flex: 1,
  },
  messagesContent: {
    paddingVertical: 14,
  },
});
