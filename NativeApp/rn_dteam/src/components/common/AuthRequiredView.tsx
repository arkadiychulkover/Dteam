import React from 'react';
import {
  View,
  Text,
  StyleSheet,
  TouchableOpacity,
  StatusBar,
  StyleProp,
  ViewStyle,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { Ionicons } from '@expo/vector-icons';
import { theme } from '../../styles/theme';
import { colors } from '../../theme/colors';

interface AuthRequiredViewProps {
  screenTitle?: string;
  screenSubtitle?: string;
  icon?: keyof typeof Ionicons.glyphMap;
  title?: string;
  description: string;
  onLogin: () => void;
  style?: StyleProp<ViewStyle>;
  showHeader?: boolean;
}

export const AuthRequiredView: React.FC<AuthRequiredViewProps> = ({
  screenTitle,
  screenSubtitle,
  icon = 'lock-closed-outline',
  title = 'Потрібна авторизація',
  description,
  onLogin,
  style,
  showHeader = true,
}) => {
  return (
    <SafeAreaView style={[styles.safeArea, style]} edges={['top', 'left', 'right']}>
      <StatusBar barStyle="light-content" backgroundColor={colors.background} />

      {showHeader && screenTitle ? (
        <View style={styles.header}>
          <Text style={styles.headerTitle}>{screenTitle}</Text>
          {screenSubtitle ? <Text style={styles.headerSubtitle}>{screenSubtitle}</Text> : null}
        </View>
      ) : null}

      <View style={styles.contentContainer}>
        <View style={styles.card}>
          <View style={styles.iconCircle}>
            <Ionicons name={icon} size={42} color={colors.primary} />
          </View>

          <Text style={styles.title}>{title}</Text>
          <Text style={styles.description}>{description}</Text>

          <TouchableOpacity
            style={styles.loginButton}
            onPress={onLogin}
            activeOpacity={0.85}
          >
            <Ionicons name="log-in-outline" size={18} color="#021118" />
            <Text style={styles.loginButtonText}>Увійти у DTEAM</Text>
          </TouchableOpacity>
        </View>
      </View>
    </SafeAreaView>
  );
};

const styles = StyleSheet.create({
  safeArea: {
    flex: 1,
    backgroundColor: colors.background,
  },
  header: {
    paddingHorizontal: 16,
    paddingTop: 12,
    paddingBottom: 16,
    borderBottomWidth: 1,
    borderBottomColor: colors.border,
  },
  headerTitle: {
    fontSize: 22,
    fontWeight: '900',
    color: colors.text,
    letterSpacing: 0.3,
  },
  headerSubtitle: {
    fontSize: 12,
    color: colors.textMuted,
    marginTop: 2,
  },
  contentContainer: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
    paddingHorizontal: 20,
  },
  card: {
    width: '100%',
    maxWidth: 360,
    backgroundColor: colors.surfaceCard,
    borderRadius: 24,
    padding: 28,
    alignItems: 'center',
    borderWidth: 1,
    borderColor: 'rgba(34, 211, 238, 0.2)',
    shadowColor: colors.primary,
    shadowOffset: { width: 0, height: 8 },
    shadowOpacity: 0.1,
    shadowRadius: 20,
  },
  iconCircle: {
    width: 80,
    height: 80,
    borderRadius: 40,
    backgroundColor: 'rgba(34, 211, 238, 0.1)',
    borderWidth: 1.5,
    borderColor: 'rgba(34, 211, 238, 0.3)',
    justifyContent: 'center',
    alignItems: 'center',
    marginBottom: 20,
  },
  title: {
    fontSize: 20,
    fontWeight: '900',
    color: colors.text,
    textAlign: 'center',
    marginBottom: 10,
    letterSpacing: 0.2,
  },
  description: {
    fontSize: 13,
    color: colors.textMuted,
    textAlign: 'center',
    lineHeight: 20,
    marginBottom: 24,
    paddingHorizontal: 8,
  },
  loginButton: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'center',
    gap: 8,
    width: '100%',
    backgroundColor: colors.primary,
    paddingVertical: 14,
    borderRadius: 14,
    shadowColor: colors.primary,
    shadowOffset: { width: 0, height: 4 },
    shadowOpacity: 0.3,
    shadowRadius: 10,
  },
  loginButtonText: {
    fontSize: 15,
    fontWeight: '900',
    color: '#021118',
    letterSpacing: 0.3,
  },
});
