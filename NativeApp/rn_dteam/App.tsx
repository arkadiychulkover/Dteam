import React, { useEffect, useState } from 'react';
import { StyleSheet, View } from 'react-native';
import { StatusBar } from 'expo-status-bar';
import { NavigationContainer, DarkTheme } from '@react-navigation/native';
import { createNativeStackNavigator } from '@react-navigation/native-stack';
import { SafeAreaProvider, SafeAreaView } from 'react-native-safe-area-context';

import { useAuthStore } from './src/store/useAuthStore';
import { tokenStorage } from './src/services/tokenStorage';
import { theme } from './src/styles/theme';
import { colors } from './src/theme/colors';

import type { Game, UserGame, Dialog, UserProfile, TabId } from './src/types';
import {
  initialProfile,
  initialDialogs,
  initialTransactions,
} from './src/data/mockData';

import { HomeScreen } from './src/screens/HomeScreen';
import { GameDetailsScreen } from './src/screens/GameDetailsScreen';
import { LibraryScreen } from './src/screens/LibraryScreen';
import { LibraryGameScreen } from './src/screens/LibraryGameScreen';
import { ChatsScreen } from './src/screens/ChatsScreen';
import { ChatDetailScreen } from './src/screens/ChatDetailScreen';
import { ProfileScreen } from './src/screens/ProfileScreen';
import { SettingsScreen } from './src/screens/SettingsScreen';
import { LoginScreen } from './src/screens/LoginScreen';
import { ForgotPasswordScreen } from './src/screens/ForgotPasswordScreen';
import { VerifyCodeScreen } from './src/screens/VerifyCodeScreen';

import { BottomTabBar } from './src/components/common/BottomTabBar';

export type RootStackParamList = {
  MainTabs: { initialTab?: TabId } | undefined;
  GameDetails: { game: Game };
  LibraryGame: { game: Game; userGame?: UserGame };
  ChatDetail: { dialog: Dialog };
  Settings: undefined;
  Login: undefined;
  ForgotPassword: undefined;
  VerifyCode: { email?: string; code?: string };
};

const Stack = createNativeStackNavigator<RootStackParamList>();

const navigationTheme = {
  ...DarkTheme,
  colors: {
    ...DarkTheme.colors,
    primary: theme.colors.primary,
    background: theme.colors.background,
    card: theme.colors.surface,
    text: theme.colors.text,
    border: theme.colors.borderSubtle,
  },
};

interface MainTabsScreenProps {
  navigation: any;
  route?: any;
  profile: UserProfile;
  dialogs: Dialog[];
  onUpdateProfile: (updated: Partial<UserProfile>) => void;
}

function MainTabsScreen({
  navigation,
  route,
  profile,
  dialogs,
}: MainTabsScreenProps) {
  const [activeTab, setActiveTab] = useState<TabId>(route?.params?.initialTab || 'store');

  const unreadCount = dialogs.reduce((sum, d) => sum + (d.unreadCount || 0), 0);

  return (
    <SafeAreaView style={styles.tabContainer} edges={['left', 'right', 'bottom']}>
      <View style={styles.tabContent}>
        {activeTab === 'store' && (
          <HomeScreen
            navigation={navigation}
            onSelectGame={(game) => navigation.navigate('GameDetails', { game })}
            onNavigateLibrary={() => setActiveTab('library')}
            onNavigateLogin={() => navigation.navigate('Login')}
          />
        )}

        {activeTab === 'library' && (
          <LibraryScreen
            navigation={navigation}
            onSelectLibraryGame={(game, userGame) =>
              navigation.navigate('LibraryGame', { game, userGame })
            }
            onNavigateCatalog={() => setActiveTab('store')}
          />
        )}

        {activeTab === 'chats' && (
          <ChatsScreen
            dialogs={dialogs}
            onSelectDialog={(dialog) => navigation.navigate('ChatDetail', { dialog })}
          />
        )}

        {activeTab === 'profile' && (
          <ProfileScreen
            profile={profile}
            onNavigateToSettings={() => navigation.navigate('Settings')}
            onNavigateToLibrary={() => setActiveTab('library')}
            onNavigateToLogin={() => navigation.navigate('Login')}
          />
        )}
      </View>

      <BottomTabBar
        activeTab={activeTab}
        onSelectTab={setActiveTab}
        unreadCount={unreadCount}
      />
    </SafeAreaView>
  );
}

export default function App() {
  const [profile, setProfile] = useState<UserProfile>(initialProfile);
  const [dialogs, setDialogs] = useState<Dialog[]>(initialDialogs);
  const [transactions] = useState(initialTransactions);

  useEffect(() => {
    tokenStorage.init().catch((err) => {
      console.warn('[App] Failed to initialize token storage:', err);
    });

    useAuthStore.getState().checkAuth().catch((err) => {
      console.warn('[App] Failed to check auth session:', err);
    });
  }, []);

  const handleUpdateProfile = (updated: Partial<UserProfile>) => {
    setProfile((prev) => ({ ...prev, ...updated }));
  };

  return (
    <SafeAreaProvider>
      <StatusBar style="light" />
      <NavigationContainer theme={navigationTheme}>
        <Stack.Navigator
          initialRouteName="MainTabs"
          screenOptions={{
            headerShown: false,
            animation: 'slide_from_right',
            contentStyle: { backgroundColor: colors.background },
          }}
        >
          <Stack.Screen name="MainTabs">
            {(props) => (
              <MainTabsScreen
                {...props}
                profile={profile}
                dialogs={dialogs}
                onUpdateProfile={handleUpdateProfile}
              />
            )}
          </Stack.Screen>

          <Stack.Screen name="GameDetails" component={GameDetailsScreen} />
          <Stack.Screen name="LibraryGame" component={LibraryGameScreen} />
          <Stack.Screen name="ChatDetail" component={ChatDetailScreen} />

          <Stack.Screen name="Settings">
            {(props) => (
              <SettingsScreen
                profile={profile}
                transactions={transactions}
                onUpdateProfile={handleUpdateProfile}
                onBack={() => props.navigation.goBack()}
              />
            )}
          </Stack.Screen>

          <Stack.Screen name="Login" component={LoginScreen} />
          <Stack.Screen name="ForgotPassword" component={ForgotPasswordScreen} />
          <Stack.Screen name="VerifyCode" component={VerifyCodeScreen} />
        </Stack.Navigator>
      </NavigationContainer>
    </SafeAreaProvider>
  );
}

const styles = StyleSheet.create({
  tabContainer: {
    flex: 1,
    backgroundColor: colors.background,
  },
  tabContent: {
    flex: 1,
  },
});
