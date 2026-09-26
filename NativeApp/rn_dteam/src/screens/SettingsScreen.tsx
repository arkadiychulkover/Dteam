import React, { useState } from 'react';
import {
  View,
  Text,
  StyleSheet,
  ScrollView,
  TouchableOpacity,
  StatusBar,
} from 'react-native';
import { colors } from '../theme/colors';
import { useAuthStore } from '../store/useAuthStore';
import { AuthRequiredView } from '../components/common/AuthRequiredView';
import { SettingsTabSelector, SettingsTabType } from '../components/settings/SettingsTabSelector';
import { GeneralTab } from '../components/settings/GeneralTab';
import { PasswordTab } from '../components/settings/PasswordTab';
import { NotificationsTab } from '../components/settings/NotificationsTab';
import { WalletTab } from '../components/settings/WalletTab';
import { DeleteAccountTab } from '../components/settings/DeleteAccountTab';
import { Ionicons } from '@expo/vector-icons';

interface SettingsScreenProps {
  onBack?: () => void;
  navigation?: any;
}

export const SettingsScreen: React.FC<SettingsScreenProps> = ({
  onBack,
  navigation,
}) => {
  const { isAuthenticated } = useAuthStore();
  const [activeTab, setActiveTab] = useState<SettingsTabType>('general');

  if (!isAuthenticated) {
    return (
      <AuthRequiredView
        screenTitle="Налаштування"
        screenSubtitle="Керування обліковим записом"
        icon="settings-outline"
        title="Потрібна авторизація"
        description="Увійдіть у ваш акаунт DTEAM, щоб переглядати та змінювати налаштування профілю."
        onLogin={() => {
          if (navigation?.navigate) {
            navigation.navigate('Login');
          }
        }}
      />
    );
  }

  return (
    <View style={styles.container}>
      <StatusBar barStyle="light-content" backgroundColor={colors.background} />

      <View style={styles.header}>
        {onBack && (
          <TouchableOpacity style={styles.backBtn} onPress={onBack} activeOpacity={0.7}>
            <Ionicons name="arrow-back" size={20} color={colors.primary} />
          </TouchableOpacity>
        )}
        <View style={styles.headerTitleBlock}>
          <Text style={styles.headerTitle}>Налаштування</Text>
          <Text style={styles.headerSubtitle}>Керування вашим обліковим записом</Text>
        </View>
      </View>

      <SettingsTabSelector activeTab={activeTab} onSelectTab={setActiveTab} />

      <ScrollView
        showsVerticalScrollIndicator={false}
        contentContainerStyle={styles.scrollContent}
      >
        {activeTab === 'general' && <GeneralTab />}

        {activeTab === 'password' && <PasswordTab />}

        {activeTab === 'notifications' && <NotificationsTab />}

        {activeTab === 'wallet' && <WalletTab />}

        {activeTab === 'delete' && <DeleteAccountTab />}
      </ScrollView>
    </View>
  );
};

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: colors.background,
  },
  header: {
    flexDirection: 'row',
    alignItems: 'center',
    paddingHorizontal: 16,
    paddingTop: 12,
    paddingBottom: 8,
    gap: 12,
    borderBottomWidth: 1,
    borderBottomColor: colors.border,
  },
  backBtn: {
    width: 38,
    height: 38,
    borderRadius: 12,
    backgroundColor: colors.surfaceCard,
    alignItems: 'center',
    justifyContent: 'center',
    borderWidth: 1,
    borderColor: colors.border,
  },
  headerTitleBlock: {
    flex: 1,
  },
  headerTitle: {
    fontSize: 18,
    fontWeight: '900',
    color: colors.text,
  },
  headerSubtitle: {
    fontSize: 11,
    color: colors.textDim,
    marginTop: 1,
  },
  scrollContent: {
    paddingBottom: 40,
  },
});
