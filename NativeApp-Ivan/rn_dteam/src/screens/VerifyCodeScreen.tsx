import React, { useState, useEffect } from 'react';
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

interface VerifyCodeScreenProps {
  navigation?: any;
  route?: { params?: { email?: string; code?: string } };
  email?: string;
  onSuccessReset?: () => void;
  onNavigateLogin?: () => void;
}

export const VerifyCodeScreen: React.FC<VerifyCodeScreenProps> = ({
  navigation,
  route,
  email: propEmail,
  onSuccessReset,
  onNavigateLogin,
}) => {
  const email = route?.params?.email || propEmail || '';
  const initialCode = route?.params?.code || '';

  const [step, setStep] = useState<'verify' | 'reset'>('verify');
  const [resetCode, setResetCode] = useState(initialCode);
  const [resetToken, setResetToken] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [errorMessage, setErrorMessage] = useState('');

  const [resendCountdown, setResendCountdown] = useState(60);

  useEffect(() => {
    let interval: any = null;
    if (resendCountdown > 0) {
      interval = setInterval(() => {
        setResendCountdown((prev) => prev - 1);
      }, 1000);
    }
    return () => {
      if (interval) clearInterval(interval);
    };
  }, [resendCountdown]);

  const handleResend = async () => {
    if (resendCountdown > 0 || !email) return;
    try {
      await authService.forgotPassword(email);
      setResendCountdown(60);
      Alert.alert('Код надіслано', 'Новий код підтвердження надіслано на вашу пошту.');
    } catch (err: any) {
      setErrorMessage(err.message || 'Не вдалося повторно надіслати код.');
    }
  };

  const handleVerifyCode = async () => {
    const trimmed = resetCode.trim();
    if (!trimmed) {
      setErrorMessage('Введіть код підтвердження (GUID)');
      return;
    }

    setErrorMessage('');
    setIsSubmitting(true);

    try {
      const res = await authService.verifyResetCode(email, trimmed);
      setResetToken(res.resetToken);
      setStep('reset');
    } catch (err: any) {
      setErrorMessage(err.message || 'Невірний або застарілий код підтвердження.');
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleResetPassword = async () => {
    if (!newPassword || !confirmPassword) {
      setErrorMessage('Заповніть обидва поля пароля');
      return;
    }

    if (newPassword !== confirmPassword) {
      setErrorMessage('Паролі не співпадають');
      return;
    }

    if (newPassword.length < 6) {
      setErrorMessage('Пароль має бути не менше 6 символів');
      return;
    }

    setErrorMessage('');
    setIsSubmitting(true);

    try {
      await authService.resetPassword(resetToken, newPassword);
      Alert.alert('Пароль змінено', 'Ваш пароль успішно оновлено. Увійдіть з новим паролем.', [
        {
          text: 'ОК',
          onPress: () => goToLogin(),
        },
      ]);
    } catch (err: any) {
      setErrorMessage(err.message || 'Не вдалося оновити пароль.');
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
            {step === 'verify' ? (
              <>
                <View style={styles.header}>
                  <View style={styles.logoBadge}>
                    <Ionicons name="shield-checkmark-outline" size={28} color={theme.colors.primary} />
                  </View>
                  <Text style={styles.title}>Підтвердження коду</Text>
                  <Text style={styles.subtitle}>
                    Введіть код підтвердження з листа{email ? ` на ${email}` : ''}
                  </Text>
                </View>

                {errorMessage ? (
                  <View style={styles.errorBanner}>
                    <Ionicons name="warning-outline" size={18} color={theme.colors.error} />
                    <Text style={styles.errorText}>{errorMessage}</Text>
                  </View>
                ) : null}

                <View style={styles.formGroup}>
                  <Text style={styles.label}>КОД ПІДТВЕРДЖЕННЯ</Text>
                  <View style={styles.inputWrapper}>
                    <Ionicons
                      name="key-outline"
                      size={18}
                      color={theme.colors.textMuted}
                      style={styles.inputIcon}
                    />
                    <TextInput
                      style={[styles.input, styles.monoInput]}
                      value={resetCode}
                      onChangeText={setResetCode}
                      placeholder="3fa85f64-5717-4562-b3fc-2c963f66afa6"
                      placeholderTextColor={theme.colors.textDim}
                      autoCapitalize="none"
                    />
                  </View>
                </View>

                <View style={styles.resendContainer}>
                  {resendCountdown > 0 ? (
                    <Text style={styles.resendTimerText}>
                      Надіслати повторно через{' '}
                      <Text style={styles.resendSecondsText}>{resendCountdown}с</Text>
                    </Text>
                  ) : (
                    <TouchableOpacity
                      style={styles.resendButton}
                      onPress={handleResend}
                      activeOpacity={0.7}
                    >
                      <Ionicons name="refresh-outline" size={15} color={theme.colors.primary} />
                      <Text style={styles.resendButtonText}>Надіслати код повторно</Text>
                    </TouchableOpacity>
                  )}
                </View>

                <TouchableOpacity
                  style={[styles.submitButton, isSubmitting && styles.disabledButton]}
                  onPress={handleVerifyCode}
                  disabled={isSubmitting}
                  activeOpacity={0.8}
                >
                  {isSubmitting ? (
                    <ActivityIndicator size="small" color="#000" />
                  ) : (
                    <>
                      <Ionicons name="checkmark-circle-outline" size={18} color="#000" />
                      <Text style={styles.submitButtonText}>Підтвердити код</Text>
                    </>
                  )}
                </TouchableOpacity>
              </>
            ) : (
              <>
                <View style={styles.header}>
                  <View style={styles.logoBadge}>
                    <Ionicons name="lock-closed-outline" size={28} color={theme.colors.primary} />
                  </View>
                  <Text style={styles.title}>Новий пароль</Text>
                  <Text style={styles.subtitle}>
                    Придумайте надійний пароль для вашого облікового запису
                  </Text>
                </View>

                {errorMessage ? (
                  <View style={styles.errorBanner}>
                    <Ionicons name="warning-outline" size={18} color={theme.colors.error} />
                    <Text style={styles.errorText}>{errorMessage}</Text>
                  </View>
                ) : null}

                <View style={styles.formGroup}>
                  <Text style={styles.label}>НОВИЙ ПАРОЛЬ</Text>
                  <View style={styles.inputWrapper}>
                    <Ionicons
                      name="lock-closed-outline"
                      size={18}
                      color={theme.colors.textMuted}
                      style={styles.inputIcon}
                    />
                    <TextInput
                      style={[styles.input, styles.passwordInput]}
                      value={newPassword}
                      onChangeText={setNewPassword}
                      placeholder="••••••••••••"
                      placeholderTextColor={theme.colors.textDim}
                      secureTextEntry={!showPassword}
                      autoCapitalize="none"
                    />
                    <TouchableOpacity
                      onPress={() => setShowPassword(!showPassword)}
                      style={styles.eyeButton}
                      activeOpacity={0.7}
                    >
                      <Ionicons
                        name={showPassword ? 'eye-off-outline' : 'eye-outline'}
                        size={18}
                        color={theme.colors.textMuted}
                      />
                    </TouchableOpacity>
                  </View>
                </View>

                <View style={styles.formGroup}>
                  <Text style={styles.label}>ПІДТВЕРДІТЬ НОВИЙ ПАРОЛЬ</Text>
                  <View style={styles.inputWrapper}>
                    <Ionicons
                      name="lock-closed-outline"
                      size={18}
                      color={theme.colors.textMuted}
                      style={styles.inputIcon}
                    />
                    <TextInput
                      style={styles.input}
                      value={confirmPassword}
                      onChangeText={setConfirmPassword}
                      placeholder="••••••••••••"
                      placeholderTextColor={theme.colors.textDim}
                      secureTextEntry={!showPassword}
                      autoCapitalize="none"
                    />
                  </View>
                </View>

                <TouchableOpacity
                  style={[styles.submitButton, isSubmitting && styles.disabledButton]}
                  onPress={handleResetPassword}
                  disabled={isSubmitting}
                  activeOpacity={0.8}
                >
                  {isSubmitting ? (
                    <ActivityIndicator size="small" color="#000" />
                  ) : (
                    <>
                      <Ionicons name="checkmark-circle-outline" size={18} color="#000" />
                      <Text style={styles.submitButtonText}>Зберегти пароль</Text>
                    </>
                  )}
                </TouchableOpacity>
              </>
            )}

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
    fontSize: 22,
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
    marginBottom: 16,
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
  monoInput: {
    fontFamily: Platform.OS === 'ios' ? 'Courier' : 'monospace',
    fontSize: 13,
    color: theme.colors.primary,
  },
  passwordInput: {
    paddingRight: 36,
  },
  eyeButton: {
    position: 'absolute',
    right: 12,
    padding: 4,
  },
  resendContainer: {
    alignItems: 'center',
    marginVertical: 12,
  },
  resendTimerText: {
    fontSize: 12,
    color: theme.colors.textDim,
  },
  resendSecondsText: {
    color: theme.colors.primary,
    fontWeight: '800',
    fontFamily: Platform.OS === 'ios' ? 'Courier' : 'monospace',
  },
  resendButton: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 6,
    paddingVertical: 6,
    paddingHorizontal: 12,
  },
  resendButtonText: {
    fontSize: 12,
    color: theme.colors.primary,
    fontWeight: '700',
  },
  submitButton: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'center',
    gap: 8,
    backgroundColor: theme.colors.primary,
    paddingVertical: 14,
    borderRadius: theme.borderRadius.md,
    marginTop: 8,
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
