import React from 'react';
import { View, Text, StyleSheet, Image, TouchableOpacity } from 'react-native';
import { colors } from '../../theme/colors';
import { ChatMessage } from '../../types';
import { AudioPlayer } from './AudioPlayer';
import { Ionicons } from '@expo/vector-icons';

interface MessageBubbleProps {
  message: ChatMessage;
}

export const MessageBubble: React.FC<MessageBubbleProps> = ({ message }) => {
  const isMine = message.isMine;

  return (
    <View style={[styles.wrapper, isMine ? styles.wrapperMine : styles.wrapperOther]}>
      <View style={[styles.bubble, isMine ? styles.bubbleMine : styles.bubbleOther]}>
        {message.type === 'voice' ? (
          <AudioPlayer
            duration={message.voiceDuration || 14}
            voiceUri={message.voiceUri}
            isMine={isMine}
          />
        ) : message.type === 'image' && message.imageUrl ? (
          <View style={styles.imageContainer}>
            <Image source={{ uri: message.imageUrl }} style={styles.attachedImage} resizeMode="cover" />
            {message.text ? (
              <Text style={[styles.text, isMine ? styles.textMine : styles.textOther, { marginTop: 6 }]}>
                {message.text}
              </Text>
            ) : null}
          </View>
        ) : message.type === 'file' ? (
          <View style={styles.fileContainer}>
            <View style={[styles.fileIconBox, isMine ? styles.fileIconBoxMine : styles.fileIconBoxOther]}>
              <Ionicons
                name="document-text-outline"
                size={24}
                color={isMine ? '#03212c' : colors.primary}
              />
            </View>
            <View style={styles.fileInfo}>
              <Text style={[styles.fileName, isMine ? styles.fileNameMine : styles.fileNameOther]} numberOfLines={1}>
                {message.fileName || 'Документ.pdf'}
              </Text>
              <Text style={[styles.fileSize, isMine ? styles.fileSizeMine : styles.fileSizeOther]}>
                {message.fileSize || '1.8 MB'}
              </Text>
            </View>
          </View>
        ) : (
          <Text style={[styles.text, isMine ? styles.textMine : styles.textOther]}>
            {message.text}
          </Text>
        )}

        <View style={styles.footerRow}>
          <Text style={[styles.timestamp, isMine ? styles.timestampMine : styles.timestampOther]}>
            {message.timestamp}
          </Text>
          {isMine && (
            <Text style={[styles.statusTicks, { color: isMine ? '#03212c' : colors.primary }]}>
              ✓✓
            </Text>
          )}
        </View>
      </View>
    </View>
  );
};

const styles = StyleSheet.create({
  wrapper: {
    marginVertical: 4,
    paddingHorizontal: 12,
    flexDirection: 'row',
  },
  wrapperMine: {
    justifyContent: 'flex-end',
  },
  wrapperOther: {
    justifyContent: 'flex-start',
  },
  bubble: {
    maxWidth: '84%',
    borderRadius: 18,
    paddingHorizontal: 14,
    paddingVertical: 10,
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 2 },
    shadowOpacity: 0.2,
    shadowRadius: 4,
    elevation: 2,
  },
  bubbleMine: {
    backgroundColor: colors.primary,
    borderBottomRightRadius: 4,
  },
  bubbleOther: {
    backgroundColor: colors.surfaceCardLight,
    borderBottomLeftRadius: 4,
    borderWidth: 1,
    borderColor: colors.border,
  },
  text: {
    fontSize: 14,
    lineHeight: 20,
  },
  textMine: {
    color: '#021118',
    fontWeight: '600',
  },
  textOther: {
    color: colors.text,
    fontWeight: '400',
  },
  imageContainer: {
    borderRadius: 12,
    overflow: 'hidden',
  },
  attachedImage: {
    width: 230,
    height: 160,
    borderRadius: 12,
    backgroundColor: colors.surfaceCard,
  },
  fileContainer: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 10,
    minWidth: 180,
    maxWidth: 240,
    paddingVertical: 4,
  },
  fileIconBox: {
    width: 42,
    height: 42,
    borderRadius: 10,
    alignItems: 'center',
    justifyContent: 'center',
  },
  fileIconBoxMine: {
    backgroundColor: 'rgba(3, 33, 44, 0.15)',
  },
  fileIconBoxOther: {
    backgroundColor: 'rgba(34, 211, 238, 0.15)',
    borderWidth: 1,
    borderColor: colors.primary,
  },
  fileInfo: {
    flex: 1,
  },
  fileName: {
    fontSize: 13,
    fontWeight: '700',
  },
  fileNameMine: {
    color: '#021118',
  },
  fileNameOther: {
    color: colors.text,
  },
  fileSize: {
    fontSize: 10,
    marginTop: 2,
    fontFamily: 'monospace',
  },
  fileSizeMine: {
    color: 'rgba(2, 17, 24, 0.7)',
  },
  fileSizeOther: {
    color: colors.textDim,
  },
  footerRow: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'flex-end',
    gap: 4,
    marginTop: 4,
  },
  timestamp: {
    fontSize: 10,
    fontFamily: 'monospace',
  },
  timestampMine: {
    color: 'rgba(2, 17, 24, 0.7)',
  },
  timestampOther: {
    color: colors.textDim,
  },
  statusTicks: {
    fontSize: 10,
    fontWeight: '900',
  },
});
