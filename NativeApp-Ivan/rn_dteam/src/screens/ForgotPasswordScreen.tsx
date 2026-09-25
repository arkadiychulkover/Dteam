import React, { useState } from 'react';
import {
  View,
  Text,
  TextInput,
  TouchableOpacity,
  ScrollView,
  StyleSheet,
  ActivityIndicator,
  StatusBar,
  KeyboardAvoidingView,
  Platform,
  Alert,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { Ionicons } from '@expo/vector-icons';
import { authService } from '../services/authService';
import { theme } from '../styles/theme';

interface ForgotPasswordScreenProps {
  navigation?: any;
  onNavigateVerifyCode?: (email: string) => void;
  onNavigateLogin?: () => void;
}

export const ForgotPasswordScreen: React.FC<ForgotPasswordScreenProps> = ({
  navigation,
  onNavigateVerifyCode,
  onNavigateLogin,
}) => {
  const [identifier, setIdentifier] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [errorMessage, setErrorMessage] = useState('');

  const handleSubmit = async () => {
    if (!identifier.trim()) {
      setErrorMessage('Введіть email або логін');
      return;
    }

    setErrorMessage('');
    setIsSubmitting(true);

    try {
      const res = await authService.forgotPassword(identifier);
      const targetEmail = res.userEmail || identifier.trim();

      if (res.debugCode) {
        Alert.alert(
          'Код підтвердження',
          `Для тестування використайте код підтвердження:\n\n${res.debugCode}`,
          [
            {
              text: 'Ввести код',
              onPress: () => {
                if (onNavigateVerifyCode) {
                  onNavigateVerifyCode(targetEmail);
                } else if (navigation?.navigate) {
                  navigation.navigate('VerifyCode', { email: targetEmail, code: res.debugCode });
                }
              },
            },
          ]
        );
      } else {
        if (onNavigateVerifyCode) {
          onNavigateVerifyCode(targetEmail);
        } else if (navigation?.navigate) {
          navigation.navigate('VerifyCode', { email: targetEmail });
        }
      }
    } catch (err: any) {
      setErrorMessage(err.message || 'Не вдалося надіслати код відновлення.');
    } finally {
      setIsSubmitting(false);
    }
  };

  const goToLogin = () => {
    if (onNavigateLogin) {
      onNavigateLogin();
    } else if (navigation?.navigate) {
      navigation.navigate('Login');
    }
  };

  return (
    <SafeAreaView style={styles.safeArea} edges={['top', 'left', 'right', 'bottom']}>
      <StatusBar barStyle="light-content" backgroundColor={theme.colors.background} />

      <View style={styles.topNavBar}>
        <TouchableOpacity
          style={styles.topBackBtn}
          onPress={() => (navigation?.canGoBack?.() ? navigation.goBack() : navigation?.navigate?.('Login'))}
          activeOpacity={0.7}
        >
          <Ionicons name="arrow-back" size={20} color={theme.colors.text} />
          <Text style={styles.topBackBtnText}>Назад</Text>
        </TouchableOpacity>
      </View>

      <KeyboardAvoidingView
        behavior={Platform.OS === 'ios' ? 'padding' : 'height'}
        style={styles.keyboardContainer}
      >
        <ScrollView
          showsVerticalScrollIndicator={false}
          contentContainerStyle={styles.scrollContent}
        >
          <View style={styles.cardContainer}>
            <View style={styles.header}>
              <View style={styles.logoBadge}>
                <Ionicons name="key-outline" size={28} color={theme.colors.primary} />
              </View>
              <Text style={styles.title}>Скидання пароля</Text>
              <Text style={styles.subtitle}>
                Вкажіть вашу пошту для отримання коду відновлення
              </Text>
            </View>

            {errorMessage ? (
              <View style={styles.errorBanner}>
                <Ionicons name="warning-outline" size={18} color={theme.colors.error} />
                <Text style={styles.errorText}>{errorMessage}</Text>
              </View>
            ) : null}

            <View style={styles.formGroup}>
              <Text style={styles.label}>EMAIL АБО ЛОГІН</Text>
              <View style={styles.inputWrapper}>
                <Ionicons
                  name="mail-outline"
                  size={18}
                  color={theme.colors.textMuted}
                  style={styles.inputIcon}
                />
                <TextInput
                  style={styles.input}
                  value={identifier}
                  onChangeText={setIdentifier}
                  placeholder="user@example.com або логін"
                  placeholderTextColor={theme.colors.textDim}
                  autoCapitalize="none"
                  keyboardType="email-address"
                />
              </View>
            </View>

            <TouchableOpacity
              style={[styles.submitButton, isSubmitting && styles.disabledButton]}
              onPress={handleSubmit}
              disabled={isSubmitting}
              activeOpacity={0.8}
            >
              {isSubmitting ? (
                <ActivityIndicator size="small" color="#000" />
              ) : (
                <>
                  <Ionicons name="send-outline" size={18} color="#000" />
                  <Text style={styles.submitButtonText}>Надіслати код</Text>
                </>
              )}
            </TouchableOpacity>

            <View style={styles.footer}>
              <TouchableOpacity
                style={styles.backLink}
                onPress={goToLogin}
                activeOpacity={0.7}
              >
                <Ionicons name="arrow-back" size={16} color={theme.colors.textMuted} />
                <Text style={styles.backLinkText}>Повернутися до входу</Text>
              </TouchableOpacity>
            </View>
          </View>
        </ScrollView>
      </KeyboardAvoidingView>
    </SafeAreaView>
  );
};

const styles = StyleSheet.create({
  safeArea: {
    flex: 1,
    backgroundColor: theme.colors.background,
  },
  topNavBar: {
    paddingHorizontal: 16,
    paddingVertical: 10,
    backgroundColor: theme.colors.surface,
    borderBottomWidth: 1,
    borderBottomColor: theme.colors.borderSubtle,
  },
  topBackBtn: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 6,
    paddingVertical: 4,
  },
  topBackBtnText: {
    fontSize: 14,
    fontWeight: '700',
    color: theme.colors.text,
  },
  keyboardContainer: {
    flex: 1,
  },
  scrollContent: {
    flexGrow: 1,
    paddingHorizontal: 16,
    paddingVertical: 20,
  },
  cardContainer: {
    backgroundColor: theme.colors.glassBg,
    borderRadius: theme.borderRadius.xl,
    borderWidth: 1,
    borderColor: theme.colors.border,
    padding: 24,
    shadowColor: theme.colors.primary,
    shadowOffset: { width: 0, height: 6 },
    shadowOpacity: 0.15,
    shadowRadius: 20,
    elevation: 8,
  },
  header: {
    alignItems: 'center',
    marginBottom: 24,
  },
  logoBadge: {
    width: 56,
    height: 56,
    borderRadius: 16,
    backgroundColor: 'rgba(0, 242, 254, 0.12)',
    borderWidth: 1,
    borderColor: 'rgba(0, 242, 254, 0.3)',
    alignItems: 'center',
    justifyContent: 'center',
    marginBottom: 12,
  },
  title: {
    fontSize: 24,
    fontWeight: '900',
    color: theme.colors.text,
    letterSpacing: 0.5,
  },
  subtitle: {
    fontSize: 12,
    color: theme.colors.textMuted,
    marginTop: 6,
    textAlign: 'center',
  },
  errorBanner: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 8,
    backgroundColor: 'rgba(244, 63, 94, 0.12)',
    borderWidth: 1,
    borderColor: 'rgba(244, 63, 94, 0.3)',
    borderRadius: theme.borderRadius.md,
    padding: 12,
    marginBottom: 16,
  },
  errorText: {
    fontSize: 12,
    color: theme.colors.error,
    fontWeight: '600',
    flex: 1,
  },
  formGroup: {
    marginBottom: 20,
  },
  label: {
    fontSize: 11,
    fontWeight: '800',
    color: theme.colors.textSecondary,
    marginBottom: 8,
    letterSpacing: 0.8,
  },
  inputWrapper: {
    flexDirection: 'row',
    alignItems: 'center',
    backgroundColor: theme.colors.inputBg,
    borderWidth: 1,
    borderColor: theme.colors.borderSubtle,
    borderRadius: theme.borderRadius.md,
    paddingHorizontal: 12,
  },
  inputIcon: {
    marginRight: 10,
  },
  input: {
    flex: 1,
    height: 46,
    fontSize: 14,
    color: theme.colors.text,
  },
  submitButton: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'center',
    gap: 8,
    backgroundColor: theme.colors.primary,
    paddingVertical: 14,
    borderRadius: theme.borderRadius.md,
    shadowColor: theme.colors.primary,
    shadowOffset: { width: 0, height: 4 },
    shadowOpacity: 0.25,
    shadowRadius: 10,
    elevation: 4,
  },
  disabledButton: {
    opacity: 0.6,
  },
  submitButtonText: {
    fontSize: 15,
    fontWeight: '900',
    color: '#000',
    letterSpacing: 0.5,
  },
  footer: {
    marginTop: 24,
    paddingTop: 16,
    borderTopWidth: 1,
    borderTopColor: theme.colors.borderSubtle,
    alignItems: 'center',
  },
  backLink: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 6,
  },
  backLinkText: {
    fontSize: 13,
    color: theme.colors.textMuted,
    fontWeight: '700',
  },
});
