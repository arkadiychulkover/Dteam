import React, { useRef, useEffect, useState } from 'react';
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
  dialog?: Dialog;
  route?: { params?: { dialog: Dialog } };
  navigation?: any;
  onBack?: () => void;
  onSendMessage?: (dialogId: string, text: string) => void;
  onSendVoice?: (dialogId: string, voiceUri?: string, duration?: number) => void;
  onSendImage?: (dialogId: string, imageUri: string, caption?: string) => void;
  onSendFile?: (dialogId: string, fileName: string, fileSize: string, fileUri?: string) => void;
}

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
  const currentDialog = propDialog || route?.params?.dialog;
  const flatListRef = useRef<FlatList>(null);
  const [messages, setMessages] = useState<ChatMessage[]>(currentDialog?.messages || []);

  useEffect(() => {
    if (currentDialog?.messages) {
      setMessages(currentDialog.messages);
    }
  }, [currentDialog]);

  useEffect(() => {
    setTimeout(() => {
      flatListRef.current?.scrollToEnd({ animated: true });
    }, 100);
  }, [messages.length]);

  if (!currentDialog) {
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

  const handleSend = (text: string) => {
    const newMsg: ChatMessage = {
      id: `m-${Date.now()}`,
      dialogId: currentDialog.id,
      senderId: 'usr-current',
      text,
      type: 'text',
      timestamp: getCurrentTimeFormatted(),
      isMine: true,
      status: 'sent',
    };
    setMessages((prev) => [...prev, newMsg]);
    if (onSendMessage) {
      onSendMessage(currentDialog.id, text);
    }
  };

  const handleVoice = (voiceUri?: string, duration?: number) => {
    const newMsg: ChatMessage = {
      id: `m-voice-${Date.now()}`,
      dialogId: currentDialog.id,
      senderId: 'usr-current',
      text: 'Голосове повідомлення',
      type: 'voice',
      voiceDuration: duration || 10,
      voiceUri,
      timestamp: getCurrentTimeFormatted(),
      isMine: true,
      status: 'sent',
    };
    setMessages((prev) => [...prev, newMsg]);
    if (onSendVoice) {
      onSendVoice(currentDialog.id, voiceUri, duration);
    }
  };

  const handleImage = (imageUri: string, caption?: string) => {
    const newMsg: ChatMessage = {
      id: `m-img-${Date.now()}`,
      dialogId: currentDialog.id,
      senderId: 'usr-current',
      text: caption || '',
      type: 'image',
      imageUrl: imageUri,
      timestamp: getCurrentTimeFormatted(),
      isMine: true,
      status: 'sent',
    };
    setMessages((prev) => [...prev, newMsg]);
    if (onSendImage) {
      onSendImage(currentDialog.id, imageUri, caption);
    }
  };

  const handleFile = (fileName: string, fileSize: string, fileUri?: string) => {
    const newMsg: ChatMessage = {
      id: `m-file-${Date.now()}`,
      dialogId: currentDialog.id,
      senderId: 'usr-current',
      text: fileName,
      type: 'file',
      fileName,
      fileSize,
      timestamp: getCurrentTimeFormatted(),
      isMine: true,
      status: 'sent',
    };
    setMessages((prev) => [...prev, newMsg]);
    if (onSendFile) {
      onSendFile(currentDialog.id, fileName, fileSize, fileUri);
    }
  };

  const getStatusColor = () => {
    if (currentDialog.friendStatus === 1) return colors.accentEmerald;
    if (currentDialog.friendStatus === 2) return colors.accentPurple;
    return colors.offline;
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
          <Image source={{ uri: currentDialog.friendAvatarUrl }} style={styles.avatar} />
          <View style={[styles.statusDot, { backgroundColor: getStatusColor() }]} />
        </View>

        <View style={styles.headerInfo}>
          <Text style={styles.username} numberOfLines={1}>
            {currentDialog.friendUsername}
          </Text>
          <Text style={[styles.statusText, { color: getStatusColor() }]} numberOfLines={1}>
            {currentDialog.gameStatus || (currentDialog.friendStatus === 1 ? 'У мережі' : 'Офлайн')}
          </Text>
        </View>

        <TouchableOpacity style={styles.infoButton} activeOpacity={0.7}>
          <Ionicons name="ellipsis-vertical" size={18} color={colors.textMuted} />
        </TouchableOpacity>
      </View>

      <FlatList
        ref={flatListRef}
        data={messages}
        keyExtractor={(item) => item.id}
        renderItem={({ item }) => <MessageBubble message={item} />}
        contentContainerStyle={styles.messagesContent}
        style={styles.messagesList}
        showsVerticalScrollIndicator={false}
      />

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
