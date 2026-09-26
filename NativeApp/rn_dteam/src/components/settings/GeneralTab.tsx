import React, { useState } from 'react';
import { View, Text, StyleSheet, TextInput, TouchableOpacity, Alert, ActivityIndicator } from 'react-native';
import { colors } from '../../theme/colors';
import { BackendImage } from '../BackendImage';
import { useAuthStore } from '../../store/useAuthStore';
import { api } from '../../services/api';
import { Ionicons } from '@expo/vector-icons';

interface GeneralTabProps {
  onSuccess?: () => void;
}

export const GeneralTab: React.FC<GeneralTabProps> = ({ onSuccess }) => {
  const { user, checkAuth } = useAuthStore();
  const [bio, setBio] = useState(user?.bio || '');
  const [isSaving, setIsSaving] = useState(false);
  const [isSaved, setIsSaved] = useState(false);

  const username = user?.username || 'Користувач';
  const email = user?.email || '';
  const walletAddress = user?.hardhatAddress || user?.walletAddress || 'Не прив\'язано';

  const handleSave = async () => {
    setIsSaving(true);
    try {
      await api.put('/users/me', { bio });
      await checkAuth();
      setIsSaved(true);
      setTimeout(() => setIsSaved(false), 2500);
      if (onSuccess) onSuccess();
    } catch (err: any) {
      Alert.alert('Помилка', err.message || 'Не вдалося зберегти зміни профілю.');
    } finally {
      setIsSaving(false);
    }
  };

  return (
    <View style={styles.container}>
      <View style={styles.header}>
        <Text style={styles.title}>Загальні налаштування</Text>
        <Text style={styles.subtitle}>Персоналізуйте ваш публічний профіль у мережі Dteam</Text>
      </View>

      <View style={styles.avatarSection}>
        <View style={styles.avatarWrapper}>
          <BackendImage
            src={user?.avatarUrl}
            style={styles.avatar}
            fallbackText={username.slice(0, 2).toUpperCase()}
          />
        </View>
        <View style={styles.avatarInfo}>
          <Text style={styles.avatarTitle}>{username}</Text>
          <Text style={styles.avatarSubtitle}>Справжній акаунт DTEAM</Text>
        </View>
      </View>

      <View style={styles.inputGroup}>
        <Text style={styles.inputLabel}>ВІДОБРАЖУВАНЕ ІМ'Я (USERNAME)</Text>
        <TextInput
          style={[styles.input, { opacity: 0.7 }]}
          value={username}
          editable={false}
          placeholderTextColor={colors.textDim}
        />
        <Text style={styles.inputHelper}>Логін синхронізовано з обліковим записом</Text>
      </View>

      <View style={styles.inputGroup}>
        <Text style={styles.inputLabel}>ПРО СЕБЕ (БІОГРАФІЯ)</Text>
        <TextInput
          style={[styles.input, styles.textArea]}
          value={bio}
          onChangeText={setBio}
          placeholder="Розкажіть трохи про себе..."
          placeholderTextColor={colors.textDim}
          multiline
          numberOfLines={3}
        />
      </View>

      <View style={styles.infoCard}>
        <View style={styles.infoRow}>
          <Text style={styles.infoKey}>Email:</Text>
          <Text style={styles.infoValue}>{email || 'Не вказано'}</Text>
        </View>
        <View style={styles.infoRow}>
          <Text style={styles.infoKey}>Web3 адреса:</Text>
          <Text style={styles.infoCode} numberOfLines={1}>{walletAddress}</Text>
        </View>
      </View>

      <TouchableOpacity
        style={[styles.saveButton, isSaved && styles.saveButtonSuccess]}
        onPress={handleSave}
        disabled={isSaving}
        activeOpacity={0.8}
      >
        {isSaving ? (
          <ActivityIndicator size="small" color="#000000" />
        ) : (
          <>
            <Ionicons name={isSaved ? 'checkmark' : 'save-outline'} size={18} color="#000000" />
            <Text style={styles.saveButtonText}>
              {isSaved ? 'Збережено на сервері!' : 'Зберегти зміни'}
            </Text>
          </>
        )}
      </TouchableOpacity>
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
  avatarSection: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 16,
    backgroundColor: colors.surfaceCard,
    padding: 14,
    borderRadius: 16,
    marginBottom: 20,
    borderWidth: 1,
    borderColor: colors.border,
  },
  avatarWrapper: {
    position: 'relative',
  },
  avatar: {
    width: 60,
    height: 60,
    borderRadius: 30,
    borderWidth: 2,
    borderColor: colors.primary,
  },
  changeAvatarBtn: {
    position: 'absolute',
    bottom: -2,
    right: -2,
    backgroundColor: colors.primary,
    width: 24,
    height: 24,
    borderRadius: 12,
    alignItems: 'center',
    justifyContent: 'center',
  },
  avatarInfo: {
    flex: 1,
  },
  avatarTitle: {
    fontSize: 14,
    fontWeight: '700',
    color: colors.text,
    marginBottom: 2,
  },
  avatarSubtitle: {
    fontSize: 11,
    color: colors.textDim,
  },
  inputGroup: {
    marginBottom: 16,
  },
  inputLabel: {
    fontSize: 11,
    fontWeight: '700',
    color: colors.textSecondary,
    marginBottom: 8,
    fontFamily: 'monospace',
    letterSpacing: 0.5,
  },
  input: {
    backgroundColor: colors.surfaceCard,
    borderRadius: 14,
    paddingHorizontal: 16,
    paddingVertical: 12,
    fontSize: 14,
    color: colors.text,
    borderWidth: 1,
    borderColor: colors.border,
  },
  textArea: {
    minHeight: 80,
    textAlignVertical: 'top',
  },
  inputHelper: {
    fontSize: 11,
    color: colors.textDim,
    marginTop: 4,
    marginLeft: 2,
  },
  infoCard: {
    backgroundColor: colors.background,
    borderRadius: 14,
    padding: 14,
    marginBottom: 24,
    borderWidth: 1,
    borderColor: colors.border,
    gap: 8,
  },
  infoRow: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
  },
  infoKey: {
    fontSize: 12,
    color: colors.textMuted,
  },
  infoValue: {
    fontSize: 12,
    color: colors.text,
    fontWeight: '600',
  },
  infoCode: {
    fontSize: 12,
    color: colors.primary,
    fontFamily: 'monospace',
  },
  saveButton: {
    backgroundColor: colors.primary,
    borderRadius: 14,
    paddingVertical: 14,
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'center',
    gap: 8,
    shadowColor: colors.primary,
    shadowOffset: { width: 0, height: 4 },
    shadowOpacity: 0.5,
    shadowRadius: 8,
    elevation: 4,
  },
  saveButtonSuccess: {
    backgroundColor: colors.accentEmerald,
  },
  saveButtonText: {
    color: '#000000',
    fontWeight: '800',
    fontSize: 14,
  },
});
