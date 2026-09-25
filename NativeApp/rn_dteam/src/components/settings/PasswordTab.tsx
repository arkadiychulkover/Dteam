import React, { useState } from 'react';
import { View, Text, StyleSheet, TextInput, TouchableOpacity, Alert } from 'react-native';
import { colors } from '../../theme/colors';
import { Ionicons } from '@expo/vector-icons';

export const PasswordTab: React.FC = () => {
  const [oldPassword, setOldPassword] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [showOld, setShowOld] = useState(false);
  const [showNew, setShowNew] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [errorMessage, setErrorMessage] = useState('');

  const hasMinLength = newPassword.length >= 7;
  const hasLetter = /[a-zA-Z]/.test(newPassword);
  const hasDigit = /\d/.test(newPassword);
  const hasNoSpaces = !/\s/.test(newPassword);
  const isMatch = newPassword.length > 0 && newPassword === confirmPassword;

  const handleSave = async () => {
    if (!oldPassword.trim()) {
      setErrorMessage('Введіть поточний пароль');
      return;
    }
    if (!hasMinLength || !hasLetter || !hasDigit || !hasNoSpaces) {
      setErrorMessage('Новий пароль не відповідає всім вимогам безпеки');
      return;
    }
    if (!isMatch) {
      setErrorMessage('Нові паролі не співпадають');
      return;
    }

    setErrorMessage('');
    setIsSubmitting(true);

    setTimeout(() => {
      setIsSubmitting(false);
      Alert.alert('Успішно', 'Ваш пароль успішно оновлено!');
      setOldPassword('');
      setNewPassword('');
      setConfirmPassword('');
    }, 600);
  };

  return (
    <View style={styles.container}>
      <View style={styles.header}>
        <Text style={styles.title}>Зміна пароля</Text>
        <Text style={styles.subtitle}>Оновіть пароль для надійного захисту вашого акаунта</Text>
      </View>

      {errorMessage ? (
        <View style={styles.errorBox}>
          <Ionicons name="warning-outline" size={16} color={colors.accentRose} />
          <Text style={styles.errorText}>{errorMessage}</Text>
        </View>
      ) : null}

      <View style={styles.inputGroup}>
        <Text style={styles.inputLabel}>ПОТОЧНИЙ ПАРОЛЬ</Text>
        <View style={styles.inputWrapper}>
          <TextInput
            style={styles.input}
            secureTextEntry={!showOld}
            value={oldPassword}
            onChangeText={setOldPassword}
            placeholder="Введіть поточний пароль..."
            placeholderTextColor={colors.textDim}
          />
          <TouchableOpacity onPress={() => setShowOld(!showOld)} style={styles.eyeBtn}>
            <Ionicons name={showOld ? 'eye-off-outline' : 'eye-outline'} size={18} color={colors.textDim} />
          </TouchableOpacity>
        </View>
      </View>

      <View style={styles.inputGroup}>
        <Text style={styles.inputLabel}>НОВИЙ ПАРОЛЬ</Text>
        <View style={styles.inputWrapper}>
          <TextInput
            style={styles.input}
            secureTextEntry={!showNew}
            value={newPassword}
            onChangeText={setNewPassword}
            placeholder="Мінімум 7 символів..."
            placeholderTextColor={colors.textDim}
          />
          <TouchableOpacity onPress={() => setShowNew(!showNew)} style={styles.eyeBtn}>
            <Ionicons name={showNew ? 'eye-off-outline' : 'eye-outline'} size={18} color={colors.textDim} />
          </TouchableOpacity>
        </View>
      </View>

      <View style={styles.inputGroup}>
        <Text style={styles.inputLabel}>ПІДТВЕРДЖЕННЯ ПАРОЛЯ</Text>
        <View style={styles.inputWrapper}>
          <TextInput
            style={styles.input}
            secureTextEntry={!showNew}
            value={confirmPassword}
            onChangeText={setConfirmPassword}
            placeholder="Повторіть новий пароль..."
            placeholderTextColor={colors.textDim}
          />
        </View>
      </View>

      <View style={styles.checklistCard}>
        <Text style={styles.checklistTitle}>Вимоги до пароля:</Text>
        <View style={styles.checkItem}>
          <Ionicons
            name={hasMinLength ? 'checkmark-circle' : 'ellipse-outline'}
            size={14}
            color={hasMinLength ? colors.accentEmerald : colors.textDim}
          />
          <Text style={[styles.checkText, hasMinLength && styles.checkTextDone]}>
            Щонайменше 7 символів
          </Text>
        </View>
        <View style={styles.checkItem}>
          <Ionicons
            name={hasLetter ? 'checkmark-circle' : 'ellipse-outline'}
            size={14}
            color={hasLetter ? colors.accentEmerald : colors.textDim}
          />
          <Text style={[styles.checkText, hasLetter && styles.checkTextDone]}>
            Містить хоча б одну літеру
          </Text>
        </View>
        <View style={styles.checkItem}>
          <Ionicons
            name={hasDigit ? 'checkmark-circle' : 'ellipse-outline'}
            size={14}
            color={hasDigit ? colors.accentEmerald : colors.textDim}
          />
          <Text style={[styles.checkText, hasDigit && styles.checkTextDone]}>
            Містить хоча б одну цифру
          </Text>
        </View>
        <View style={styles.checkItem}>
          <Ionicons
            name={hasNoSpaces ? 'checkmark-circle' : 'ellipse-outline'}
            size={14}
            color={hasNoSpaces ? colors.accentEmerald : colors.textDim}
          />
          <Text style={[styles.checkText, hasNoSpaces && styles.checkTextDone]}>
            Без пробілів
          </Text>
        </View>
        <View style={styles.checkItem}>
          <Ionicons
            name={isMatch ? 'checkmark-circle' : 'ellipse-outline'}
            size={14}
            color={isMatch ? colors.accentEmerald : colors.textDim}
          />
          <Text style={[styles.checkText, isMatch && styles.checkTextDone]}>
            Паролі співпадають
          </Text>
        </View>
      </View>

      <TouchableOpacity
        style={styles.saveBtn}
        onPress={handleSave}
        disabled={isSubmitting}
        activeOpacity={0.8}
      >
        <Text style={styles.saveBtnText}>
          {isSubmitting ? 'Збереження...' : 'Змінити пароль'}
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
  errorBox: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 8,
    backgroundColor: 'rgba(244, 63, 94, 0.1)',
    borderWidth: 1,
    borderColor: 'rgba(244, 63, 94, 0.3)',
    borderRadius: 12,
    padding: 12,
    marginBottom: 16,
  },
  errorText: {
    fontSize: 12,
    color: colors.accentRose,
    fontWeight: '600',
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
  inputWrapper: {
    position: 'relative',
  },
  input: {
    backgroundColor: colors.surfaceCard,
    borderRadius: 14,
    paddingHorizontal: 16,
    paddingVertical: 12,
    paddingRight: 48,
    fontSize: 14,
    color: colors.text,
    borderWidth: 1,
    borderColor: colors.border,
  },
  eyeBtn: {
    position: 'absolute',
    right: 14,
    top: 14,
  },
  checklistCard: {
    backgroundColor: colors.surfaceCard,
    borderRadius: 14,
    padding: 14,
    marginBottom: 24,
    borderWidth: 1,
    borderColor: colors.border,
    gap: 8,
  },
  checklistTitle: {
    fontSize: 12,
    fontWeight: '700',
    color: colors.textSecondary,
    marginBottom: 4,
  },
  checkItem: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 8,
  },
  checkText: {
    fontSize: 12,
    color: colors.textDim,
  },
  checkTextDone: {
    color: colors.text,
    fontWeight: '600',
  },
  saveBtn: {
    backgroundColor: colors.primary,
    borderRadius: 14,
    paddingVertical: 14,
    alignItems: 'center',
    justifyContent: 'center',
  },
  saveBtnText: {
    color: '#021118',
    fontWeight: '800',
    fontSize: 14,
  },
});
