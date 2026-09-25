import React, { useState } from 'react';
import { View, Text, StyleSheet, TextInput, TouchableOpacity, Image } from 'react-native';
import { colors } from '../../theme/colors';
import { UserProfile } from '../../types';
import { Ionicons } from '@expo/vector-icons';

interface GeneralTabProps {
  profile: UserProfile;
  onUpdateProfile: (updated: Partial<UserProfile>) => void;
}

export const GeneralTab: React.FC<GeneralTabProps> = ({ profile, onUpdateProfile }) => {
  const [displayName, setDisplayName] = useState(profile.displayName);
  const [bio, setBio] = useState(profile.bio);
  const [isSaved, setIsSaved] = useState(false);

  const handleSave = () => {
    onUpdateProfile({ displayName, bio });
    setIsSaved(true);
    setTimeout(() => setIsSaved(false), 2000);
  };

  return (
    <View style={styles.container}>
      <View style={styles.header}>
        <Text style={styles.title}>Загальні налаштування</Text>
        <Text style={styles.subtitle}>Персоналізуйте ваш публічний профіль у мережі Dteam</Text>
      </View>

      {/* Avatar Section */}
      <View style={styles.avatarSection}>
        <View style={styles.avatarWrapper}>
          <Image source={{ uri: profile.avatarUrl }} style={styles.avatar} />
          <TouchableOpacity style={styles.changeAvatarBtn} activeOpacity={0.8}>
            <Ionicons name="camera" size={14} color="#000000" />
          </TouchableOpacity>
        </View>
        <View style={styles.avatarInfo}>
          <Text style={styles.avatarTitle}>Зображення профілю</Text>
          <Text style={styles.avatarSubtitle}>PNG, JPG або WebP до 5MB</Text>
        </View>
      </View>

      {/* Display Name Input */}
      <View style={styles.inputGroup}>
        <Text style={styles.inputLabel}>ВІДОБРАЖУВАНЕ ІМ'Я</Text>
        <TextInput
          style={styles.input}
          value={displayName}
          onChangeText={setDisplayName}
          placeholder="Ваш псевдонім..."
          placeholderTextColor={colors.textDim}
        />
      </View>

      {/* Bio Input */}
      <View style={styles.inputGroup}>
        <Text style={styles.inputLabel}>ПРО СЕБЕ</Text>
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

      {/* Readonly info */}
      <View style={styles.infoCard}>
        <View style={styles.infoRow}>
          <Text style={styles.infoKey}>Email:</Text>
          <Text style={styles.infoValue}>{profile.email}</Text>
        </View>
        <View style={styles.infoRow}>
          <Text style={styles.infoKey}>Web3 адреса:</Text>
          <Text style={styles.infoCode}>{profile.walletAddress}</Text>
        </View>
      </View>

      {/* Save Button */}
      <TouchableOpacity
        style={[styles.saveButton, isSaved && styles.saveButtonSuccess]}
        onPress={handleSave}
        activeOpacity={0.8}
      >
        <Ionicons name={isSaved ? 'checkmark' : 'save-outline'} size={18} color="#000000" />
        <Text style={styles.saveButtonText}>
          {isSaved ? 'Збережено!' : 'Зберегти зміни'}
        </Text>
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
