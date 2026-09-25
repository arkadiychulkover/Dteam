import React, { useState, useRef, useEffect } from 'react';
import {
  View,
  Text,
  TextInput,
  TouchableOpacity,
  StyleSheet,
  Modal,
  Platform,
  Alert,
} from 'react-native';
import { colors } from '../../theme/colors';
import { Ionicons } from '@expo/vector-icons';
import * as ImagePicker from 'expo-image-picker';
import * as DocumentPicker from 'expo-document-picker';
import { Audio } from 'expo-av';

interface ChatInputBarProps {
  onSendMessage: (text: string) => void;
  onSendVoice: (voiceUri?: string, duration?: number) => void;
  onSendImage: (imageUri: string, caption?: string) => void;
  onSendFile: (fileName: string, fileSize: string, fileUri?: string) => void;
}

export const ChatInputBar: React.FC<ChatInputBarProps> = ({
  onSendMessage,
  onSendVoice,
  onSendImage,
  onSendFile,
}) => {
  const [text, setText] = useState('');
  const [isAttachModalVisible, setIsAttachModalVisible] = useState(false);

  const [isRecording, setIsRecording] = useState(false);
  const [recordingDuration, setRecordingDuration] = useState(0);

  const recordingRef = useRef<Audio.Recording | null>(null);

  const mediaRecorderRef = useRef<any>(null);
  const audioChunksRef = useRef<Blob[]>([]);
  const streamRef = useRef<MediaStream | null>(null);

  const durationTimerRef = useRef<any>(null);

  useEffect(() => {
    return () => {
      if (durationTimerRef.current) clearInterval(durationTimerRef.current);
    };
  }, []);

  const handlePickImage = async () => {
    setIsAttachModalVisible(false);
    try {
      const permissionResult = await ImagePicker.requestMediaLibraryPermissionsAsync();
      if (!permissionResult.granted) {
        Alert.alert('Потрібен дозвіл', 'Надайте додатку доступ до фотогалереї у налаштуваннях.');
        return;
      }

      const result = await ImagePicker.launchImageLibraryAsync({
        mediaTypes: ['images'],
        allowsEditing: true,
        quality: 0.8,
      });

      if (!result.canceled && result.assets && result.assets.length > 0) {
        const asset = result.assets[0];
        onSendImage(asset.uri, text.trim() || undefined);
        setText('');
      }
    } catch (err) {
      console.warn('Image picker error:', err);
      Alert.alert('Помилка', 'Не вдалося вибрати зображення.');
    }
  };

  const handleTakePhoto = async () => {
    setIsAttachModalVisible(false);
    try {
      const permissionResult = await ImagePicker.requestCameraPermissionsAsync();
      if (!permissionResult.granted) {
        Alert.alert('Потрібен дозвіл', 'Надайте додатку доступ до камери.');
        return;
      }

      const result = await ImagePicker.launchCameraAsync({
        allowsEditing: true,
        quality: 0.8,
      });

      if (!result.canceled && result.assets && result.assets.length > 0) {
        const asset = result.assets[0];
        onSendImage(asset.uri, text.trim() || undefined);
        setText('');
      }
    } catch (err) {
      console.warn('Camera error:', err);
      Alert.alert('Помилка', 'Не вдалося відкрити камеру.');
    }
  };

  const handlePickDocument = async () => {
    setIsAttachModalVisible(false);
    try {
      const result = await DocumentPicker.getDocumentAsync({
        type: '*/*',
        copyToCacheDirectory: true,
      });

      if (!result.canceled && result.assets && result.assets.length > 0) {
        const doc = result.assets[0];
        const sizeMb = doc.size ? `${(doc.size / (1024 * 1024)).toFixed(1)} MB` : '1.2 MB';
        onSendFile(doc.name, sizeMb, doc.uri);
      }
    } catch (err) {
      console.warn('Document picker error:', err);
      Alert.alert('Помилка', 'Не вдалося вибрати файл.');
    }
  };

  const startRecording = async () => {
    try {
      setRecordingDuration(0);

      if (Platform.OS === 'web') {
        if (!navigator.mediaDevices || !navigator.mediaDevices.getUserMedia) {
          Alert.alert('Помилка', 'Запис звуку не підтримується у цьому браузері.');
          return;
        }

        const stream = await navigator.mediaDevices.getUserMedia({ audio: true });
        streamRef.current = stream;
        audioChunksRef.current = [];

        const mediaRecorder = new (window as any).MediaRecorder(stream);
        mediaRecorder.ondataavailable = (event: any) => {
          if (event.data.size > 0) {
            audioChunksRef.current.push(event.data);
          }
        };

        mediaRecorder.start(200);
        mediaRecorderRef.current = mediaRecorder;
      } else {
        const permission = await Audio.requestPermissionsAsync();
        if (!permission.granted) {
          Alert.alert('Потрібен дозвіл', 'Надайте дозвіл на використання мікрофону.');
          return;
        }

        await Audio.setAudioModeAsync({
          allowsRecordingIOS: true,
          playsInSilentModeIOS: true,
        });

        const recording = new Audio.Recording();
        await recording.prepareToRecordAsync(Audio.RecordingOptionsPresets.HIGH_QUALITY);
        await recording.startAsync();
        recordingRef.current = recording;
      }

      setIsRecording(true);

      durationTimerRef.current = setInterval(() => {
        setRecordingDuration((prev) => prev + 1);
      }, 1000);
    } catch (err) {
      console.warn('Failed to start recording:', err);
      Alert.alert('Помилка запису', 'Не вдалося активувати мікрофон.');
    }
  };

  const cancelRecording = async () => {
    if (durationTimerRef.current) clearInterval(durationTimerRef.current);

    if (Platform.OS === 'web') {
      if (mediaRecorderRef.current && mediaRecorderRef.current.state !== 'inactive') {
        mediaRecorderRef.current.stop();
      }
      if (streamRef.current) {
        streamRef.current.getTracks().forEach((track) => track.stop());
      }
      mediaRecorderRef.current = null;
      streamRef.current = null;
      audioChunksRef.current = [];
    } else {
      if (recordingRef.current) {
        try {
          await recordingRef.current.stopAndUnloadAsync();
        } catch (_) {}
        recordingRef.current = null;
      }
    }

    setIsRecording(false);
    setRecordingDuration(0);
  };

  const stopAndSendRecording = async () => {
    if (durationTimerRef.current) clearInterval(durationTimerRef.current);

    const finalDuration = Math.max(1, recordingDuration);

    if (Platform.OS === 'web') {
      if (mediaRecorderRef.current && mediaRecorderRef.current.state !== 'inactive') {
        mediaRecorderRef.current.onstop = () => {
          const audioBlob = new Blob(audioChunksRef.current, { type: 'audio/webm' });
          const audioUrl = URL.createObjectURL(audioBlob);
          onSendVoice(audioUrl, finalDuration);

          if (streamRef.current) {
            streamRef.current.getTracks().forEach((track) => track.stop());
          }
          mediaRecorderRef.current = null;
          streamRef.current = null;
          audioChunksRef.current = [];
        };
        mediaRecorderRef.current.stop();
      } else {
        onSendVoice(undefined, finalDuration);
      }
    } else {
      if (recordingRef.current) {
        try {
          await recordingRef.current.stopAndUnloadAsync();
          const uri = recordingRef.current.getURI();
          onSendVoice(uri || undefined, finalDuration);
        } catch (err) {
          console.warn('Stop recording error:', err);
          onSendVoice(undefined, finalDuration);
        }
        recordingRef.current = null;
      } else {
        onSendVoice(undefined, finalDuration);
      }
    }

    setIsRecording(false);
    setRecordingDuration(0);
  };

  const handleSendText = () => {
    if (!text.trim()) return;
    onSendMessage(text.trim());
    setText('');
  };

  const formatTimer = (secs: number) => {
    const mins = Math.floor(secs / 60);
    const rem = secs % 60;
    return `${mins}:${rem < 10 ? '0' : ''}${rem}`;
  };

  return (
    <View style={styles.container}>
      {isRecording ? (
        <View style={styles.recordingBar}>
          <View style={styles.recordingLeft}>
            <View style={styles.recordingDot} />
            <Text style={styles.recordingText}>Запис аудіо...</Text>
            <Text style={styles.recordingTimer}>{formatTimer(recordingDuration)}</Text>
          </View>

          <View style={styles.recordingActions}>
            <TouchableOpacity style={styles.cancelRecordBtn} onPress={cancelRecording} activeOpacity={0.8}>
              <Ionicons name="trash-outline" size={18} color={colors.danger} />
            </TouchableOpacity>

            <TouchableOpacity style={styles.sendRecordBtn} onPress={stopAndSendRecording} activeOpacity={0.8}>
              <Ionicons name="checkmark" size={20} color="#000000" />
            </TouchableOpacity>
          </View>
        </View>
      ) : (
        <>
          <TouchableOpacity
            style={styles.iconButton}
            onPress={() => setIsAttachModalVisible(true)}
            activeOpacity={0.7}
          >
            <Ionicons name="attach-outline" size={22} color={colors.primary} />
          </TouchableOpacity>

          <TextInput
            style={styles.input}
            placeholder="Повідомлення..."
            placeholderTextColor={colors.textDim}
            value={text}
            onChangeText={setText}
            multiline
            maxLength={500}
          />

          {text.trim().length > 0 ? (
            <TouchableOpacity style={styles.sendButton} onPress={handleSendText} activeOpacity={0.8}>
              <Ionicons name="send" size={17} color="#000000" />
            </TouchableOpacity>
          ) : (
            <TouchableOpacity style={styles.micButton} onPress={startRecording} activeOpacity={0.8}>
              <Ionicons name="mic" size={18} color={colors.primary} />
            </TouchableOpacity>
          )}
        </>
      )}

      <Modal
        visible={isAttachModalVisible}
        transparent
        animationType="fade"
        onRequestClose={() => setIsAttachModalVisible(false)}
      >
        <TouchableOpacity
          style={styles.modalOverlay}
          activeOpacity={1}
          onPress={() => setIsAttachModalVisible(false)}
        >
          <View style={styles.modalContent}>
            <Text style={styles.modalTitle}>Прикріпити вкладення</Text>

            <TouchableOpacity style={styles.modalOption} onPress={handlePickImage} activeOpacity={0.7}>
              <View style={[styles.modalOptionIcon, { backgroundColor: 'rgba(34, 211, 238, 0.15)' }]}>
                <Ionicons name="images-outline" size={20} color={colors.primary} />
              </View>
              <View style={styles.modalOptionTextWrapper}>
                <Text style={styles.modalOptionLabel}>Фото з галереї</Text>
                <Text style={styles.modalOptionSub}>Вибрати зображення з пам'яті пристрою</Text>
              </View>
            </TouchableOpacity>

            <TouchableOpacity style={styles.modalOption} onPress={handleTakePhoto} activeOpacity={0.7}>
              <View style={[styles.modalOptionIcon, { backgroundColor: 'rgba(168, 85, 247, 0.15)' }]}>
                <Ionicons name="camera-outline" size={20} color={colors.accentPurple} />
              </View>
              <View style={styles.modalOptionTextWrapper}>
                <Text style={styles.modalOptionLabel}>Зробити фото</Text>
                <Text style={styles.modalOptionSub}>Відкрити камеру для швидкого знімку</Text>
              </View>
            </TouchableOpacity>

            <TouchableOpacity style={styles.modalOption} onPress={handlePickDocument} activeOpacity={0.7}>
              <View style={[styles.modalOptionIcon, { backgroundColor: 'rgba(16, 185, 129, 0.15)' }]}>
                <Ionicons name="document-text-outline" size={20} color={colors.accentEmerald} />
              </View>
              <View style={styles.modalOptionTextWrapper}>
                <Text style={styles.modalOptionLabel}>Файл або документ</Text>
                <Text style={styles.modalOptionSub}>ZIP, PDF, білд гри або текстовий файл</Text>
              </View>
            </TouchableOpacity>

            <TouchableOpacity
              style={styles.modalCancelBtn}
              onPress={() => setIsAttachModalVisible(false)}
              activeOpacity={0.8}
            >
              <Text style={styles.modalCancelText}>Скасувати</Text>
            </TouchableOpacity>
          </View>
        </TouchableOpacity>
      </Modal>
    </View>
  );
};

const styles = StyleSheet.create({
  container: {
    flexDirection: 'row',
    alignItems: 'center',
    paddingHorizontal: 12,
    paddingVertical: 10,
    backgroundColor: colors.surface,
    borderTopWidth: 1,
    borderTopColor: colors.border,
    gap: 8,
  },
  iconButton: {
    width: 38,
    height: 38,
    borderRadius: 19,
    backgroundColor: colors.surfaceCard,
    alignItems: 'center',
    justifyContent: 'center',
    borderWidth: 1,
    borderColor: colors.border,
  },
  input: {
    flex: 1,
    backgroundColor: colors.background,
    borderRadius: 20,
    paddingHorizontal: 16,
    paddingVertical: 8,
    fontSize: 14,
    color: colors.text,
    borderWidth: 1,
    borderColor: colors.border,
    maxHeight: 100,
  },
  sendButton: {
    width: 38,
    height: 38,
    borderRadius: 19,
    backgroundColor: colors.primary,
    alignItems: 'center',
    justifyContent: 'center',
    shadowColor: colors.primary,
    shadowOffset: { width: 0, height: 2 },
    shadowOpacity: 0.8,
    shadowRadius: 6,
    elevation: 3,
  },
  micButton: {
    width: 38,
    height: 38,
    borderRadius: 19,
    backgroundColor: colors.surfaceCard,
    alignItems: 'center',
    justifyContent: 'center',
    borderWidth: 1,
    borderColor: colors.borderLight,
  },
  recordingBar: {
    flex: 1,
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    backgroundColor: colors.surfaceCard,
    borderRadius: 22,
    paddingHorizontal: 14,
    paddingVertical: 6,
    borderWidth: 1,
    borderColor: 'rgba(239, 68, 68, 0.4)',
  },
  recordingLeft: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 8,
  },
  recordingDot: {
    width: 10,
    height: 10,
    borderRadius: 5,
    backgroundColor: colors.danger,
  },
  recordingText: {
    fontSize: 13,
    color: colors.text,
    fontWeight: '700',
  },
  recordingTimer: {
    fontSize: 12,
    color: colors.danger,
    fontFamily: 'monospace',
    fontWeight: '800',
  },
  recordingActions: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 10,
  },
  cancelRecordBtn: {
    width: 32,
    height: 32,
    borderRadius: 16,
    backgroundColor: 'rgba(239, 68, 68, 0.15)',
    alignItems: 'center',
    justifyContent: 'center',
  },
  sendRecordBtn: {
    width: 34,
    height: 34,
    borderRadius: 17,
    backgroundColor: colors.primary,
    alignItems: 'center',
    justifyContent: 'center',
  },
  modalOverlay: {
    flex: 1,
    backgroundColor: 'rgba(0, 0, 0, 0.7)',
    justifyContent: 'flex-end',
    padding: 16,
  },
  modalContent: {
    backgroundColor: colors.surface,
    borderRadius: 24,
    padding: 18,
    borderWidth: 1,
    borderColor: colors.border,
    gap: 12,
  },
  modalTitle: {
    fontSize: 15,
    fontWeight: '800',
    color: colors.text,
    textAlign: 'center',
    marginBottom: 6,
  },
  modalOption: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 14,
    backgroundColor: colors.surfaceCard,
    padding: 12,
    borderRadius: 16,
    borderWidth: 1,
    borderColor: colors.border,
  },
  modalOptionIcon: {
    width: 40,
    height: 40,
    borderRadius: 20,
    alignItems: 'center',
    justifyContent: 'center',
  },
  modalOptionTextWrapper: {
    flex: 1,
  },
  modalOptionLabel: {
    fontSize: 14,
    fontWeight: '700',
    color: colors.text,
    marginBottom: 2,
  },
  modalOptionSub: {
    fontSize: 11,
    color: colors.textDim,
  },
  modalCancelBtn: {
    marginTop: 4,
    paddingVertical: 12,
    alignItems: 'center',
    borderRadius: 14,
    backgroundColor: colors.surfaceCardLight,
  },
  modalCancelText: {
    fontSize: 13,
    fontWeight: '700',
    color: colors.textMuted,
  },
});
